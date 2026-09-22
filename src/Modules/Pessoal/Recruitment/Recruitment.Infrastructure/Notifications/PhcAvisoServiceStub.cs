using Microsoft.Extensions.Logging;
using Recruitment.Application.Services;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;

namespace Recruitment.Infrastructure.Notifications;

/// <summary>
/// BR-05 stub for native XcUtil.criaAvs.
/// Records the aviso intent; does not invent a parallel product notification channel.
/// Wire a PHC CS script/COM hook in ops when available from this host.
/// </summary>
public sealed class PhcAvisoServiceStub : IPhcAvisoService
{
    private readonly ILogger<PhcAvisoServiceStub> _logger;

    public PhcAvisoServiceStub(ILogger<PhcAvisoServiceStub> logger)
    {
        _logger = logger;
    }

    public Task EmitAnalisarCompletedAsync(
        string rctStamp,
        string cveStamp,
        string estadoIa,
        IReadOnlyList<RctInterveniente> recipients,
        CancellationToken ct = default)
    {
        ForbiddenTitleCheck(estadoIa);

        var users = recipients.Count == 0
            ? "(lab GO — dono nomeado fora da RCTCLB)"
            : string.Join(",", recipients.Select(r => r.UserId));

        // Metadata only — never log CV body (BR-08).
        _logger.LogInformation(
            "BR-05 criaAvs stub: rct={Rct} cve={Cve} estado_ia={Estado} recipients={Recipients}. " +
            "Hook PHC: XcUtil.criaAvs (homepage). Canal paralelo proibido.",
            rctStamp, cveStamp, estadoIa, users);

        return Task.CompletedTask;
    }

    private static void ForbiddenTitleCheck(string estadoIa)
    {
        // Ensure we never emit AH-04 phrases even in stub titles.
        var title = $"Analise IA concluida ({estadoIa}). Score e input ao RH.";
        if (HitlCopy.ForbiddenPhrases.Any(p => title.Contains(p, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("AH-04");
    }
}
