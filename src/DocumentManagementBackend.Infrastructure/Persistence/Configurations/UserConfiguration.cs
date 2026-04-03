using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.ValueObjects;

namespace DocumentManagementBackend.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

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

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        // ✅ Roles cu ValueComparer — fix pentru warning
        var rolesComparer = new ValueComparer<List<UserRole>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            r => r.Aggregate(0, (acc, v) => HashCode.Combine(acc, v.GetHashCode())),
            r => r.ToList());

        builder.Property<List<UserRole>>("_roles")
            .HasColumnName("Roles")
            .HasConversion(
                roles => string.Join(",", roles.Select(r => r.ToString())),
                value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => Enum.Parse<UserRole>(r))
                    .ToList())
            .Metadata.SetValueComparer(rolesComparer);

        builder.Property<List<UserRole>>("_roles")
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Ignore(x => x.FullName);
        builder.Ignore(x => x.Roles);

        builder.HasMany(x => x.Documents)
            .WithOne(x => x.Owner)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
