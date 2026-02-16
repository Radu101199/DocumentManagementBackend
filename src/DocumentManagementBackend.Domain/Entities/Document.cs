using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Events;
using DocumentManagementBackend.Domain.Exceptions;

namespace DocumentManagementBackend.Domain.Entities;

public class Document : BaseAuditableEntity
{
    private readonly List<BaseDomainEvent> _domainEvents = new();

    // ✅ Private setters - NIMENI nu poate seta direct
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public DocumentStatus Status { get; private set; } = DocumentStatus.Draft;
    
    // Approval window tracking
    public DateTime? ApprovalRequestedAt { get; private set; }
    public DateTime? ApprovalExpiresAt { get; private set; }
    
    // Foreign key
    public Guid OwnerId { get; private set; }
    
    // Navigation property (EF Core needs setter, dar e controlat)
    public User Owner { get; private set; } = null!;

    // Domain Events (read-only collection)
    public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Business rule: Approval window is 7 days
    private const int ApprovalWindowDays = 7;

    // ✅ Constructor privat - doar factory methods pot crea documente
    private Document() { }

    // ✅ Factory method pentru creare
    public static Document Create(
        string title,
        string description,
        string fileName,
        string filePath,
        string contentType,
        long fileSizeBytes,
        Guid ownerId,
        Guid creatorId)
    {
        // Validări
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Document title is required");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name is required");

        if (fileSizeBytes <= 0)
            throw new DomainException("File size must be greater than zero");

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            FileName = fileName,
            FilePath = filePath,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            OwnerId = ownerId,
            Status = DocumentStatus.Draft
        };

        document._domainEvents.Add(new DocumentCreatedEvent(document.Id, creatorId));

        return document;
    }

    // ✅ Metode publice pentru modificări controlate
    public void UpdateMetadata(string title, string? description)
    {
        if (Status == DocumentStatus.Archived)
        {
            throw new DocumentInvalidStateException(Id, Status.ToString(), "update metadata");
        }

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Document title is required");

        Title = title;
        Description = description;
    }

    public void RequestApproval()
    {
        if (Status != DocumentStatus.Draft)
        {
            throw new InvalidStatusTransitionException(Id, Status, DocumentStatus.Published);
        }

        ApprovalRequestedAt = DateTime.UtcNow;
        ApprovalExpiresAt = DateTime.UtcNow.AddDays(ApprovalWindowDays);
    }

    public void MarkAsReviewed(Guid reviewerId, string? notes = null)
    {
        if (Status == DocumentStatus.Archived)
        {
            throw new DocumentInvalidStateException(Id, Status.ToString(), "review");
        }

        _domainEvents.Add(new DocumentReviewedEvent(Id, reviewerId, notes));
    }

    public void Approve(Guid approverId, string? notes = null)
    {
        // Check if approval window is still valid
        if (ApprovalExpiresAt.HasValue && DateTime.UtcNow > ApprovalExpiresAt.Value)
        {
            throw new ApprovalWindowExpiredException(Id, ApprovalExpiresAt.Value);
        }

        // Can only approve from Draft status
        if (Status != DocumentStatus.Draft)
        {
            throw new InvalidStatusTransitionException(Id, Status, DocumentStatus.Published);
        }

        Status = DocumentStatus.Published;
        _domainEvents.Add(new DocumentApprovedEvent(Id, approverId, notes));
    }

    public void Reject(Guid rejectorId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Rejection reason is required");
        }

        // Can only reject from Draft status during approval
        if (Status != DocumentStatus.Draft)
        {
            throw new InvalidStatusTransitionException(Id, Status, DocumentStatus.Draft);
        }

        // Clear approval window
        ApprovalRequestedAt = null;
        ApprovalExpiresAt = null;

        _domainEvents.Add(new DocumentRejectedEvent(Id, rejectorId, reason));
    }

    public void CancelApproval(Guid cancelledById, string? reason = null)
    {
        // Can only cancel if document is Published
        if (Status != DocumentStatus.Published)
        {
            throw new DocumentInvalidStateException(Id, Status.ToString(), "cancel approval");
        }

        Status = DocumentStatus.Draft;
        ApprovalRequestedAt = null;
        ApprovalExpiresAt = null;

        _domainEvents.Add(new DocumentApprovalCancelledEvent(Id, cancelledById, reason));
    }

    public void Archive()
    {
        // Can archive from any status except already archived
        if (Status == DocumentStatus.Archived)
        {
            throw new DocumentInvalidStateException(Id, Status.ToString(), "archive");
        }

        Status = DocumentStatus.Archived;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}