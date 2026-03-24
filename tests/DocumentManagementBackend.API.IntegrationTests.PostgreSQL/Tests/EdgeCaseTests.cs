using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.PostgreSQL.Helpers;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.PostgreSQL.Tests;

[TestFixture]
public class EdgeCaseTests
{
    private PostgreSqlWebApplicationFactory _factory = null!;
    private HttpClient _userClient = null!;
    private HttpClient _adminClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new PostgreSqlWebApplicationFactory();
        await _factory.InitializeAsync();

        var userToken = await AuthHelper.GetTokenAsync(
            _factory.CreateClient(), "user@test.com", "password123");
        var adminToken = await AuthHelper.GetTokenAsync(
            _factory.CreateClient(), "admin@test.com", "password123");

        _userClient = _factory.CreateClient();
        _userClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userToken);

        _adminClient = _factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _userClient.Dispose();
        _adminClient.Dispose();
        await _factory.DisposeAsync();
        _factory.Dispose();
    }

    [Test]
    public async Task CreateDocument_With_Empty_Title_Should_Return_BadRequest()
    {
        var response = await _userClient.PostAsJsonAsync("/api/documents", new
        {
            title = "",
            description = "Test",
            fileName = "test.pdf",
            contentType = "application/pdf",
            fileSizeBytes = 1024,
            ownerId = _factory.TestUserId
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetDocument_With_Nonexistent_Id_Should_Return_NotFound()
    {
        var response = await _userClient.GetAsync($"/api/documents/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ApproveDocument_Not_In_PendingReview_Should_Return_BadRequest()
    {
        var createResponse = await _userClient.PostAsJsonAsync("/api/documents", new
        {
            title = "Draft Document",
            description = "Test",
            fileName = "test.pdf",
            contentType = "application/pdf",
            fileSizeBytes = 1024,
            ownerId = _factory.TestUserId
        });
        createResponse.EnsureSuccessStatusCode();
        var docId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var approveResponse = await _adminClient.PostAsJsonAsync(
            $"/api/documents/{docId}/approve", new { });
        Assert.That(approveResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetDocuments_With_Large_PageSize_Should_Return_Ok()
    {
        var response = await _userClient.GetAsync("/api/documents?pageSize=999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task HealthCheck_Should_Return_Ok()
    {
        var response = await _factory.CreateClient().GetAsync("/health");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}