using System.Security.Claims;
using DocumentManagementBackend.Application.Common.Models;
using DocumentManagementBackend.Application.Features.Documents.Commands.ApproveDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.CancelApproval;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.DeleteDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.MarkReviewed;
using DocumentManagementBackend.Application.Features.Documents.Commands.RejectDocument;
using DocumentManagementBackend.Application.Features.Documents.Commands.SaveVersion;
using DocumentManagementBackend.Application.Features.Documents.Queries;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentById;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;
using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentVersions;
using DocumentManagementBackend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DocumentManagementBackend.API.Controllers;

/// <summary>Document management and approval workflow</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("api")]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get paginated list of documents</summary>
    /// <remarks>
    /// Supports filtering by owner and status, and sorting by multiple fields.
    /// 
    /// Valid sortBy values: `title_asc`, `title_desc`, `createdAt_asc`, `createdAt_desc`, `status_asc`, `status_desc`
    /// 
    /// Sample request:
    /// 
    ///     GET /api/documents?page=1&amp;pageSize=20&amp;status=Draft&amp;sortBy=createdAt_desc
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page, max 100 (default: 20)</param>
    /// <param name="ownerId">Filter by owner ID</param>
    /// <param name="status">Filter by document status</param>
    /// <param name="sortBy">Sort field and direction</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? ownerId = null,
        [FromQuery] DocumentStatus? status = null,
        [FromQuery] string? sortBy = null)
    {
        var result = await _mediator.Send(new GetDocumentsQuery(page, pageSize, ownerId, status, sortBy));
        return Ok(result);
    }

    /// <summary>Get a document by ID</summary>
    /// <param name="id">Document ID</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id));
        return Ok(result);
    }

    /// <summary>Create a new document</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/documents
    ///     {
    ///         "title": "Q4 Financial Report",
    ///         "description": "Quarterly financial summary",
    ///         "fileName": "q4-report.pdf",
    ///         "contentType": "application/pdf",
    ///         "fileSizeBytes": 204800
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentCommand command)
    {
        var documentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = documentId }, documentId);
    }

    /// <summary>Request approval for a document</summary>
    /// <remarks>
    /// Only the document owner can request approval.
    /// </remarks>
    /// <param name="id">Document ID</param>
    [HttpPost("{id:guid}/request-approval")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequestApproval(Guid id)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new ApproveDocumentCommand(id, userId, null));
        return NoContent();
    }

    /// <summary>Approve a document (Admin only)</summary>
    /// <remarks>
    /// Requires **Admin** role.
    /// </remarks>
    /// <param name="id">Document ID</param>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new ApproveDocumentCommand(id, userId, null));
        return NoContent();
    }

    /// <summary>Reject a document (Admin only)</summary>
    /// <remarks>
    /// Requires **Admin** role. A rejection reason is mandatory.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/documents/{id}/reject
    ///     {
    ///         "reason": "Missing required attachments"
    ///     }
    /// </remarks>
    /// <param name="id">Document ID</param>
    /// <param name="command">Rejection reason</param>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDocumentCommand command)
    {
        await _mediator.Send(command with { DocumentId = id });
        return NoContent();
    }

    /// <summary>Cancel an approval request</summary>
    /// <remarks>
    /// Only the document owner can cancel the approval request.
    /// </remarks>
    /// <param name="id">Document ID</param>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelApproval(Guid id)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new CancelApprovalCommand(id, userId, null));
        return NoContent();
    }

    /// <summary>Mark a document as reviewed</summary>
    /// <remarks>
    /// Marks the document as reviewed without approving or rejecting.
    /// Requires **Admin** role.
    /// </remarks>
    /// <param name="id">Document ID</param>
    [HttpPost("{id:guid}/review")] 
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkReviewed(Guid id)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new MarkReviewedCommand(id, userId, null));
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
    
    /// <summary>Save a version snapshot of the document</summary>
    [HttpPost("{id:guid}/versions")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveVersion(Guid id, [FromBody] SaveVersionRequest request)
    {
        var userId = GetCurrentUserId();
        var versionNumber = await _mediator.Send(
            new SaveVersionCommand(id, userId, request.Comment));
        return Ok(new { versionNumber });
    }
    
    /// <summary>
    /// Soft delete document.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _mediator.Send(new DeleteDocumentCommand(id, userId), ct);
        return NoContent();
    }
    
    /// <summary>Get version history of a document</summary>
    [HttpGet("{id:guid}/versions")]
    [ProducesResponseType(typeof(List<DocumentVersionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVersions(Guid id)
    {
        var versions = await _mediator.Send(new GetDocumentVersionsQuery(id));
        return Ok(versions);
    }
}