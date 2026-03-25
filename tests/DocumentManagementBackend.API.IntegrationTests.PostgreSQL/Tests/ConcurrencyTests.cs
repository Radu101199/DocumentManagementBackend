using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.PostgreSQL.Helpers;
using DocumentManagementBackend.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.PostgreSQL.Tests;

[TestFixture]
public class ConcurrencyTests
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

    private async Task<Guid> CreateDocumentAsync()
    {
        var response = await _userClient.PostAsJsonAsync("/api/documents", new
        {
            title = $"Test Doc {Guid.NewGuid()}",
            description = "Concurrency test",
            fileName = "test.pdf",
            filePath = "/uploads/test.pdf",
            contentType = "application/pdf",
            fileSizeBytes = 1024,
            ownerId = _factory.TestUserId,
            creatorId = _factory.TestUserId
        });
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Create failed ({response.StatusCode}): {body}");
        }
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    [Test]
    public async Task Concurrent_RequestApproval_Should_Handle_Race_Condition()
    {
        var docId = await CreateDocumentAsync();

        var tasks = Enumerable.Range(0, 5).Select(_ =>
            _userClient.PostAsJsonAsync($"/api/documents/{docId}/request-approval", new { }));

        var responses = await Task.WhenAll(tasks);
        var statusCodes = responses.Select(r => r.StatusCode).ToList();

        Assert.That(statusCodes, Has.Some.EqualTo(HttpStatusCode.NoContent)
            .Or.Some.EqualTo(HttpStatusCode.Conflict)
            .Or.Some.EqualTo(HttpStatusCode.BadRequest));

        Assert.That(statusCodes, Has.None.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task PostgreSQL_xmin_ConcurrencyToken_Is_Detected()
    {
        var docId = await CreateDocumentAsync();

        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var doc1 = await db1.Documents.FindAsync(docId);
        var doc2 = await db2.Documents.FindAsync(docId);

        Assert.That(doc1, Is.Not.Null);
        Assert.That(doc2, Is.Not.Null);

        doc1!.UpdatedAt = DateTime.UtcNow.AddSeconds(1);
        await db1.SaveChangesAsync();

        doc2!.UpdatedAt = DateTime.UtcNow.AddSeconds(2);
        Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>(
            async () => await db2.SaveChangesAsync());
    }
}