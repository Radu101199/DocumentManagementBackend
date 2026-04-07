using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.SaveVersion;

public record SaveVersionCommand(
    Guid DocumentId,
    Guid UserId,
    string? Comment
) : IRequest<int>;  // returnează numărul versiunii
