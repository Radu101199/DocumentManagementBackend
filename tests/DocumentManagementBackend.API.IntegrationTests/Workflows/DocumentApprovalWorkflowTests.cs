using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.API.IntegrationTests.Helpers;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Workflows;

[TestFixture]
public class DocumentApprovalWorkflowTests
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
    public async Task CompleteDocumentLifecycle_Should_Work_EndToEnd()
    {
        var createCommand = new CreateDocumentCommand(
            Title: "Project Proposal Workflow",
            Description: "Q1 2026 Project Proposal",
            FileName: "proposal.pdf",
            FilePath: "/files/proposal.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 2048,
            OwnerId: _sharedFactory!.TestUserId,
            CreatorId: _sharedFactory!.TestUserId);

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await createResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Create failed ({createResponse.StatusCode}): {body}");
        }

        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.That(documentId, Is.Not.EqualTo(Guid.Empty));

        // Review cu user normal
        var reviewPayload = new { ReviewerId = _sharedFactory!.TestUserId, Notes = "Initial review completed" };
        var reviewResponse = await _client.PostAsJsonAsync($"/api/documents/{documentId}/review", reviewPayload);
        Assert.That(reviewResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // ✅ Approve cu Admin — trebuie să returneze BadRequest (fără RequestApproval)
        var approvePayload = new { ApproverId = _sharedFactory!.TestAdminId, Notes = "Approved" };
        var approveResponse =
            await _adminClient.PostAsJsonAsync($"/api/documents/{documentId}/approve", approvePayload);
        Assert.That(approveResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Reject_Should_Fail_Without_Approval_Request()
    {
        var createCommand = new CreateDocumentCommand(
            Title: "Draft Document Reject Test",
            Description: "Draft",
            FileName: "draft.pdf",
            FilePath: "/files/draft.pdf",
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

        // ✅ Reject cu Admin — trebuie să returneze BadRequest (fără approval request)
        var rejectPayload = new { RejectorId = _sharedFactory!.TestAdminId, Reason = "Missing signatures" };
        var rejectResponse = await _adminClient.PostAsJsonAsync($"/api/documents/{documentId}/reject", rejectPayload);

        Assert.That(rejectResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}