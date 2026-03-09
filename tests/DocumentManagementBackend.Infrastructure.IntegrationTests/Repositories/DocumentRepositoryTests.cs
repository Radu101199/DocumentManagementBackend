using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.ValueObjects;
using DocumentManagementBackend.Infrastructure.Persistence;
using DocumentManagementBackend.Infrastructure.Persistence.Repositories;
using DocumentManagementBackend.Infrastructure.Services;
using Moq;
using NUnit.Framework;

namespace DocumentManagementBackend.Infrastructure.IntegrationTests.Repositories;

public class DocumentRepositoryTests
{
    private ApplicationDbContext _context = null!;
    private DocumentRepository _repository = null!;
    private Mock<IDomainEventDispatcher> _dispatcherMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _dispatcherMock = new Mock<IDomainEventDispatcher>();
        _repository = new DocumentRepository(_context, _dispatcherMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task Should_Add_Document_Successfully()
    {
        // Arrange
        var document = Document.Create(
            "Test Document",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        await _repository.AddAsync(document);

        // Assert
        var saved = await _context.Documents.FindAsync(document.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Title, Is.EqualTo("Test Document"));
    }

    [Test]
    public async Task Should_Get_Document_By_Id()
    {
        // Arrange
        var owner = User.Create(
            Email.Create("owner@test.com"),
            "Owner",
            "User",
            "password");

        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var document = Document.Create(
            "Test Document",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            owner.Id,
            owner.Id);

        await _repository.AddAsync(document);

        // Act
        var retrieved = await _repository.GetByIdAsync(document.Id);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Null_When_Document_NotFound()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Should_Get_Documents_By_OwnerId()
    {
        // Arrange
        var owner = User.Create(
            Email.Create("owner@example.com"),
            "Owner",
            "User",
            "password");
        await _context.Users.AddAsync(owner);
        await _context.SaveChangesAsync();

        var creatorId = Guid.NewGuid();

        var doc1 = Document.Create("Doc 1", "Desc", "file1.pdf", "/path1", "application/pdf", 1024, owner.Id, creatorId);
        var doc2 = Document.Create("Doc 2", "Desc", "file2.pdf", "/path2", "application/pdf", 2048, owner.Id, creatorId);
        var doc3 = Document.Create("Doc 3", "Desc", "file3.pdf", "/path3", "application/pdf", 3072, Guid.NewGuid(), creatorId);

        await _repository.AddAsync(doc1);
        await _repository.AddAsync(doc2);
        await _repository.AddAsync(doc3);

        // Act
        var results = await _repository.GetByOwnerIdAsync(owner.Id);

        // Assert
        Assert.That(results.Count(), Is.EqualTo(2));
        Assert.That(results.All(d => d.OwnerId == owner.Id), Is.True);
    }

    [Test]
    public async Task Should_Update_Document()
    {
        // Arrange
        var owner = User.Create(
            Email.Create("owner@example.com"),
            "Owner",
            "User",
            "password");
        await _context.Users.AddAsync(owner);
        await _context.SaveChangesAsync();

        var document = Document.Create(
            "Original Title",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            owner.Id,
            owner.Id);

        await _repository.AddAsync(document);

        // Act
        document.UpdateMetadata("Updated Title", "Updated Description");
        await _repository.UpdateAsync(document);

        // Assert
        var updated = await _repository.GetByIdAsync(document.Id);
        Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
        Assert.That(updated.Description, Is.EqualTo("Updated Description"));
    }

    [Test]
    public async Task Should_Delete_Document()
    {
        // Arrange
        var document = Document.Create(
            "Test Document",
            "Description",
            "test.pdf",
            "/files/test.pdf",
            "application/pdf",
            1024,
            Guid.NewGuid(),
            Guid.NewGuid());

        await _repository.AddAsync(document);

        // Act
        await _repository.DeleteAsync(document.Id);

        // Assert
        var deleted = await _repository.GetByIdAsync(document.Id);
        Assert.That(deleted, Is.Null);
    }
    
    [Test]
    public async Task UpdateAsync_Should_Throw_ConcurrencyException_When_Concurrent_Update()
    {
        // Arrange — seed document
        var owner = User.Create(Email.Create("owner@test.com"), "Test", "User", "hash");
        await _context.Users.AddAsync(owner);
        await _context.SaveChangesAsync();

        var document = Document.Create(
            "Original", "Desc", "file.pdf", "/path",
            "application/pdf", 1024, owner.Id, owner.Id);
        await _repository.AddAsync(document);

        // Simulează doi useri care iau același document
        var documentForUserA = await _repository.GetByIdAsync(document.Id);
        var documentForUserB = await _repository.GetByIdAsync(document.Id);

        // User A face update primul
        documentForUserA!.UpdateMetadata("Updated by User A", null);
        await _repository.UpdateAsync(documentForUserA);

        // User B încearcă să facă update cu RowVersion vechi
        documentForUserB!.UpdateMetadata("Updated by User B", null);

        // Assert — User B primește ConcurrencyException
        Assert.ThrowsAsync<ConcurrencyException>(
            () => _repository.UpdateAsync(documentForUserB));
    }
    
}