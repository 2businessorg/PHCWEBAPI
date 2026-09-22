using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Audit.Application;
using Audit.Presentation.REST;
using Audit.Infrastructure;

namespace Audit.Presentation;

/// <summary>
/// Configuração de dependências do módulo Audit (Presentation Layer)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuditPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableRest = true)
    {
        // Application Layer (Mappers + MediatR)
        services.AddAuditApplication();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.Features.GetAllAuditLogs.GetAllAuditLogsQuery).Assembly);
        });

        // Infrastructure Layer
        services.AddAuditInfrastructure(configuration);

        // Presentation Layer (REST)
        if (enableRest)
        {
            services.AddAuditRestApi();
        }

        return services;
    }
}
