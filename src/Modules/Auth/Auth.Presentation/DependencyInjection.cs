using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auth.Application;
using Auth.Infrastructure;

namespace Auth.Presentation;

/// <summary>
/// Configuração de dependências do módulo Auth (Presentation Layer)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuthPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Application Layer
        services.AddAuthApplication();

        // MediatR - registra todos os handlers do assembly Auth.Application
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Auth.Application.Features.Login.LoginCommand).Assembly);
        });

        // Infrastructure Layer (Identity + JWT + Repositories)
        services.AddAuthInfrastructure(configuration);

        return services;
    }
}
