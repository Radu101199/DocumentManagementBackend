using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.Helpers;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Application.Features.Documents.Queries;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Controllers;

[TestFixture]
public class DocumentsQueryIntegrationTests
{
    private static CustomWebApplicationFactory? _factory;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetup() => _factory = new CustomWebApplicationFactory();

    [OneTimeTearDown]
    public void OneTimeTearDown() => _factory?.Dispose();

    [SetUp]
    public async Task Setup()
    {
        _client = _factory!.CreateClient();
        var token = await AuthHelper.GetTokenAsync(_client, "user@test.com", "password123");
        _client.AddAuthHeader(token);

        // Seed câteva documente
        for (int i = 1; i <= 5; i++)
        {
            var command = new CreateDocumentCommand(
                Title: $"Document {i}",
                Description: $"Description {i}",
                FileName: $"file{i}.pdf",
                FilePath: $"/files/file{i}.pdf",
                ContentType: "application/pdf",
                FileSizeBytes: 1024 * i,
                OwnerId: _factory!.TestUserId,
                CreatorId: _factory!.TestUserId);

            await _client.PostAsJsonAsync("/api/documents", command);
        }
    }

    [TearDown]
    public void TearDown() => _client.Dispose();

    [Test]
    public async Task GetAll_Should_Return_Ok_With_PagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/documents");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Is.Not.Empty);
        Assert.That(result.Page, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAll_Should_Return_Paginated_Results()
    {
        // Act
        var response = await _client.GetAsync("/api/documents?page=1&pageSize=2");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>();
        Assert.That(result!.Items.Count(), Is.LessThanOrEqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAll_Should_Filter_By_OwnerId()
    {
        // Act
        var response = await _client.GetAsync($"/api/documents?ownerId={_factory!.TestUserId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>();
        Assert.That(result!.Items.All(d => d.OwnerId == _factory!.TestUserId), Is.True);
    }

    [Test]
    public async Task GetAll_Should_Filter_By_Status()
    {
        // Act
        var response = await _client.GetAsync("/api/documents?status=Draft");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>();
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetAll_Should_Sort_By_Title()
    {
        // Act
        var response = await _client.GetAsync("/api/documents?sortBy=title_asc");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>();
        var titles = result!.Items.Select(d => d.Title).ToList();
        Assert.That(titles, Is.EqualTo(titles.OrderBy(t => t).ToList()));
    }

    [Test]
    public async Task GetById_Should_Return_Document()
    {
        // Arrange — creează un document și ia ID-ul
        var command = new CreateDocumentCommand(
            Title: "Specific Document",
            Description: "Test",
            FileName: "specific.pdf",
            FilePath: "/files/specific.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _factory!.TestUserId,
            CreatorId: _factory!.TestUserId);

        var createResponse = await _client.PostAsJsonAsync("/api/documents", command);
        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await _client.GetAsync($"/api/documents/{documentId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.That(result!.Id, Is.EqualTo(documentId));
        Assert.That(result.Title, Is.EqualTo("Specific Document"));
    }

    [Test]
    public async Task GetById_Should_Return_NotFound_When_Document_DoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/documents/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetAll_Should_Return_Unauthorized_Without_Token()
    {
        // Arrange — client fără token
        var unauthClient = _factory!.CreateClient();

        // Act
        var response = await unauthClient.GetAsync("/api/documents");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
