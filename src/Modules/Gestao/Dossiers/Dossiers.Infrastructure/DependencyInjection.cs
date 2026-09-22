using Dossiers.Domain.ExternalServices;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.ExternalServices;
using Dossiers.Infrastructure.Persistence;
using Dossiers.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Abstractions.ExternalServices;

namespace Dossiers.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDossiersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DossiersDbContextEFCore>((sp, options) =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();

            // ✅ Configure provider with a fallback connection string
            // This is used only if TenantContext doesn't have credentials
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                .EnableSensitiveDataLogging(environment.IsDevelopment())
                .EnableDetailedErrors(environment.IsDevelopment());
        });

        // Registrar HttpClient para PHC WEB
        services.AddHttpClient<IPhcWebServiceDossiers, PhcWebService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Registrar PHC WEB Credentials Provider
        services.AddScoped<IPhcWebCredentialsProvider, PhcWebCredentialsProvider>();

        services.AddScoped<IDossierRepository, DossierRepositoryEFCore>();
        services.AddScoped<IDossierClientRepository, DossierClientRepositoryEFCore>();
        services.AddScoped<ISupplierRepository, SupplierRepositoryEFCore>();
        services.AddScoped<IEntityRepository, EntityRepositoryEFCore>();
        services.AddScoped<IContactRepository, ContactRepositoryEFCore>();
        services.AddScoped<ICl2Repository, Cl2RepositoryEFCore>();
        services.AddScoped<IStockRepository, StockRepositoryEFCore>();
        services.AddScoped<IMoedaRepository, MoedaRepositoryEFCore>();
        services.AddScoped<ITaxasIvaRepository, TaxasIvaRepositoryEFCore>();
        services.AddScoped<ITipoDossierRepository, TipoDossierRepositoryEFCore>();

        return services;
    }
}
