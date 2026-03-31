using DocumentManagementBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagementBackend.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(20).IsRequired();
        builder.Property(a => a.ChangedBy).HasMaxLength(256);
        builder.Property(a => a.OldValues).HasColumnType("jsonb");
        builder.Property(a => a.NewValues).HasColumnType("jsonb");
        builder.Property(a => a.AffectedColumns).HasColumnType("jsonb");

        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.ChangedAt);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
    }
}
