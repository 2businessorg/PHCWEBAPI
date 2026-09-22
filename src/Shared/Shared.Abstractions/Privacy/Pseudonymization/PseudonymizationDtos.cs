namespace Shared.Abstractions.Privacy.Pseudonymization;

public sealed record PseudonymizationPolicy(
    bool ConservativeOnLowConfidence = true,
    bool PseudonymizeEducation = false,
    bool PreserveProfessionalSkills = true,
    bool StrictDetoken = true,
    TimeSpan MapTtl = default,
    IReadOnlySet<string>? ExtraAllowlist = null,
    IReadOnlySet<string>? ExtraDenyDict = null)
{
    public TimeSpan EffectiveMapTtl => MapTtl <= TimeSpan.Zero ? TimeSpan.FromHours(24) : MapTtl;
}

public sealed record PseudonymizationRequest(
    string SessionId,
    string DocumentId,
    string PlainText,
    PseudonymizationMode Mode,
    PseudonymizationPolicy Policy);

public sealed record DetectedEntitySummary(
    string EntityType,
    string Token,
    float Confidence,
    string? SourceRecognizer,
    int Start,
    int End);

public sealed record LeakFinding(
    string Kind,
    string Detail,
    int? Start = null,
    int? End = null);

public sealed record LeakCheckResult(
    bool Passed,
    IReadOnlyList<LeakFinding> Findings)
{
    public static LeakCheckResult Ok() => new(true, Array.Empty<LeakFinding>());

    public static LeakCheckResult Fail(params LeakFinding[] findings) =>
        new(false, findings);
}

public sealed record PseudonymizationResult(
    string PseudonymizedText,
    string SessionId,
    string DocumentId,
    IReadOnlyList<DetectedEntitySummary> Entities,
    LeakCheckResult LeakCheck,
    bool EgressAllowed,
    string? FailureReason = null)
{
    public static PseudonymizationResult Failed(
        string sessionId,
        string documentId,
        string reason,
        LeakCheckResult? leak = null) =>
        new(
            PseudonymizedText: string.Empty,
            SessionId: sessionId,
            DocumentId: documentId,
            Entities: Array.Empty<DetectedEntitySummary>(),
            LeakCheck: leak ?? LeakCheckResult.Fail(new LeakFinding("pipeline", reason)),
            EgressAllowed: false,
            FailureReason: reason);
}

public sealed record DetokenizationRequest(
    string SessionId,
    string DocumentId,
    string ModelResponseText);

public sealed record DetokenizationResult(
    string FinalText,
    IReadOnlyList<string> RejectedTokens,
    bool Success,
    string? FailureReason = null);

public sealed record RawEntitySpan(
    string EntityType,
    string Text,
    int Start,
    int End,
    float Score,
    string SourceRecognizer);

public sealed record ScoredEntity(
    string EntityType,
    string Text,
    string NormalizedValue,
    int Start,
    int End,
    float Score,
    string SourceRecognizer);

public sealed record ResolvedEntity(
    string EntityType,
    string CanonicalValue,
    string NormalizedValue,
    float Confidence,
    string SourceRecognizer,
    IReadOnlyList<(int Start, int End, string Text)> Mentions);

public sealed record TokenMapEntry(
    string SessionId,
    string DocumentId,
    string Token,
    string EntityType,
    byte[] OriginalValueCipher,
    byte[]? NormalizedValueCipher,
    string ValueHmac,
    float Confidence,
    string? SourceRecognizer,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
