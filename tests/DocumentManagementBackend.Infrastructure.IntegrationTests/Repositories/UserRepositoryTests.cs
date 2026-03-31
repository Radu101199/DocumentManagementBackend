using Microsoft.EntityFrameworkCore;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.ValueObjects;
using DocumentManagementBackend.Infrastructure.Persistence;
using DocumentManagementBackend.Infrastructure.Persistence.Interceptors;
using DocumentManagementBackend.Infrastructure.Persistence.Repositories;
using NUnit.Framework;

namespace DocumentManagementBackend.Infrastructure.IntegrationTests.Repositories;

public class UserRepositoryTests
{
    private ApplicationDbContext _context = null!;
    private UserRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, new AuditInterceptor());
        _repository = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task Should_Add_User_Successfully()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", "hashedpassword");

        // Act
        await _repository.AddAsync(user);

        // Assert
        var saved = await _context.Users.FindAsync(user.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Email.Value, Is.EqualTo("test@example.com"));
    }

    [Test]
    public async Task Should_Get_User_By_Id()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", "hashedpassword");
        await _repository.AddAsync(user);

        // Act
        var retrieved = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task Should_Get_User_By_Email()
    {
        // Arrange
        var email = Email.Create("john@example.com");
        var user = User.Create(email, "John", "Doe", "hashedpassword");
        await _repository.AddAsync(user);

        // Act
        var retrieved = await _repository.GetByEmailAsync(email);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task Should_Return_True_When_Email_Exists()
    {
        // Arrange
        var email = Email.Create("existing@example.com");
        var user = User.Create(email, "John", "Doe", "hashedpassword");
        await _repository.AddAsync(user);

        // Act
        var exists = await _repository.ExistsWithEmailAsync(email);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task Should_Return_False_When_Email_DoesNotExist()
    {
        // Arrange
        var email = Email.Create("nonexistent@example.com");

        // Act
        var exists = await _repository.ExistsWithEmailAsync(email);

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task Should_Update_User()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", "hashedpassword");
        await _repository.AddAsync(user);

        // Act
        user.UpdateProfile("Jane", "Smith");
        await _repository.UpdateAsync(user);

        // Assert
        var updated = await _repository.GetByIdAsync(user.Id);
        Assert.That(updated!.FirstName, Is.EqualTo("Jane"));
        Assert.That(updated.LastName, Is.EqualTo("Smith"));
    }

    [Test]
    public async Task Should_Delete_User()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = User.Create(email, "John", "Doe", "hashedpassword");
        await _repository.AddAsync(user);

        // Act
        await _repository.DeleteAsync(user.Id);

        // Assert
        var deleted = await _repository.GetByIdAsync(user.Id);
        Assert.That(deleted, Is.Null);
    }
}