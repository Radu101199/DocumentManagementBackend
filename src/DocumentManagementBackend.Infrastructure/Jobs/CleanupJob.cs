using DocumentManagementBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocumentManagementBackend.Infrastructure.Jobs;

public class CleanupJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(ApplicationDbContext context, ILogger<CleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task PurgeOldDeletedDocumentsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var oldDeleted = await _context.Documents
            .IgnoreQueryFilters()
            .Where(d => d.IsDeleted && d.DeletedAt < cutoff)
            .ToListAsync();

        if (oldDeleted.Count == 0)
        {
            _logger.LogInformation("No documents to purge");
            return;
        }

        _context.Documents.RemoveRange(oldDeleted);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Purged {Count} old deleted documents", oldDeleted.Count);
    }
}
