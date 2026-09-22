using Microsoft.Extensions.DependencyInjection;
using Providers.Application.Mappings;
using Providers.Application.DTOs.Providers;
using Providers.Domain.Entities;
using Shared.Kernel.Interfaces;

namespace Providers.Application;

/// <summary>
/// Registro de dependências da camada Application do módulo Providers
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddProvidersApplication(this IServiceCollection services)
    {
        // Configurar mappings do Mapster
        ProviderMappingConfig.Configure();

        // Mappers
        services.AddSingleton<IDomainMapper<Provider, ProviderOutputDTO>, ProviderMapper>();

        return services;
    }
}
