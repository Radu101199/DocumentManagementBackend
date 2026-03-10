using DocumentManagementBackend.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.ApproveDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.CancelApproval;
using DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentById;
using DocumentManagementBackend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace DocumentManagementBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get paginated documents with filtering and sorting</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? ownerId = null,
        [FromQuery] DocumentStatus? status = null,
        [FromQuery] string? sortBy = null)
    {
        var query = new GetDocumentsQuery(page, pageSize, ownerId, status, sortBy);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>Get document by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetDocumentByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentCommand command)
    {
        var documentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = documentId }, documentId);
    }

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

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequest request)
    {
        var command = new ApproveDocumentCommand(id, request.ApproverId, request.Notes);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest request)
    {
        var command = new RejectDocumentCommand(id, request.RejectorId, request.Reason);
        await _mediator.Send(command);
        return NoContent();
    }

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

public record MarkReviewedRequest(Guid ReviewerId, string? Notes);
public record ApproveRequest(Guid ApproverId, string? Notes);
public record RejectRequest(Guid RejectorId, string Reason);
public record CancelApprovalRequest(Guid CancelledById, string? Reason);