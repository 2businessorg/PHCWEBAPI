using Microsoft.Extensions.DependencyInjection;
using Audit.Application.Services;
using Audit.Application.Mappings;
using Audit.Application.DTOs;
using Audit.Domain.Entities;
using Shared.Kernel.Interfaces;

namespace Audit.Application;

/// <summary>
/// Registro de dependências da camada Application do módulo Audit
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuditApplication(this IServiceCollection services)
    {
        // Mappers
        services.AddSingleton<IDomainMapper<AuditLog, AuditLogOutputDTO>, AuditLogMapper>();

        // Services
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
