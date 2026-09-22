namespace Shared.Kernel.Extensions;

/// <summary>
/// Extension methods para Correlation ID
/// </summary>
public static class CorrelationIdExtensions
{
    public const string CorrelationIdKey = "CorrelationId";
    public const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Obtém o Correlation ID do HttpContext atual
    /// </summary>
    public static decimal? GetCorrelationId(this Microsoft.AspNetCore.Http.HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdKey, out var correlationId) &&
            correlationId is decimal id)
        {
            return id;
        }
        return null;
    }
}
