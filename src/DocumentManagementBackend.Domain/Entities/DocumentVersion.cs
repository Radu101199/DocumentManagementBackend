using DocumentManagementBackend.Domain.Enums;

namespace DocumentManagementBackend.Domain.Entities;

public class DocumentVersion
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long FileSizeBytes { get; private set; }
    public DocumentStatus Status { get; private set; }
    public string? Comment { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation
    public Document Document { get; private set; } = null!;

    private DocumentVersion() { }

    public static DocumentVersion CreateSnapshot(
        Document document,
        int versionNumber,
        string? comment,
        string? createdBy) => new()
    {
        DocumentId = document.Id,
        VersionNumber = versionNumber,
        Title = document.Title,
        Description = document.Description,
        FileName = document.FileName,
        ContentType = document.ContentType,
        FileSizeBytes = document.FileSizeBytes,
        Status = document.Status,
        Comment = comment,
        CreatedBy = createdBy
    };
}
