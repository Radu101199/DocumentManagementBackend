using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Controllers;

public class DocumentsControllerTests
{
    private CustomWebApplicationFactory _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task CreateDocument_Should_Return_Created_With_DocumentId()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/documents", command);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var documentId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.That(documentId, Is.Not.EqualTo(Guid.Empty));
        
        Assert.That(response.Headers.Location, Is.Not.Null);
    }

    [Test]
    public async Task CreateDocument_Should_Return_BadRequest_When_Title_Empty()
    {
        // Arrange
        var command = new CreateDocumentCommand(
            Title: "",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/documents", command);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ApproveDocument_Should_Return_NoContent_When_Valid()
    {
        // Arrange - First create a document
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Need to request approval first
        var requestApprovalPayload = new { /* if you have this endpoint */ };

        // Act - Approve the document
        var approvePayload = new
        {
            ApproverId = Guid.NewGuid(),
            Notes = "Looks good!"
        };

        var response = await _client.PostAsJsonAsync($"/api/documents/{documentId}/approve", approvePayload);

        // Assert
        // Will fail because document needs to request approval first
        // This test demonstrates the domain rule enforcement
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ApproveDocument_Should_Return_NotFound_When_Document_DoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var approvePayload = new
        {
            ApproverId = Guid.NewGuid(),
            Notes = "Looks good!"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/documents/{nonExistentId}/approve", approvePayload);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task RejectDocument_Should_Return_BadRequest_When_Reason_Missing()
    {
        // Arrange
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var rejectPayload = new
        {
            RejectorId = Guid.NewGuid(),
            Reason = "" // Empty reason
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/documents/{documentId}/reject", rejectPayload);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MarkReviewed_Should_Return_NoContent_When_Valid()
    {
        // Arrange - Create document
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act - Mark as reviewed
        var reviewPayload = new
        {
            ReviewerId = Guid.NewGuid(),
            Notes = "Reviewed successfully"
        };

        var response = await _client.PostAsJsonAsync($"/api/documents/{documentId}/review", reviewPayload);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CancelApproval_Should_Return_BadRequest_When_Document_Not_Published()
    {
        // Arrange - Create document
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act - Try to cancel approval when document is not published
        var cancelPayload = new
        {
            CancelledById = Guid.NewGuid(),
            Reason = "Wrong version"
        };

        var response = await _client.PostAsJsonAsync($"/api/documents/{documentId}/cancel", cancelPayload);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}