using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Interfaces;
using DocumentManagementBackend.Infrastructure.Auth;
using DocumentManagementBackend.Infrastructure.Jobs;
using DocumentManagementBackend.Infrastructure.Persistence;
using DocumentManagementBackend.Infrastructure.Persistence.Interceptors;
using DocumentManagementBackend.Infrastructure.Persistence.Repositories;
using DocumentManagementBackend.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
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
            services.AddDistributedMemoryCache();
        }
        services.AddScoped<ICacheService, CacheService>();

        // Hangfire — doar în afara Testing
        if (environment != "Testing")
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(connectionString)));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2;
                options.Queues = new[] { "critical", "default", "low" };
            });

            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
        }
        else
        {
            // Fallback pentru teste — no-op implementation
            services.AddScoped<IBackgroundJobService, NoOpBackgroundJobService>();
        }

        services.AddScoped<EmailJob>();
        services.AddScoped<CleanupJob>();

        return services;
    }
}
