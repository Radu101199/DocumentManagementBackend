using System.Text.RegularExpressions;
using DocumentManagementBackend.Domain.Exceptions;

namespace DocumentManagementBackend.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");

        var trimmed = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(trimmed))
            throw new DomainException($"Invalid email format: {email}");

        if (trimmed.Length > 256)
            throw new DomainException("Email must not exceed 256 characters");

        return new Email(trimmed);
    }

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Email email && Equals(email);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    public static bool operator ==(Email? left, Email? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Email? left, Email? right)
    {
        return !(left == right);
    }
}