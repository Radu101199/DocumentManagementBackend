using Microsoft.EntityFrameworkCore;
using DocumentManagementBackend.Domain.Entities;

namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Document> Documents { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}