using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Clients.Domain.Repositories;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Repositories;

namespace Clients.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddClientsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext (EF Core - Database First)
        // ⚠️ IMPORTANT: Do NOT configure connection string here
        // The connection string is dynamically resolved in OnConfiguring via ITenantContext
        // which reads the encrypted credentials from the JWT token
        services.AddDbContext<ClientsDbContextEFCore>((sp, options) =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();

            // ✅ Configure provider with a fallback connection string
            // This is used only if TenantContext doesn't have credentials
            // In production, every request should have a valid JWT with tenant credentials
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                   .EnableSensitiveDataLogging(false)
                   .EnableDetailedErrors(false);

            // Log de SQL em desenvolvimento ou durante debug attach
            if (environment.IsDevelopment() || Debugger.IsAttached)
            {
                options.LogTo(
                        Console.WriteLine,
                        new[] { DbLoggerCategory.Database.Command.Name },
                        LogLevel.Information)
                    .LogTo(
                        message => Debug.WriteLine(message),
                        new[] { DbLoggerCategory.Database.Command.Name },
                        LogLevel.Information)
                    .EnableDetailedErrors(true)
                    .EnableSensitiveDataLogging(true);

            }
        });

        // Repositories (EF Core implementation)
        services.AddScoped<IClientRepository, ClientRepositoryEFCore>();

        // PHC Web integration service
        services.AddHttpClient<Clients.Domain.ExternalServices.IPhcWebServiceClients, Clients.Infrastructure.ExternalServices.PhcWebService>();

        return services;
    }
}
