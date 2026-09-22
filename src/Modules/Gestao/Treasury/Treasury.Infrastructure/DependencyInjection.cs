using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Treasury.Domain.Repositories;
using Treasury.Infrastructure.Persistence;
using Treasury.Infrastructure.Repositories;

namespace Treasury.Infrastructure;

/// <summary>
/// Infrastructure DI for the Treasury module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTreasuryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TreasuryDbContext>((sp, options) =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                .EnableSensitiveDataLogging(environment.IsDevelopment())
                .EnableDetailedErrors(environment.IsDevelopment());
        });

        services.AddScoped<ITreasuryAccountRepository, TreasuryAccountRepositoryEFCore>();
        services.AddScoped<IBankReconciliationRepository, BankReconciliationRepositoryEFCore>();

        return services;
    }
}
