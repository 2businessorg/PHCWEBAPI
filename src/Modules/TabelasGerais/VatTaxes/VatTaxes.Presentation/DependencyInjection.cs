using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VatTaxes.Application;
using VatTaxes.Infrastructure;

namespace VatTaxes.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddVatTaxesPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Application Layer
        services.AddVatTaxesApplication();

        // Infrastructure Layer
        services.AddVatTaxesInfrastructure(configuration);

        return services;
    }
}
