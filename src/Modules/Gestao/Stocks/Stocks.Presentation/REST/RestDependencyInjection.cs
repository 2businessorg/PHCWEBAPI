using Microsoft.Extensions.DependencyInjection;
using Stocks.Presentation.REST.Services;

namespace Stocks.Presentation.REST;

/// <summary>
/// Registro de dependências REST API do módulo Stocks.
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddStocksRestPresentation(this IServiceCollection services)
    {
        services.AddScoped<IStocksLinkBuilder, StocksLinkBuilder>();
        return services;
    }
}
