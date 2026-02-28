using DocumentManagementBackend.Domain.Interfaces;
using DocumentManagementBackend.Infrastructure.Persistence;
using DocumentManagementBackend.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string environment)
    {
        if (environment != "Testing")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }

        // Always register repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }
}