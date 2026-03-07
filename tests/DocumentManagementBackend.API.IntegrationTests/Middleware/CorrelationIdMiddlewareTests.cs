using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.Helpers;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Middleware;

[TestFixture]
public class CorrelationIdMiddlewareTests
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
    }

    [TearDown]
    public void TearDown() => _client.Dispose();

    [Test]
    public async Task Request_Should_Return_CorrelationId_Header_When_Provided()
    {
        // Arrange
        var correlationId = "test-correlation-123";
        _client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

        // Act
        var response = await _client.GetAsync("/api/documents/health");

        // Assert
        Assert.That(
            response.Headers.Contains("X-Correlation-ID"),
            Is.True);

        var returnedId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.That(returnedId, Is.EqualTo(correlationId));
    }

    [Test]
    public async Task Request_Should_Generate_CorrelationId_When_Not_Provided()
    {
        // Act
        var response = await _client.GetAsync("/api/documents/health");

        // Assert
        Assert.That(
            response.Headers.Contains("X-Correlation-ID"),
            Is.True);

        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.That(correlationId, Is.Not.Null.And.Not.Empty);
        Assert.That(Guid.TryParse(correlationId, out _), Is.True);
    }

    [Test]
    public async Task Error_Response_Should_Contain_TraceId()
    {
        // Folosește Admin token ca să treacă de autorizare
        // și să ajungă la domeniu unde middleware-ul gestionează eroarea
        var adminToken = await AuthHelper.GetTokenAsync(_client, "admin@test.com", "password123");
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act - document inexistent → NotFoundException → middleware → traceId în response
        var response = await _client.PostAsJsonAsync(
            $"/api/documents/{Guid.NewGuid()}/approve",
            new { ApproverId = Guid.NewGuid(), Notes = "test" });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("traceId"));
    }
}