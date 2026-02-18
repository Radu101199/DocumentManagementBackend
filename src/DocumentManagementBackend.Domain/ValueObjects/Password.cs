using DocumentManagementBackend.Domain.Exceptions;

namespace DocumentManagementBackend.Domain.ValueObjects;

public sealed class Password
{
    public string Value { get; }

    private const int MinLength = 8;
    private const int MaxLength = 128;

    private Password(string value)
    {
        Value = value;
    }

    public static Password Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password is required");

        if (password.Length < MinLength)
            throw new DomainException($"Password must be at least {MinLength} characters");

        if (password.Length > MaxLength)
            throw new DomainException($"Password must not exceed {MaxLength} characters");

        // Policy: must contain uppercase, lowercase, digit, special char
        if (!HasUpperCase(password))
            throw new DomainException("Password must contain at least one uppercase letter");

        if (!HasLowerCase(password))
            throw new DomainException("Password must contain at least one lowercase letter");

        if (!HasDigit(password))
            throw new DomainException("Password must contain at least one digit");

        if (!HasSpecialChar(password))
            throw new DomainException("Password must contain at least one special character");

        return new Password(password);
    }

    private static bool HasUpperCase(string password) 
        => password.Any(char.IsUpper);

    private static bool HasLowerCase(string password) 
        => password.Any(char.IsLower);

    private static bool HasDigit(string password) 
        => password.Any(char.IsDigit);

    private static bool HasSpecialChar(string password) 
        => password.Any(c => !char.IsLetterOrDigit(c));

    public override string ToString() => "***REDACTED***";
}