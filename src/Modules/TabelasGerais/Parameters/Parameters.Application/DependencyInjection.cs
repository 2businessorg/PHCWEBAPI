using Microsoft.Extensions.DependencyInjection;
using Parameters.Application.Mappings;
using Parameters.Application.DTOs.Parameters;
using Parameters.Domain.Entities;
using Shared.Kernel.Interfaces;

namespace Parameters.Application;

/// <summary>
/// Registro de dependências da camada Application do módulo Parameters
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddParametersApplication(this IServiceCollection services)
    {
        // Mappers
        services.AddSingleton<IDomainMapper<Para1, ParameterOutputDTO>, Para1Mapper>();

        return services;
    }
}
