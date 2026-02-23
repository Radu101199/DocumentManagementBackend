using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.ApproveDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.CancelApproval;
using DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;

namespace DocumentManagementBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new document
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentCommand command)
    {
        var documentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = documentId }, documentId);
    }

    /// <summary>
    /// Get document by ID (placeholder)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // TODO: Implement GetDocumentQuery
        return Ok(new { id, message = "GetDocumentQuery not implemented yet" });
    }

    /// <summary>
    /// Mark document as reviewed
    /// </summary>
    [HttpPost("{id}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkReviewed(Guid id, [FromBody] MarkReviewedRequest request)
    {
        var command = new MarkReviewedCommand(id, request.ReviewerId, request.Notes);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Approve document
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequest request)
    {
        var command = new ApproveDocumentCommand(id, request.ApproverId, request.Notes);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Reject document
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest request)
    {
        var command = new RejectDocumentCommand(id, request.RejectorId, request.Reason);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Cancel document approval
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelApproval(Guid id, [FromBody] CancelApprovalRequest request)
    {
        var command = new CancelApprovalCommand(id, request.CancelledById, request.Reason);
        await _mediator.Send(command);
        return NoContent();
    }
}

// Request DTOs for API endpoints
public record MarkReviewedRequest(Guid ReviewerId, string? Notes);
public record ApproveRequest(Guid ApproverId, string? Notes);
public record RejectRequest(Guid RejectorId, string Reason);
public record CancelApprovalRequest(Guid CancelledById, string? Reason);