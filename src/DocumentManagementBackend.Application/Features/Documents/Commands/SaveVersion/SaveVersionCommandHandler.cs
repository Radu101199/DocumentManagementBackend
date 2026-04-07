using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Interfaces;
using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Commands.SaveVersion;

public class SaveVersionCommandHandler : IRequestHandler<SaveVersionCommand, int>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IApplicationDbContext _context;

    public SaveVersionCommandHandler(
        IDocumentRepository documentRepository,
        IApplicationDbContext context)
    {
        _documentRepository = documentRepository;
        _context = context;
    }

    public async Task<int> Handle(SaveVersionCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), request.DocumentId);

        var version = document.SaveVersion(request.Comment, request.UserId.ToString());

        _context.DocumentVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);

        return version.VersionNumber;
    }
}
