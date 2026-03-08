using DocumentManagementBackend.Domain.Interfaces;

namespace DocumentManagementBackend.Domain.Entities;

public abstract class BaseAuditableEntity : IAuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    public byte[] RowVersion { get; set; } = null!;
}