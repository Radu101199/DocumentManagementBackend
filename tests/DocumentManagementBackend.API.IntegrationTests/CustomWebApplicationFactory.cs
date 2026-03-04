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

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            // Seed regular user
            var testUser = User.Create(
                Email.Create("user@test.com"),
                "Test", "User",
                BCrypt.Net.BCrypt.HashPassword("password123"),
                UserRole.User);
            TestUserId = testUser.Id;
            db.Users.Add(testUser);

            // Seed admin user
            var adminUser = User.Create(
                Email.Create("admin@test.com"),
                "Admin", "User",
                BCrypt.Net.BCrypt.HashPassword("password123"),
                UserRole.Admin);
            TestAdminId = adminUser.Id;
            db.Users.Add(adminUser);

            db.SaveChanges();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection?.Dispose();
    }
}