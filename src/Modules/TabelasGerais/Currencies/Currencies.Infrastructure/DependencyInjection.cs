using Currencies.Domain.Repositories;
using Currencies.Infrastructure.Persistence;
using Currencies.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel.MultiTenancy;

namespace Currencies.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure do módulo Currencies.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCurrenciesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CurrenciesDbContext>((provider, options) =>
        {
            var tenantContext = provider.GetRequiredService<ITenantContext>();
            var connectionString = tenantContext.GetConnectionString();

            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsAssembly(typeof(CurrenciesDbContext).Assembly.FullName));
        });

        services.AddScoped<ICurrencyRepository, CurrencyRepositoryEFCore>();
        services.AddScoped<IPara1Repository, Para1RepositoryEFCore>();
        services.AddScoped<ICbRepository, CbRepositoryEFCore>();

        return services;
    }
}
