using Microsoft.Extensions.DependencyInjection;

namespace Audit.Presentation.REST;

/// <summary>
/// Configuração da API REST do módulo Audit
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddAuditRestApi(this IServiceCollection services)
    {
        // Controllers são registrados automaticamente via AddControllers() no Program.cs
        return services;
    }
}
