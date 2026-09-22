using Agent.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Application;

/// <summary>
/// Application-layer DI for the Agent module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAgentApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<ITreasuryAgent, TreasuryAgent>();

        return services;
    }
}
