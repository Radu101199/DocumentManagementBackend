using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.Helpers;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Security;

[TestFixture]
public class DocumentsSecurityTests
{
    private static CustomWebApplicationFactory? _factory;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetup() => _factory = new CustomWebApplicationFactory();

    [OneTimeTearDown]
    public void OneTimeTearDown() => _factory?.Dispose();

    [SetUp]
    public void Setup() => _client = _factory!.CreateClient();

    [TearDown]
    public void TearDown() => _client.Dispose();

    // ── No token ──────────────────────────────────────────────

    [Test]
    public async Task CreateDocument_Should_Return_Unauthorized_When_No_Token()
    {
        var command = new CreateDocumentCommand(
            Title: "Test", Description: "Test",
            FileName: "test.pdf", FilePath: "/files/test.pdf",
            ContentType: "application/pdf", FileSizeBytes: 1024,
            OwnerId: _factory!.TestUserId, CreatorId: _factory!.TestUserId);

        var response = await _client.PostAsJsonAsync("/api/documents", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ApproveDocument_Should_Return_Unauthorized_When_No_Token()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/documents/{Guid.NewGuid()}/approve",
            new { ApproverId = Guid.NewGuid(), Notes = "test" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    // ── Invalid token ─────────────────────────────────────────

    [Test]
    public async Task CreateDocument_Should_Return_Unauthorized_When_Invalid_Token()
    {
        _client.AddAuthHeader("this.is.not.a.valid.token");

        var command = new CreateDocumentCommand(
            Title: "Test", Description: "Test",
            FileName: "test.pdf", FilePath: "/files/test.pdf",
            ContentType: "application/pdf", FileSizeBytes: 1024,
            OwnerId: _factory!.TestUserId, CreatorId: _factory!.TestUserId);

        var response = await _client.PostAsJsonAsync("/api/documents", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    // ── Wrong role ────────────────────────────────────────────

    [Test]
    public async Task ApproveDocument_Should_Return_Forbidden_When_User_Role()
    {
        // Login as regular user (not Admin)
        var token = await AuthHelper.GetTokenAsync(_client, "user@test.com", "password123");
        _client.AddAuthHeader(token);

        var response = await _client.PostAsJsonAsync(
            $"/api/documents/{Guid.NewGuid()}/approve",
            new { ApproverId = _factory!.TestUserId, Notes = "test" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    // ── Correct role ──────────────────────────────────────────

    [Test]
    public async Task CreateDocument_Should_Return_Created_When_Authenticated()
    {
        var token = await AuthHelper.GetTokenAsync(_client, "user@test.com", "password123");
        _client.AddAuthHeader(token);

        var command = new CreateDocumentCommand(
            Title: "Secure Document", Description: "Test",
            FileName: "test.pdf", FilePath: "/files/test.pdf",
            ContentType: "application/pdf", FileSizeBytes: 1024,
            OwnerId: _factory!.TestUserId, CreatorId: _factory!.TestUserId);

        var response = await _client.PostAsJsonAsync("/api/documents", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task ApproveDocument_Should_Return_NotFound_When_Admin_And_Document_Missing()
    {
        // Admin with correct role — gets past authorization, hits domain logic
        var token = await AuthHelper.GetTokenAsync(_client, "admin@test.com", "password123");
        _client.AddAuthHeader(token);

        var response = await _client.PostAsJsonAsync(
            $"/api/documents/{Guid.NewGuid()}/approve",
            new { ApproverId = _factory!.TestAdminId, Notes = "approved" });

        // 404 means auth passed, document just doesn't exist
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
