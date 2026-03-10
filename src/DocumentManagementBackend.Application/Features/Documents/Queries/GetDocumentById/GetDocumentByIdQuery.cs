using DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;
using MediatR;

namespace DocumentManagementBackend.Application.Features.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery(Guid Id) : IRequest<DocumentDto>;