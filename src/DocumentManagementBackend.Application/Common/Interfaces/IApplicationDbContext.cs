using Microsoft.EntityFrameworkCore;
using DocumentManagementBackend.Domain.Entities;

namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Document> Documents { get; }
    DbSet<AuditLog> AuditLogs { get;  }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}