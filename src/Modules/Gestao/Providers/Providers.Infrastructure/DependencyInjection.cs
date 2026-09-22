using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Providers.Domain.Repositories;
using Providers.Domain.Services;
using Providers.Infrastructure.Caching;
using Providers.Infrastructure.Caching.Configuration;
using Providers.Infrastructure.Persistence;
using Providers.Infrastructure.Repositories;
using Providers.Infrastructure.Services;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Providers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProvidersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core Interceptors (do Shared)
        services.AddSingleton<AuditableEntityInterceptorEFCore>();

        // EF Core DbContext (Database First - sem migrations)
        services.AddDbContext<ProvidersDbContextEFCore>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptorEFCore>();

            // ✅ Configure provider with a fallback connection string
            // This is used only if TenantContext doesn't have credentials
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                   .AddInterceptors(auditInterceptor);
        });

        // Caching Configuration
        services.Configure<ProviderCachingOptions>(
            configuration.GetSection("Caching:Providers"));

        // Memory Cache with size limit
        var cachingOptions = configuration.GetSection("Caching:Providers").Get<ProviderCachingOptions>();
        if (cachingOptions?.Enabled == true)
        {
            services.AddMemoryCache(options =>
            {
                // Convert MB to bytes and set as size limit
                options.SizeLimit = cachingOptions.MaxCacheSizeMB * 1024 * 1024;
                options.CompactionPercentage = 0.25; // Compact 25% when limit reached
            });

            // Cache Policy (Strategy Pattern)
            services.AddSingleton<IProviderCachePolicy, ConfigurableProviderCachePolicy>();

            // Repositories - Decorator Pattern
            // 1. Register the actual EF Core implementation
            services.AddScoped<ProviderRepositoryEFCore>();
            
            // 2. Register IProviderRepository as the decorated version
            services.AddScoped<IProviderRepository>(sp =>
            {
                var innerRepository = sp.GetRequiredService<ProviderRepositoryEFCore>();
                var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                var policy = sp.GetRequiredService<IProviderCachePolicy>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CachedProviderRepository>>();
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ProviderCachingOptions>>();

                return new CachedProviderRepository(innerRepository, cache, policy, logger, options);
            });

            // Pre-warming Service
            if (cachingOptions.EnablePreWarming)
            {
                services.AddHostedService<ProviderCachePreWarmingService>();
            }
        }
        else
        {
            // No caching - register repository directly
            services.AddScoped<IProviderRepository, ProviderRepositoryEFCore>();
        }

        // ProviderValue Repository (no caching for now - could be added later)
        services.AddScoped<IProviderValueRepository, ProviderValueRepositoryEFCore>();

        // Encryption Service
        var encryptionKey = configuration["Encryption:Key"] ?? "PHC_DEFAULT_ENCRYPTION_KEY_2024_SECURE";
        services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));

        return services;
    }
}
