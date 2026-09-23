using Recruitment.Domain.Entities;

namespace Recruitment.Domain.Repositories;

public interface IRctVacancyRepository
{
    /// <summary>
    /// Maps the public vacancy id (rct.idrct, a PHC string) to rctstamp.
    /// Null when the id is empty, longer than 50 characters, or the vacancy does not exist.
    /// </summary>
    Task<string?> ResolveStampByIdAsync(string idRct, CancellationToken ct = default);
}

public interface IRctCriteriaRepository
{
    /// <summary>Usable crt rows for an RCT (weight &gt; 0). Empty ⇒ enqueue forbidden (BR-01).</summary>
    Task<IReadOnlyList<RctCriterion>> GetUsableCriteriaAsync(string rctStamp, CancellationToken ct = default);

    Task<int> CountUsableCriteriaAsync(string rctStamp, CancellationToken ct = default);
}

public interface IRctIntervenienteRepository
{
    /// <summary>RCTCLB rows with a PHC user (BR-01 / BR-05).</summary>
    Task<IReadOnlyList<RctInterveniente>> GetIntervenientesAsync(string rctStamp, CancellationToken ct = default);

    Task<int> CountIntervenientesAsync(string rctStamp, CancellationToken ct = default);
}

public interface ICvAnexoRepository
{
    Task<CvAnexo?> GetCvAnexoAsync(string cveStamp, CancellationToken ct = default);

    Task SaveTextoAsync(string anexoStamp, string texto, CancellationToken ct = default);
}

public interface ISrtScoreRepository
{
    Task<SrtCandidateRow?> GetByStampAsync(string srtStamp, CancellationToken ct = default);

    Task<IReadOnlyList<SrtCandidateRow>> GetByRctAsync(string rctStamp, CancellationToken ct = default);

    /// <summary>
    /// SRT rows of the vacancy that have a CV attachment
    /// (anexos.oritable=cve, tipo=CV, non-empty bytes).
    /// </summary>
    Task<IReadOnlyList<SrtCandidateRow>> ListWithCvByRctAsync(string rctStamp, CancellationToken ct = default);

    /// <summary>Counts of cve.u_estadoia across SRT rows of the vacancy.</summary>
    Task<IaEstadoCounts> CountEstadosByRctAsync(string rctStamp, CancellationToken ct = default);

    /// <summary>
    /// Persists IA score + justification on SRT only (BR-02).
    /// Must not touch condp or selection/apurado/entrevista state (BR-03 / BR-04).
    /// </summary>
    Task SaveScoreAsync(
        string srtStamp,
        decimal score,
        string justificationJson,
        string? modelo,
        string? promptVer,
        DateTime stampUtc,
        string? auditJson,
        CancellationToken ct = default);

    /// <summary>Clears IA score fields on OCR failure (AH-08) — still never touches selection state.</summary>
    Task ClearScoreOnOcrErrorAsync(string srtStamp, CancellationToken ct = default);
}

public interface ICveEstadoIaRepository
{
    Task SetEstadoIaAsync(string cveStamp, string estadoIa, CancellationToken ct = default);

    Task<string?> GetEstadoIaAsync(string cveStamp, CancellationToken ct = default);
}

public interface IRecruitmentOutboxRepository
{
    Task<long?> TryEnqueueAsync(RecruitmentOutboxItem item, CancellationToken ct = default);

    Task<RecruitmentOutboxItem?> GetAsync(long id, CancellationToken ct = default);

    Task MarkProcessingAsync(long id, CancellationToken ct = default);

    Task HeartbeatAsync(long id, CancellationToken ct = default);

    Task MarkDoneAsync(long id, CancellationToken ct = default);

    Task MarkErrorAsync(long id, string errorMessage, CancellationToken ct = default);

    Task<IReadOnlyList<RecruitmentOutboxItem>> GetOrphansAsync(TimeSpan ttl, CancellationToken ct = default);
}
