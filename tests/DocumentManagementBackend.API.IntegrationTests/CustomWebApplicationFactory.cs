using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DocumentManagementBackend.Infrastructure.Persistence;

namespace DocumentManagementBackend.API.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var dbContextDescriptor2 = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            
            if (dbContextDescriptor2 != null)
                services.Remove(dbContextDescriptor2);

            // Add InMemory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}");
            });

            // Build service provider and create database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            db.Database.EnsureCreated();
        });
    }
}