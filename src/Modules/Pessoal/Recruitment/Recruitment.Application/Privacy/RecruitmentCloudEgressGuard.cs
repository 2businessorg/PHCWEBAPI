using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Privacy.Pseudonymization;
using Recruitment.Application.Options;

namespace Recruitment.Application.Privacy;

/// <summary>
/// Thin Recruitment adapter for cloud egress preparation (BR-08).
/// Does not invent scoring/business rules — only gates pseudonymization before any cloud path.
/// When EnableCloudLlm=false, no sidecar is required.
/// </summary>
public interface IRecruitmentCloudEgressGuard
{
    /// <summary>
    /// Prepares pseudonymized text for a future cloud LLM call.
    /// Returns EgressAllowed=false when flag is off, sidecar down, or leak check fails.
    /// The analyze job calls Qwen only when EgressAllowed is true.
    /// </summary>
    Task<CloudEgressPreparation> PrepareAsync(
        string sessionId,
        string documentId,
        string plainText,
        CancellationToken cancellationToken = default);
}

public sealed record CloudEgressPreparation(
    bool EnableCloudLlm,
    bool EgressAllowed,
    string? PseudonymizedText,
    string? FailureReason,
    PseudonymizationResult? Result);

public sealed class RecruitmentCloudEgressGuard : IRecruitmentCloudEgressGuard
{
    private readonly IDocumentPseudonymizer _pseudonymizer;
    private readonly IOptions<RecruitmentIaOptions> _options;
    private readonly ILogger<RecruitmentCloudEgressGuard> _logger;

    public RecruitmentCloudEgressGuard(
        IDocumentPseudonymizer pseudonymizer,
        IOptions<RecruitmentIaOptions> options,
        ILogger<RecruitmentCloudEgressGuard> logger)
    {
        _pseudonymizer = pseudonymizer;
        _options = options;
        _logger = logger;
    }

    public async Task<CloudEgressPreparation> PrepareAsync(
        string sessionId,
        string documentId,
        string plainText,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Value.EnableCloudLlm)
        {
            return new CloudEgressPreparation(
                EnableCloudLlm: false,
                EgressAllowed: false,
                PseudonymizedText: null,
                FailureReason: "EnableCloudLlm=false (default). On-prem path; sidecar not required.",
                Result: null);
        }

        // GO Denilson is an operational gate (BR-08); code still refuses without Presidio Pass.
        _logger.LogWarning(
            "EnableCloudLlm=true: running Presidio pseudonymization before any cloud call (GO Denilson required). session={SessionId}",
            sessionId);

        var result = await _pseudonymizer.PseudonymizeAsync(
            new PseudonymizationRequest(
                sessionId,
                documentId,
                plainText,
                PseudonymizationMode.CloudEgress,
                new PseudonymizationPolicy()),
            cancellationToken);

        if (!result.EgressAllowed)
        {
            _logger.LogWarning(
                "Cloud egress refused after pseudonymization. session={SessionId} reason={Reason}",
                sessionId,
                result.FailureReason);
        }

        return new CloudEgressPreparation(
            EnableCloudLlm: true,
            EgressAllowed: result.EgressAllowed,
            PseudonymizedText: result.EgressAllowed ? result.PseudonymizedText : null,
            FailureReason: result.FailureReason,
            Result: result);
    }
}
