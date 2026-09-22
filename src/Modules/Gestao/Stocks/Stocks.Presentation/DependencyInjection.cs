using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stocks.Application;
using Stocks.Infrastructure;
using Stocks.Presentation.REST;

namespace Stocks.Presentation;

/// <summary>
/// Registro de dependências da camada Presentation.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddStocksPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStocksApplication();
        services.AddStocksInfrastructure(configuration);
        services.AddStocksRestPresentation();
        return services;
    }
}
