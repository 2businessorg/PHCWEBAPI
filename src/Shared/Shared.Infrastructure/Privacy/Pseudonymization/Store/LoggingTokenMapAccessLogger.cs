using Microsoft.Extensions.Logging;
using Shared.Abstractions.Privacy.Pseudonymization;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Store;

/// <summary>
/// Access-log stub: metadata only (no plaintext CV / original values).
/// </summary>
public sealed class LoggingTokenMapAccessLogger : ITokenMapAccessLogger
{
    private readonly ILogger<LoggingTokenMapAccessLogger> _logger;

    public LoggingTokenMapAccessLogger(ILogger<LoggingTokenMapAccessLogger> logger)
    {
        _logger = logger;
    }

    public Task LogAccessAsync(
        string sessionId,
        string documentId,
        string token,
        string operation,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "TokenMap access op={Operation} session={SessionId} document={DocumentId} token={Token}",
            operation,
            sessionId,
            documentId,
            token);
        return Task.CompletedTask;
    }
}
