using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.ValueObjects;
using DocumentManagementBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _connection = null!;
    public Guid TestUserId { get; private set; }
    public Guid TestAdminId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                    .ToList();
                foreach (var descriptor in descriptors)
                    services.Remove(descriptor);

                // ✅ Scoate IApplicationDbContext
                var appDbContextDescriptor = services
                    .FirstOrDefault(d => d.ServiceType == typeof(IApplicationDbContext));
                if (appDbContextDescriptor != null)
                    services.Remove(appDbContextDescriptor);

                // ✅ Scoate CurrentUserService real
                var currentUserDescriptor = services
                    .FirstOrDefault(d => d.ServiceType == typeof(ICurrentUserService));
                if (currentUserDescriptor != null)
                    services.Remove(currentUserDescriptor);

                services.AddMemoryCache();

                // Add SQLite in-memory
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                    cmd.ExecuteNonQuery();
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(_connection));

                services.AddScoped<IApplicationDbContext>(provider =>
                    provider.GetRequiredService<ApplicationDbContext>());

                // ✅ Seed PRIMUL — ca să avem TestUserId setat
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                var testUser = User.Create(
                    Email.Create("user@test.com"),
                    "Test", "User",
                    BCrypt.Net.BCrypt.HashPassword("password123"),
                    UserRole.User);
                TestUserId = testUser.Id;  // ← setat ACUM
                db.Users.Add(testUser);

                var adminUser = User.Create(
                    Email.Create("admin@test.com"),
                    "Admin", "User",
                    BCrypt.Net.BCrypt.HashPassword("password123"),
                    UserRole.Admin);
                TestAdminId = adminUser.Id;
                db.Users.Add(adminUser);

                db.SaveChanges();

                // ✅ Înregistrează mock-ul DUPĂ ce TestUserId e setat
                services.AddScoped<ICurrentUserService>(_ =>
                    new TestCurrentUserService(TestUserId.ToString()));
            });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection?.Dispose();
    }
}