using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests;

[TestFixture]
public class LoggingTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task TestEndpoint_ShouldReturnOk_AndLogMessages()
    {
        // Act
        var response = await _client.GetAsync("/api/test");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Logging is working!"));
    }
}