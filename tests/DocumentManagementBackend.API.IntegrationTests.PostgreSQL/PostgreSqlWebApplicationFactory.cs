using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Entities;
using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.ValueObjects;
using DocumentManagementBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

public class PostgreSqlWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public Guid TestUserId { get; private set; }
    public Guid TestAdminId { get; private set; }
    public string ConnectionString => _postgres.GetConnectionString() + ";Search Path=DocumentManagement,public";

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Înlocuiește DbContext cu PostgreSQL real
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            var appDbDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IApplicationDbContext));
            if (appDbDescriptor != null)
                services.Remove(appDbDescriptor);

            services.AddMemoryCache();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Rulează migrații reale pe PostgreSQL
            db.Database.Migrate();

            // Seed
            var testUser = User.Create(
                Email.Create("user@test.com"),
                "Test", "User",
                BCrypt.Net.BCrypt.HashPassword("password123"),
                UserRole.User);
            TestUserId = testUser.Id;
            db.Users.Add(testUser);

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
}
