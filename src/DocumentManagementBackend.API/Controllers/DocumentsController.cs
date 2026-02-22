using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentCommand command)
    {
        var documentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = documentId }, documentId);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // TODO: Implement GetDocumentQuery
        return Ok(new { id });
    }
}