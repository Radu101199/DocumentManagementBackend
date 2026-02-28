using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Workflows;

[TestFixture]
public class DocumentApprovalWorkflowTests
{
    private static CustomWebApplicationFactory? _sharedFactory;
    private HttpClient _client = null!;

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
    public void Setup()
    {
        _client = _sharedFactory!.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public async Task CompleteDocumentLifecycle_Should_Work_EndToEnd()
    {
        // 1. Create Document
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

        // 2. Mark as Reviewed
        var reviewPayload = new
        {
            ReviewerId = _sharedFactory!.TestUserId,
            Notes = "Initial review completed"
        };

        var reviewResponse = await _client.PostAsJsonAsync($"/api/documents/{documentId}/review", reviewPayload);
        Assert.That(reviewResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // 3. Try to Approve (should fail - needs RequestApproval first)
        var approvePayload = new
        {
            ApproverId = _sharedFactory!.TestUserId,
            Notes = "Approved"
        };

        var approveResponse = await _client.PostAsJsonAsync($"/api/documents/{documentId}/approve", approvePayload);
        Assert.That(approveResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Reject_Should_Fail_Without_Approval_Request()
    {
        // 1. Create Document
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

        // 2. Try to Reject Document (should fail - needs approval request first)
        var rejectPayload = new
        {
            RejectorId = _sharedFactory!.TestUserId,
            Reason = "Missing signatures"
        };

        var rejectResponse = await _client.PostAsJsonAsync($"/api/documents/{documentId}/reject", rejectPayload);

        Assert.That(rejectResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}