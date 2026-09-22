using Shared.Kernel.Extensions;

namespace PHCAPI.Host.Middleware;

/// <summary>
/// Middleware para adicionar Correlation ID a cada request
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Gerar ou obter Correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);

        // Adicionar ao HttpContext.Items para acesso posterior
        context.Items[CorrelationIdExtensions.CorrelationIdKey] = correlationId;

        // Adicionar ao response header
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdExtensions.CorrelationIdHeader))
            {
                context.Response.Headers[CorrelationIdExtensions.CorrelationIdHeader] = correlationId.ToString();
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static decimal GetOrGenerateCorrelationId(HttpContext context)
    {
        // Verifica se o cliente enviou um Correlation ID
        if (context.Request.Headers.TryGetValue(CorrelationIdExtensions.CorrelationIdHeader, out var headerValue) &&
            decimal.TryParse(headerValue.ToString(), out var clientCorrelationId))
        {
            return clientCorrelationId;
        }

        // Gera novo Correlation ID baseado em timestamp
        return GenerateCorrelationId();
    }

    private static decimal GenerateCorrelationId()
    {
        // Timestamp em ticks como ID único (número inteiro)
        var ticks = DateTime.UtcNow.Ticks;

        // Retorna como decimal inteiro (sem casas decimais)
        // Usa módulo para evitar overflow mantendo número grande mas gerenciável
        return ticks % 100000000000000; // 14 dígitos inteiros
    }
}
