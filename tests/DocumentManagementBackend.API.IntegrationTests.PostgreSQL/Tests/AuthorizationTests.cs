using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.PostgreSQL.Helpers;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.PostgreSQL.Tests;

[TestFixture]
public class AuthorizationTests
{
    private PostgreSqlWebApplicationFactory _factory = null!;
    private HttpClient _userClient = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _anonymousClient = null!;

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

        _anonymousClient = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _userClient.Dispose();
        _adminClient.Dispose();
        _anonymousClient.Dispose();
        await _factory.DisposeAsync();
        _factory.Dispose();
    }

    [Test]
    public async Task GetDocuments_Should_Return_Unauthorized_Without_Token()
    {
        var response = await _anonymousClient.GetAsync("/api/documents");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetDocuments_Should_Return_Ok_With_Valid_Token()
    {
        var response = await _userClient.GetAsync("/api/documents");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ApproveDocument_Should_Return_Forbidden_For_User_Role()
    {
        var docId = Guid.NewGuid();
        var response = await _userClient.PostAsJsonAsync(
            $"/api/documents/{docId}/approve", new { });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Login_With_Invalid_Credentials_Should_Return_Unauthorized()
    {
        var response = await _anonymousClient.PostAsJsonAsync("/api/auth/login", new
        {
            email = "user@test.com",
            password = "wrongpassword"
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_With_Nonexistent_User_Should_Return_NotFound()
    {
        var response = await _anonymousClient.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nonexistent@test.com",
            password = "password123"
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Invalid_JWT_Token_Should_Return_Unauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid.token.here");
        var response = await client.GetAsync("/api/documents");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
