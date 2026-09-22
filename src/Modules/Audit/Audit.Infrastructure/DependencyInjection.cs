using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Audit.Domain.Repositories;
using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Repositories;

namespace Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContextEFCore>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DBconnect"),
                sqlOptions => 
                {
                    sqlOptions.CommandTimeout(120);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
        });

        services.AddScoped<IAuditLogRepository, AuditLogRepositoryEFCore>();

        return services;
    }
}
