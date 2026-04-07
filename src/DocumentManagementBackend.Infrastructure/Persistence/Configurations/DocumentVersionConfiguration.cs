using DocumentManagementBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagementBackend.Infrastructure.Persistence.Configurations;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Title).HasMaxLength(500).IsRequired();
        builder.Property(v => v.Description).HasMaxLength(2000);
        builder.Property(v => v.FileName).HasMaxLength(500).IsRequired();
        builder.Property(v => v.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Comment).HasMaxLength(1000);
        builder.Property(v => v.CreatedBy).HasMaxLength(256);
        builder.Property(v => v.Status).HasConversion<string>();

        builder.HasIndex(v => v.DocumentId);
        builder.HasIndex(v => new { v.DocumentId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.Document)
            .WithMany(d => d.Versions)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
