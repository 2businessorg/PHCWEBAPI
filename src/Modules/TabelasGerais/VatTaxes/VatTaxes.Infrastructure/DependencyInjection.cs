using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VatTaxes.Domain;
using VatTaxes.Infrastructure.Persistence;
using VatTaxes.Infrastructure.Repositories;
using Shared.Kernel.MultiTenancy;

namespace VatTaxes.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure do módulo VatTaxes
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddVatTaxesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext com suporte a tenant
        services.AddDbContext<VatTaxesDbContext>((provider, options) =>
        {
            var tenantContext = provider.GetRequiredService<ITenantContext>();
            var connectionString = tenantContext.GetConnectionString();
            
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsAssembly(typeof(VatTaxesDbContext).Assembly.FullName));
        });

        // Repositories
        services.AddScoped<IVatTaxRepository, VatTaxRepository>();

        return services;
    }
}
