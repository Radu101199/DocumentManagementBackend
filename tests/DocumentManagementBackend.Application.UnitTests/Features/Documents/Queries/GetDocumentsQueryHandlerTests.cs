using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.ValueObjects;
using DocumentManagementBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DocumentManagementBackend.Application.UnitTests.Features.Documents.Queries;

[TestFixture]
public class GetDocumentsQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private GetDocumentsQueryHandler _handler = null!;
    private User _testUser = null!;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new GetDocumentsQueryHandler(_context);

        // Seed user
        _testUser = User.Create(
            Email.Create("owner@test.com"),
            "John", "Doe", "hash", UserRole.User);
        _context.Users.Add(_testUser);
        
        // Creează un al doilea user pentru doc3
        var otherUser = User.Create(
            Email.Create("other@test.com"),
            "Jane", "Doe", "hash", UserRole.User);
        _context.Users.Add(otherUser);

        var doc1 = Document.Create("Alpha", "Desc", "a.pdf", "/a", "application/pdf", 1024, _testUser.Id, _testUser.Id);
        var doc2 = Document.Create("Beta", "Desc", "b.pdf", "/b", "application/pdf", 2048, _testUser.Id, _testUser.Id);
        var doc3 = Document.Create("Gamma", "Desc", "c.pdf", "/c", "application/pdf", 3072, otherUser.Id, otherUser.Id);

        _context.Documents.AddRange(doc1, doc2, doc3);
        await _context.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task Handle_Should_Return_All_Documents_With_Default_Pagination()
    {
        // Arrange
        var query = new GetDocumentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.Page, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(20));
        Assert.That(result.Items.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task Handle_Should_Return_Correct_Page()
    {
        // Arrange
        var query = new GetDocumentsQuery(Page: 1, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Items.Count(), Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.TotalPages, Is.EqualTo(2));
        Assert.That(result.HasNextPage, Is.True);
        Assert.That(result.HasPreviousPage, Is.False);
    }

    [Test]
    public async Task Handle_Should_Return_Second_Page()
    {
        // Arrange
        var query = new GetDocumentsQuery(Page: 2, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.HasPreviousPage, Is.True);
    }

    [Test]
    public async Task Handle_Should_Filter_By_OwnerId()
    {
        // Arrange
        var query = new GetDocumentsQuery(OwnerId: _testUser.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.All(d => d.OwnerId == _testUser.Id), Is.True);
    }

    [Test]
    public async Task Handle_Should_Filter_By_Status()
    {
        // Arrange
        var query = new GetDocumentsQuery(Status: DocumentStatus.Draft);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Items.All(d => d.Status == DocumentStatus.Draft), Is.True);
    }

    [Test]
    public async Task Handle_Should_Sort_By_Title_Ascending()
    {
        // Arrange
        var query = new GetDocumentsQuery(SortBy: "title_asc");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var titles = result.Items.Select(d => d.Title).ToList();
        Assert.That(titles, Is.EqualTo(titles.OrderBy(t => t).ToList()));
    }

    [Test]
    public async Task Handle_Should_Sort_By_CreatedAt_Descending_By_Default()
    {
        // Arrange
        var query = new GetDocumentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dates = result.Items.Select(d => d.CreatedAt).ToList();
        Assert.That(dates, Is.EqualTo(dates.OrderByDescending(d => d).ToList()));
    }

    [Test]
    public async Task Handle_Should_Clamp_PageSize_To_100()
    {
        // Arrange
        var query = new GetDocumentsQuery(PageSize: 999);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.PageSize, Is.EqualTo(100));
    }

    [Test]
    public async Task Handle_Should_Return_Empty_When_No_Documents_Match_Filter()
    {
        // Arrange
        var query = new GetDocumentsQuery(OwnerId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(0));
        Assert.That(result.Items, Is.Empty);
    }
}