using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Controllers;

[TestFixture]
public class AuthControllerTests
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

    [Test]
    public async Task Login_Should_Return_Token_When_Valid_Credentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "user@test.com",
            Password = "password123"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("token"));
        Assert.That(body, Does.Contain("user@test.com"));
    }

    [Test]
    public async Task Login_Should_Return_Unauthorized_When_Wrong_Password()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "user@test.com",
            Password = "wrongpassword"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_Should_Return_NotFound_When_User_DoesNotExist()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nobody@test.com",
            Password = "password123"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}