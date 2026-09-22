using Shared.Kernel.Responses;

namespace Audit.Application.Services;

/// <summary>
/// Interface do serviço de auditoria
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Registra uma response de forma assíncrona (via Hangfire)
    /// NÃO recebe HttpContext para evitar problemas de serialização
    /// </summary>
    void LogResponseAsync(
        ResponseDTO response,
        string? requestId,
        string operation,
        string? ipAddress,
        string? userAgent,
        int statusCode,
        string? requestBody = null,
        string? responseJson = null,
        string? userId = null,
        string? username = null);
}
