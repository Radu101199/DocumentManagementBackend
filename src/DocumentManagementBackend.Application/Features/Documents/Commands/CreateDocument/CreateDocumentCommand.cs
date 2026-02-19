using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.CreateDocument;

public record CreateDocumentCommand(
    string Title,
    string? Description,
    string FileName,
    string FilePath,
    string ContentType,
    long FileSizeBytes,
    Guid OwnerId,
    Guid CreatorId
) : IRequest<Guid>;