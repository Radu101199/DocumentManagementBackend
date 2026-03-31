using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentById;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.ValueObjects;
using DocumentManagementBackend.Infrastructure.Persistence;
using DocumentManagementBackend.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Queries;

[TestFixture]
public class GetDocumentByIdQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private GetDocumentByIdQueryHandler _handler = null!;
    private User _testUser = null!;
    private Document _testDocument = null!;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, new AuditInterceptor());
        _handler = new GetDocumentByIdQueryHandler(_context);

        _testUser = User.Create(
            Email.Create("owner@test.com"),
            "John", "Doe", "hash", UserRole.User);
        _context.Users.Add(_testUser);

        _testDocument = Document.Create(
            "Test Document", "Description",
            "test.pdf", "/files/test.pdf",
            "application/pdf", 1024,
            _testUser.Id, _testUser.Id);
        _context.Documents.Add(_testDocument);

        await _context.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task Handle_Should_Return_Document_When_Found()
    {
        // Arrange
        var query = new GetDocumentByIdQuery(_testDocument.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(_testDocument.Id));
        Assert.That(result.Title, Is.EqualTo("Test Document"));
        Assert.That(result.OwnerFullName, Is.EqualTo("John Doe"));
    }

    [Test]
    public void Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        // Arrange
        var query = new GetDocumentByIdQuery(Guid.NewGuid());

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Test]
    public async Task Handle_Should_Return_Correct_Owner_Name()
    {
        // Arrange
        var query = new GetDocumentByIdQuery(_testDocument.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.OwnerFullName, Is.EqualTo("John Doe"));
    }
}