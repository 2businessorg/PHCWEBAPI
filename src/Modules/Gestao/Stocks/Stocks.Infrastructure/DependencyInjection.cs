using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.ExternalServices;
using Stocks.Domain.ExternalServices;
using Stocks.Domain.Repositories;
using Stocks.Infrastructure.ExternalServices;
using Stocks.Infrastructure.Persistence;
using Stocks.Infrastructure.Repositories;
using System.Diagnostics;

namespace Stocks.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddStocksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StocksDbContextEFCore>((sp, options) =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();

            // ✅ Configure provider with a fallback connection string
            // This is used only if TenantContext doesn't have credentials
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                   .EnableSensitiveDataLogging(false)
                   .EnableDetailedErrors(false);

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

        services.AddScoped<IStockRepository, StockRepositoryEFCore>();
        services.AddHttpClient<IPhcWebServiceStocks, PhcWebService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        return services;
    }
}
