using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization.Options;

namespace Shared.Infrastructure.Privacy.Pseudonymization;

/// <summary>
/// Shared orchestrator: pipeline strategies + safe detoken.
/// CloudEgress refuses when sidecar is down (if required) or leak check fails.
/// </summary>
public sealed class DocumentPseudonymizer : IDocumentPseudonymizer
{
    private readonly IReadOnlyList<IPseudonymizationStep> _steps;
    private readonly IDetokenizer _detokenizer;
    private readonly ITokenMapStore _mapStore;
    private readonly ITokenValueProtector _protector;
    private readonly IPresidioAnalyzerClient _presidio;
    private readonly PseudonymizationOptions _options;
    private readonly ILogger<DocumentPseudonymizer> _logger;

    public DocumentPseudonymizer(
        IEnumerable<IPseudonymizationStep> steps,
        IDetokenizer detokenizer,
        ITokenMapStore mapStore,
        ITokenValueProtector protector,
        IPresidioAnalyzerClient presidio,
        IOptions<PseudonymizationOptions> options,
        ILogger<DocumentPseudonymizer> logger)
    {
        _steps = steps.ToList();
        _detokenizer = detokenizer;
        _mapStore = mapStore;
        _protector = protector;
        _presidio = presidio;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PseudonymizationResult> PseudonymizeAsync(
        PseudonymizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Mode == PseudonymizationMode.Off)
        {
            return new PseudonymizationResult(
                request.PlainText,
                request.SessionId,
                request.DocumentId,
                Array.Empty<DetectedEntitySummary>(),
                LeakCheckResult.Ok(),
                EgressAllowed: false,
                FailureReason: "Mode=Off");
        }

        // Product gate (e.g. RecruitmentIa.EnableCloudLlm + GO) is the caller's responsibility.
        // Shared enforces sidecar health + leak fail-closed for CloudEgress.
        if (request.Mode == PseudonymizationMode.CloudEgress
            && _options.RequireSidecarForCloudEgress)
        {
            var healthy = await _presidio.IsHealthyAsync(cancellationToken);
            if (!healthy)
            {
                _logger.LogWarning(
                    "Presidio sidecar down; CloudEgress refused (fail-closed). session={SessionId}",
                    request.SessionId);
                return PseudonymizationResult.Failed(
                    request.SessionId,
                    request.DocumentId,
                    "Presidio sidecar unavailable; CloudEgress refused.");
            }
        }

        var policy = ApplyOptionDefaults(request.Policy);
        var ctx = new PseudonymizationPipelineContext
        {
            Request = request with { Policy = policy }
        };

        foreach (var step in _steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await step.ExecuteAsync(ctx, cancellationToken);
            if (ctx.Aborted && step.Name == "LeakCheck")
                break;
        }

        if (ctx.MapEntries.Count > 0 && string.IsNullOrEmpty(ctx.PseudonymizedText) && ctx.FailureReason is null)
        {
            return PseudonymizationResult.Failed(
                request.SessionId,
                request.DocumentId,
                "Map persist/tokenize produced empty text");
        }

        var egressAllowed = request.Mode == PseudonymizationMode.CloudEgress
            && ctx.LeakCheck.Passed
            && ctx.FailureReason is null;

        if (request.Mode == PseudonymizationMode.CloudEgress && !egressAllowed)
        {
            _logger.LogWarning(
                "CloudEgress blocked. session={SessionId} reason={Reason} findings={Findings}",
                request.SessionId,
                ctx.FailureReason,
                ctx.LeakCheck.Findings.Count);
        }

        return new PseudonymizationResult(
            ctx.PseudonymizedText,
            request.SessionId,
            request.DocumentId,
            ctx.Summaries,
            ctx.LeakCheck,
            egressAllowed,
            ctx.FailureReason);
    }

    public Task<DetokenizationResult> DetokenizeAsync(
        DetokenizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var policy = new PseudonymizationPolicy(StrictDetoken: _options.StrictDetoken);
        return _detokenizer.DetokenizeAsync(
            request,
            _mapStore,
            e => _protector.Unprotect(e.OriginalValueCipher),
            policy,
            cancellationToken);
    }

    private PseudonymizationPolicy ApplyOptionDefaults(PseudonymizationPolicy policy)
    {
        var ttl = policy.MapTtl <= TimeSpan.Zero
            ? TimeSpan.FromHours(Math.Max(1, _options.DefaultMapTtlHours))
            : policy.MapTtl;

        return policy with
        {
            MapTtl = ttl,
            ConservativeOnLowConfidence = policy.ConservativeOnLowConfidence || _options.ConservativeOnLowConfidence,
            PseudonymizeEducation = policy.PseudonymizeEducation || _options.PseudonymizeEducation,
            PreserveProfessionalSkills = policy.PreserveProfessionalSkills && _options.PreserveProfessionalSkills,
            StrictDetoken = policy.StrictDetoken && _options.StrictDetoken
        };
    }
}
