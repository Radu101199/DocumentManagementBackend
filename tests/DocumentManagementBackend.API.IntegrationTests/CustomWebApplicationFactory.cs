using DocumentManagementBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _connection;
    public Guid TestUserId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1️⃣ Remove ALL existing DbContext registrations
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();
            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            // 2️⃣ Remove any singleton DbContext or factory if exists
            var dbContextDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // 3️⃣ Add SQLite in-memory
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            
            // SQLite FK checks are off by default but EF enables them - turn them off for tests
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                cmd.ExecuteNonQuery();
            }
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // 4️⃣ Build service provider and ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            
            TestUserId = Guid.NewGuid();
            // inside ConfigureWebHost, after db.Database.EnsureCreated():
            var testUser = User.Create(
                Email.Create("test@test.com"),
                "Test",
                "User",
                "hashed_password",
                UserRole.User
            );
            TestUserId = testUser.Id;
            db.Users.Add(testUser);
            db.SaveChanges();
            
            services.AddLogging(logging => logging.AddConsole());
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}