using Advances.Domain.ExternalServices;
using Advances.Domain.Repositories;
using Advances.Infrastructure.ExternalServices;
using Advances.Infrastructure.Persistence;
using Advances.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Abstractions.ExternalServices;

namespace Advances.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure do módulo Advances
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAdvancesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AdvancesDbContext>((sp, options) =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                .EnableSensitiveDataLogging(environment.IsDevelopment())
                .EnableDetailedErrors(environment.IsDevelopment());
        });

        // PHC WEB integration
        services.AddHttpClient<IPhcWebServiceAdvances, PhcWebService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        services.AddScoped<IPhcWebCredentialsProvider, PhcWebCredentialsProvider>();

        // Repositories
        services.AddScoped<IAdvanceRepository, AdvanceRepositoryEFCore>();
        services.AddScoped<IAdvanceTypeRepository, AdvanceTypeRepositoryEFCore>();
        services.AddScoped<IClientRepository, ClientRepositoryEFCore>();

        return services;
    }
}
