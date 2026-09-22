using Shared.Kernel.Responses;
using Microsoft.Extensions.Logging;
using Hangfire;
using Audit.Application.Jobs;

namespace Audit.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IBackgroundJobClient backgroundJobClient,
        ILogger<AuditLogService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public void LogResponseAsync(
        ResponseDTO response,
        string? requestId,
        string operation,
        string? ipAddress,
        string? userAgent,
        int statusCode,
        string? requestBody = null,
        string? responseJson = null,
        string? userId = null,
        string? username = null)
    {
        try
        {
            if (response?.Response == null)
            {
                _logger.LogWarning("Response or Response.Response is null for operation: {Operation}", operation);
                return;
            }

            var code = response.Response.Code;
            var responseDesc = response.Response.Description;
            
            _backgroundJobClient.Enqueue<SaveAuditLogJob>(job =>
                job.ExecuteAsync(
                    code,
                    requestId,
                    responseDesc,
                    operation,
                    requestBody,
                    responseJson,
                    ipAddress,
                    userId,
                    username
                ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue audit log for operation: {Operation}", operation);
        }
    }
}
