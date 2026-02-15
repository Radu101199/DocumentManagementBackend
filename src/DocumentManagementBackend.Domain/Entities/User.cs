using DocumentManagementBackend.Domain.Enums;

namespace DocumentManagementBackend.Domain.Entities;

public class User : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    
    // Navigation property - documentele create de user
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}