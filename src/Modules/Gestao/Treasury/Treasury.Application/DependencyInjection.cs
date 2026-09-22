using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Treasury.Application.Matching;

namespace Treasury.Application;

/// <summary>
/// Application-layer DI for the Treasury module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTreasuryApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<IReconciliationMatcher, ReconciliationMatcher>();

        return services;
    }
}
