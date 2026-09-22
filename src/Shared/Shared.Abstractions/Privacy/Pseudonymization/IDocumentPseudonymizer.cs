namespace Shared.Abstractions.Privacy.Pseudonymization;

/// <summary>
/// Platform facade for reversible document pseudonymization (shared ownership).
/// Recruitment is the first consumer; do not re-implement inside module workers.
/// </summary>
public interface IDocumentPseudonymizer
{
    Task<PseudonymizationResult> PseudonymizeAsync(
        PseudonymizationRequest request,
        CancellationToken cancellationToken = default);

    Task<DetokenizationResult> DetokenizeAsync(
        DetokenizationRequest request,
        CancellationToken cancellationToken = default);
}

public interface IEntityDetector
{
    Task<IReadOnlyList<RawEntitySpan>> DetectAsync(
        string text,
        PseudonymizationPolicy policy,
        CancellationToken cancellationToken = default);
}

public interface IEntityResolver
{
    IReadOnlyList<ResolvedEntity> Resolve(IReadOnlyList<ScoredEntity> scored);
}

public interface ITokenMapStore
{
    Task UpsertAsync(TokenMapEntry entry, CancellationToken cancellationToken = default);

    Task<TokenMapEntry?> GetByTokenAsync(
        string sessionId,
        string documentId,
        string token,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TokenMapEntry>> ListBySessionDocumentAsync(
        string sessionId,
        string documentId,
        CancellationToken cancellationToken = default);

    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default);
}

public interface ILeakChecker
{
    Task<LeakCheckResult> CheckAsync(
        string candidateEgressText,
        PseudonymizationPolicy policy,
        IReadOnlyList<TokenMapEntry> sessionMap,
        Func<TokenMapEntry, string> decryptOriginal,
        CancellationToken cancellationToken = default);
}

public interface IDetokenizer
{
    Task<DetokenizationResult> DetokenizeAsync(
        DetokenizationRequest request,
        ITokenMapStore map,
        Func<TokenMapEntry, string> decryptOriginal,
        PseudonymizationPolicy policy,
        CancellationToken cancellationToken = default);
}

public interface IPseudonymizationStep
{
    string Name { get; }

    Task ExecuteAsync(PseudonymizationPipelineContext context, CancellationToken cancellationToken);
}

/// <summary>Mutable pipeline bag shared across strategy steps.</summary>
public sealed class PseudonymizationPipelineContext
{
    public required PseudonymizationRequest Request { get; init; }
    public string NormalizedText { get; set; } = string.Empty;
    public List<RawEntitySpan> RawSpans { get; } = new();
    public List<ScoredEntity> Scored { get; } = new();
    public List<ResolvedEntity> Resolved { get; } = new();
    public List<DetectedEntitySummary> Summaries { get; } = new();
    public List<TokenMapEntry> MapEntries { get; } = new();
    public string PseudonymizedText { get; set; } = string.Empty;
    public LeakCheckResult LeakCheck { get; set; } = LeakCheckResult.Ok();
    public bool EgressAllowed { get; set; }
    public string? FailureReason { get; set; }
    public bool Aborted => FailureReason is not null;
}

/// <summary>
/// Optional Presidio sidecar HTTP client. CloudEgress refuses when unavailable.
/// </summary>
public interface IPresidioAnalyzerClient
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RawEntitySpan>> AnalyzeAsync(
        string text,
        string language,
        CancellationToken cancellationToken = default);
}

/// <summary>Encrypts TokenMap plaintext at rest (AES-GCM).</summary>
public interface ITokenValueProtector
{
    byte[] Protect(string plaintext);
    string Unprotect(byte[] ciphertext);
    string ComputeHmac(string normalizedValue);
}

/// <summary>Access-log stub for TokenMap reads (metadata only; never plaintext).</summary>
public interface ITokenMapAccessLogger
{
    Task LogAccessAsync(
        string sessionId,
        string documentId,
        string token,
        string operation,
        CancellationToken cancellationToken = default);
}
