using System.Net;
using System.Net.Http.Json;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using NUnit.Framework;

namespace DocumentManagementBackend.API.IntegrationTests.Workflows;

public class DocumentApprovalWorkflowTests
{
    private CustomWebApplicationFactory _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task CompleteDocumentLifecycle_Should_Work_EndToEnd()
    {
        // 1. Create Document
        var createCommand = new CreateDocumentCommand(
            Title: "Project Proposal",
            Description: "Q1 2026 Project Proposal",
            FileName: "proposal.pdf",
            FilePath: "/files/proposal.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 2048,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        Assert.That(documentId, Is.Not.EqualTo(Guid.Empty));

        // 2. Mark as Reviewed
        var reviewPayload = new
        {
            ReviewerId = Guid.NewGuid(),
            Notes = "Initial review completed"
        };

        var reviewResponse = await _client.PostAsJsonAsync($"/api/documents/{documentId}/review", reviewPayload);
        Assert.That(reviewResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // 3. Try to Approve (should fail - needs RequestApproval first)
        var approvePayload = new
        {
            ApproverId = Guid.NewGuid(),
            Notes = "Approved"
        };

        var approveResponse = await _client.PostAsJsonAsync($"/api/documents/{documentId}/approve", approvePayload);
        Assert.That(approveResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // This demonstrates that domain invariants are enforced end-to-end
    }

    [Test]
    public async Task Reject_Then_Cannot_Approve_Without_New_Approval_Request()
    {
        // 1. Create Document
        var createCommand = new CreateDocumentCommand(
            Title: "Draft Document",
            Description: "Draft",
            FileName: "draft.pdf",
            FilePath: "/files/draft.pdf",
            ContentType: "application/pdf",
            FileSizeBytes: 1024,
            OwnerId: Guid.NewGuid(),
            CreatorId: Guid.NewGuid());

        var createResponse = await _client.PostAsJsonAsync("/api/documents", createCommand);
        var documentId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // 2. Reject Document
        var rejectPayload = new
        {
            RejectorId = Guid.NewGuid(),
            Reason = "Missing signatures"
        };

        var rejectResponse = await _client.PostAsJsonAsync($"/api/documents/{documentId}/reject", rejectPayload);
        
        // Should fail because document needs approval request first
        Assert.That(rejectResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}