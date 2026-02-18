using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.ValueObjects;

namespace DocumentManagementBackend.Domain.Entities;

public class User : BaseAuditableEntity
{
    // ✅ Private setters
    public Guid Id { get; private set; }
    public Email Email { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; } = UserStatus.Active;
    
    private readonly List<UserRole> _roles = new();
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    
    // Navigation property
    public ICollection<Document> Documents { get; private set; } = new List<Document>();

    // Full name computed property
    public string FullName => $"{FirstName} {LastName}";

    // ✅ Constructor privat
    private User() { }

    // ✅ Factory method
    public static User Create(
        Email email,
        string firstName,
        string lastName,
        string passwordHash,
        UserRole initialRole = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            Status = UserStatus.Active
        };

        user._roles.Add(initialRole);

        return user;
    }

    // ✅ Metode publice controlate
    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");

        FirstName = firstName;
        LastName = lastName;
    }

    public void ChangeEmail(Email newEmail)
    {
        Email = newEmail;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (Status == UserStatus.Locked)
            throw new UserLockedException(Id);

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required");

        PasswordHash = newPasswordHash;
    }

    public void Lock(string reason)
    {
        if (Status == UserStatus.Locked)
            throw new DomainException($"User {Id} is already locked");

        Status = UserStatus.Locked;
    }

    public void Unlock()
    {
        if (Status != UserStatus.Locked)
            throw new DomainException($"User {Id} is not locked");

        Status = UserStatus.Active;
    }

    public void Suspend()
    {
        if (Status == UserStatus.Suspended)
            throw new DomainException($"User {Id} is already suspended");

        Status = UserStatus.Suspended;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void AddRole(UserRole role)
    {
        if (_roles.Contains(role))
            throw new DomainException($"User already has role {role}");

        _roles.Add(role);
    }

    public void RemoveRole(UserRole role)
    {
        if (!_roles.Contains(role))
            throw new DomainException($"User does not have role {role}");

        if (_roles.Count == 1)
            throw new DomainException("User must have at least one role");

        _roles.Remove(role);
    }

    public bool HasRole(UserRole role) => _roles.Contains(role);

    public bool CanLogin() => Status == UserStatus.Active;
}