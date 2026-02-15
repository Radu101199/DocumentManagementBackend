using DocumentManagementBackend.Domain.Enums;

namespace DocumentManagementBackend.Domain.Entities;

public class Document : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    
    // Foreign key
    public Guid OwnerId { get; set; }
    
    // Navigation property
    public User Owner { get; set; } = null!;
}