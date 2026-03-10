using DocumentManagementBackend.Domain.Enums;

namespace DocumentManagementBackend.Application.Features.Documents.Queries.GetDocuments;

public record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    DocumentStatus Status,
    Guid OwnerId,
    string OwnerFullName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ApprovalRequestedAt,
    DateTime? ApprovalExpiresAt
);