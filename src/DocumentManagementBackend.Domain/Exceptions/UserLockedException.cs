namespace DocumentManagementBackend.Domain.Exceptions;

public class UserLockedException : DomainException
{
    public Guid UserId { get; }

    public UserLockedException(Guid userId)
        : base($"User '{userId}' is locked and cannot perform this action")
    {
        UserId = userId;
    }
}