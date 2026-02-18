using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.ValueObjects;
using NUnit.Framework;

namespace DocumentManagementBackend.Domain.UnitTests.ValueObjects;

public class PasswordTests
{
    [Test]
    public void Should_Create_Valid_Password()
    {
        // Act
        var password = Password.Create("SecurePass123!");

        // Assert
        Assert.That(password, Is.Not.Null);
    }

    [Test]
    [TestCase("short1!")]
    public void Should_Throw_When_Password_TooShort(string shortPassword)
    {
        Assert.Throws<DomainException>(() => Password.Create(shortPassword));
    }

    [Test]
    public void Should_Throw_When_Missing_Uppercase()
    {
        Assert.Throws<DomainException>(() => Password.Create("lowercase123!"));
    }

    [Test]
    public void Should_Throw_When_Missing_Lowercase()
    {
        Assert.Throws<DomainException>(() => Password.Create("UPPERCASE123!"));
    }

    [Test]
    public void Should_Throw_When_Missing_Digit()
    {
        Assert.Throws<DomainException>(() => Password.Create("NoDigitsHere!"));
    }

    [Test]
    public void Should_Throw_When_Missing_SpecialChar()
    {
        Assert.Throws<DomainException>(() => Password.Create("NoSpecialChar123"));
    }

    [Test]
    public void Should_Redact_ToString()
    {
        // Arrange
        var password = Password.Create("SecurePass123!");

        // Act
        var result = password.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("***REDACTED***"));
    }
}