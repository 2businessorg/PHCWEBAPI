using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Receipts.Application;
using Receipts.Infrastructure;
using Receipts.Presentation.REST;

namespace Receipts.Presentation;

/// <summary>
/// Ponto de entrada de registo de todas as camadas do módulo Receipts
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddReceiptsPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddReceiptsApplication();
        services.AddReceiptsInfrastructure(configuration);
        services.AddReceiptsRestPresentation();

        return services;
    }
}
