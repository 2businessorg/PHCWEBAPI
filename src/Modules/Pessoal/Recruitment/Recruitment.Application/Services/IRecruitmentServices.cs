using Recruitment.Domain.Entities;

namespace Recruitment.Application.Services;

/// <summary>
/// BR-05: native PHC aviso via XcUtil.criaAvs after Analisar.
/// When PHC hook is unavailable from this API host, implementations must still record the attempt.
/// Parallel notification channels as product are forbidden.
/// </summary>
public interface IPhcAvisoService
{
    Task EmitAnalisarCompletedAsync(
        string rctStamp,
        string cveStamp,
        string estadoIa,
        IReadOnlyList<RctInterveniente> recipients,
        CancellationToken ct = default);
}

public interface IRecruitmentEnqueueService
{
    Task<EnqueueDecision> TryEnqueueAsync(
        string cveStamp,
        string rctStamp,
        string srtStamp,
        string? labGoRef = null,
        CancellationToken ct = default);
}

public sealed class EnqueueDecision
{
    public bool Enqueued { get; init; }

    public long? OutboxId { get; init; }

    public string Message { get; init; } = string.Empty;

    public int CriteriaCount { get; init; }

    public int IntervenienteCount { get; init; }
}
