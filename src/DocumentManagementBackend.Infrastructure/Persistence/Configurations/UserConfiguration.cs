using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.ValueObjects;

namespace DocumentManagementBackend.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        // Email as Value Object - store as string
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        // Status instead of IsActive
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        // Roles as comma-separated string - access via backing field
        builder.Property<List<UserRole>>("_roles")
            .HasColumnName("Roles")
            .HasConversion(
                roles => string.Join(",", roles.Select(r => r.ToString())),
                value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => Enum.Parse<UserRole>(r))
                    .ToList())
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        // Ignore computed property
        builder.Ignore(x => x.FullName);
        
        // Ignore the public Roles property
        builder.Ignore(x => x.Roles);

        // Relationship: User -> Documents (one to many)
        builder.HasMany(x => x.Documents)
            .WithOne(x => x.Owner)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}