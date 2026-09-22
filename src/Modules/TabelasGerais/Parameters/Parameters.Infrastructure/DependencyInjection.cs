using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parameters.Domain.Repositories;
using Parameters.Infrastructure.Persistence;
using Parameters.Infrastructure.Repositories;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Parameters.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddParametersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core Interceptors (do Shared)
        services.AddSingleton<AuditableEntityInterceptorEFCore>();

        // EF Core DbContext (Database First - sem migrations)
        services.AddDbContext<ParametersDbContextEFCore>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptorEFCore>();

            // ✅ Configure provider with a fallback connection string
            // This is used only if TenantContext doesn't have credentials
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                   .AddInterceptors(auditInterceptor);
        });

        // Repositories - EF Core implementation
        services.AddScoped<IPara1Repository, Para1RepositoryEFCore>();
        services.AddScoped<ICbRepository, CbRepositoryEFCore>();

        return services;
    }
}

