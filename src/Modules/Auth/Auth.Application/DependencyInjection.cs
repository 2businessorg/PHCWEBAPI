using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application;

/// <summary>
/// Registro de dependências da camada Application do módulo Auth
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        // MediatR handlers são registrados automaticamente via reflection no Presentation
        // Nenhum serviço adicional necessário aqui pois usamos CQRS puro
        
        return services;
    }
}
