using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Dossiers.Application;

/// <summary>
/// Registro de dependências da camada Application
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDossiersApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
