using Microsoft.Extensions.DependencyInjection;
using Receipts.Presentation.REST.Services;

namespace Receipts.Presentation.REST;

/// <summary>
/// Registro de dependências REST do módulo Receipts
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddReceiptsRestPresentation(this IServiceCollection services)
    {
        services.AddScoped<IReceiptsLinkBuilder, ReceiptsLinkBuilder>();
        return services;
    }
}
