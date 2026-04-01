using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Interfaces;
using DocumentManagementBackend.Infrastructure.Auth;
using DocumentManagementBackend.Infrastructure.Persistence;
using DocumentManagementBackend.Infrastructure.Persistence.Interceptors;
using DocumentManagementBackend.Infrastructure.Persistence.Repositories;
using DocumentManagementBackend.Infrastructure.Services;
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
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory", "DocumentManagement")));
        }

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<AuditInterceptor>();
        // Redis Cache
        var redisConnection = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "DocumentManagement:";
            });
        }
        else
        {
            // Fallback la in-memory când Redis nu e configurat (dev/test)
            services.AddDistributedMemoryCache();
        }
        services.AddScoped<ICacheService, CacheService>();
        return services;
    }
}