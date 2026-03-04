using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.Helpers;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Controllers;

[TestFixture]
public class DocumentsControllerTests
{
    private static CustomWebApplicationFactory? _sharedFactory;
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;


    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _sharedFactory = new CustomWebApplicationFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _sharedFactory?.Dispose();
    }

    [SetUp]
    public async Task Setup()
    {
        _client = _sharedFactory!.CreateClient();
        var userToken = await AuthHelper.GetTokenAsync(_client, "user@test.com", "password123");
        _client.AddAuthHeader(userToken);

        _adminClient = _sharedFactory!.CreateClient();
        var adminToken = await AuthHelper.GetTokenAsync(_adminClient, "admin@test.com", "password123");
        _adminClient.AddAuthHeader(adminToken);
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _adminClient?.Dispose();
    }
    
    [Test]
    public async Task CreateDocument_Should_Return_Created_When_Authenticated()
    {
        
        var command = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);


        // Act
        var response = await _client.PostAsJsonAsync("/api/documents", command);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }
    
    [Test]
    public async Task CreateDocument_Should_Return_Created_With_DocumentId()
    {
        var command = new CreateDocumentCommand(
            Title: "Test Document",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);

        var response = await _client.PostAsJsonAsync("/api/documents", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var documentId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.That(documentId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(response.Headers.Location, Is.Not.Null);
    }

    [Test]
    public async Task CreateDocument_Should_Return_BadRequest_When_Title_Empty()
    {
        var command = new CreateDocumentCommand(
            Title: "",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);

        var response = await _client.PostAsJsonAsync("/api/documents", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ApproveDocument_Should_Return_BadRequest_When_Not_Requested()
    {
        // Creează document cu user normal
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document Approve",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await createResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Create failed ({createResponse.StatusCode}): {body}");
        }

        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // ✅ Aprobă cu Admin
        var approvePayload = new { ApproverId = _sharedFactory!.TestAdminId, Notes = "Looks good!" };
        var response = await _adminClient.PostAsJsonAsync($"/api/documents/{documentId}/approve", approvePayload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ApproveDocument_Should_Return_NotFound_When_Document_DoesNotExist()
    {
        // ✅ Admin încearcă să aprobă document inexistent
        var approvePayload = new { ApproverId = _sharedFactory!.TestAdminId, Notes = "Looks good!" };
        var response = await _adminClient.PostAsJsonAsync(
            $"/api/documents/{Guid.NewGuid()}/approve", approvePayload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task RejectDocument_Should_Return_BadRequest_When_Reason_Missing()
    {
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document Reject",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await createResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Create failed ({createResponse.StatusCode}): {body}");
        }

        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // ✅ Respinge cu Admin, dar fără motiv
        var rejectPayload = new { RejectorId = _sharedFactory!.TestAdminId, Reason = "" };
        var response = await _adminClient.PostAsJsonAsync($"/api/documents/{documentId}/reject", rejectPayload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MarkReviewed_Should_Return_NoContent_When_Valid()
    {
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document For Review",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await createResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Create failed ({createResponse.StatusCode}): {body}");
        }

        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var reviewPayload = new
        {
            ReviewerId = _sharedFactory!.TestUserId,
            Notes = "Reviewed successfully"
        };

        var response = await _client.PostAsJsonAsync($"/api/documents/{documentId}/review", reviewPayload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CancelApproval_Should_Return_BadRequest_When_Document_Not_Published()
    {
        var createCommand = new CreateDocumentCommand(
            Title: "Test Document For Cancel",
            Description: "Test Description",
            FileName: "test.pdf",
            FilePath: "/files/test.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await createResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Create failed ({createResponse.StatusCode}): {body}");
        }

        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var cancelPayload = new
        {
            CancelledById = _sharedFactory!.TestUserId,
            Reason = "Wrong version"
        };

        var response = await _client.PostAsJsonAsync($"/api/documents/{documentId}/cancel", cancelPayload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}