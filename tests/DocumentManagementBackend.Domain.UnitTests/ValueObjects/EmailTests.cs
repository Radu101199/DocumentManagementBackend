using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.ValueObjects;
using NUnit.Framework;

namespace DocumentManagementBackend.Domain.UnitTests.ValueObjects;

public class EmailTests
{
    [Test]
    public void Should_Create_Valid_Email()
    {
        // Act
        var email = Email.Create("test@example.com");

        // Assert
        Assert.That(email.Value, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void Should_Convert_To_Lowercase()
    {
        // Act
        var email = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        Assert.That(email.Value, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void Should_Trim_Whitespace()
    {
        // Act
        var email = Email.Create("  test@example.com  ");

        // Assert
        Assert.That(email.Value, Is.EqualTo("test@example.com"));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Should_Throw_When_Email_IsEmpty(string? invalidEmail)
    {
        Assert.Throws<DomainException>(() => Email.Create(invalidEmail!));
    }
    
    [Test]
    [TestCase("invalid")]
    [TestCase("@example.com")]
    [TestCase("test@")]
    [TestCase("test @example.com")]
    public void Should_Throw_When_Email_IsInvalid(string invalidEmail)
    {
        Assert.Throws<DomainException>(() => Email.Create(invalidEmail));
    }

    [Test]
    public void Should_Be_Equal_When_Values_Match()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Assert
        Assert.That(email1, Is.EqualTo(email2));
        Assert.That(email1 == email2, Is.True);
    }

    [Test]
    public void Should_Not_Be_Equal_When_Values_Differ()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Assert
        Assert.That(email1, Is.Not.EqualTo(email2));
        Assert.That(email1 != email2, Is.True);
    }

    [Test]
    public void Should_Have_Same_HashCode_When_Equal()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Assert
        Assert.That(email1.GetHashCode(), Is.EqualTo(email2.GetHashCode()));
    }
}