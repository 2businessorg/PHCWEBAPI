using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.AnalyzeVacancy;
using Recruitment.Application.Features.CompareCandidates;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Application.Features.GetRctRanking;
using Recruitment.Application.Features.GetVacancyRanking;
using Recruitment.Application.Features.GetVacancyStatus;
using Recruitment.Application.Features.ReprocessAnalysis;
using Recruitment.Application.Features.ReprocessVacancyCandidate;
using Recruitment.Domain.Constants;
using Shared.Kernel.Authorization;
using Shared.Kernel.DTOs;

namespace Recruitment.Presentation.REST.Controllers;

/// <summary>
/// HITL surface for Recrutamento x IA. Score is input; human decides in PHC (AH-04 / BR-09).
/// Product path is /api/recruitment/{idrct}/... Analysis runs only from these explicit endpoints.
/// No auto-advance / auto-reject endpoints (AH-04 / BR-03 / BR-09).
/// </summary>
[ApiController]
[Route("api/recruitment")]
[Produces("application/json")]
public sealed class RecruitmentIaController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecruitmentIaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Batch analyze every candidature of the vacancy that has a CV.
    /// Resolves idrct to rctstamp, then one Hangfire Recruitment.AnalyzeCandidate job per SRT.
    /// BR-01 (crt and RCTCLB) is applied inside TryEnqueueAsync. Does not write selection/condp.
    /// </summary>
    [HttpPost("{idrct}/analyze")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<AnalyzeVacancyResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<AnalyzeVacancyResultDto>>> Analyze(
        string idrct,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] AnalyzeVacancyRequest? body,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AnalyzeVacancyCommand(idrct, body?.RequestedBy, body?.LabGoRef),
            ct);
        return Ok(new SingleItemResponseDTO<AnalyzeVacancyResultDto> { Item = result });
    }

    /// <summary>Counts of cve.u_estadoia (pendente/ok/erro) for SRT rows of the vacancy.</summary>
    [HttpGet("{idrct}/status")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<VacancyIaStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<VacancyIaStatusDto>>> Status(
        string idrct,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVacancyIaStatusQuery(idrct), ct);
        return Ok(new SingleItemResponseDTO<VacancyIaStatusDto> { Item = result });
    }

    /// <summary>
    /// HITL ranking ordered by IA score. Pre-seleccao humana; not an automatic decision (AH-04 / BR-09).
    /// </summary>
    [HttpGet("{idrct}/ranking")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<RctRankingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<RctRankingDto>>> RankingById(
        string idrct,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVacancyRankingQuery(idrct), ct);
        return Ok(new SingleItemResponseDTO<RctRankingDto> { Item = result });
    }

    /// <summary>Reprocess one candidature of the vacancy. Body: srtStamp + requestedBy.</summary>
    [HttpPost("{idrct}/reprocess")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<EnqueueAnalysisResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SingleItemResponseDTO<EnqueueAnalysisResultDto>>> ReprocessById(
        string idrct,
        [FromBody] ReprocessVacancyRequest body,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ReprocessVacancyCandidateCommand(idrct, body.SrtStamp, body.RequestedBy, body.LabGoRef),
            ct);
        return Ok(new SingleItemResponseDTO<EnqueueAnalysisResultDto> { Item = result });
    }

    /// <summary>
    /// Legacy flat enqueue (three stamps). Kept for compatibility; product path is POST /{idrct}/analyze.
    /// </summary>
    [Obsolete("Use POST /api/recruitment/{idrct}/analyze.")]
    [HttpPost("enqueue")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<EnqueueAnalysisResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SingleItemResponseDTO<EnqueueAnalysisResultDto>>> Enqueue(
        [FromBody] EnqueueRequest body,
        CancellationToken ct)
    {
        MarkFlatRouteDeprecated("/api/recruitment/{idrct}/analyze");
        var result = await _mediator.Send(
            new EnqueueAnalysisCommand(body.CveStamp, body.RctStamp, body.SrtStamp, body.LabGoRef),
            ct);

        return Ok(new SingleItemResponseDTO<EnqueueAnalysisResultDto> { Item = result });
    }

    /// <summary>Ranked SRT list with per-crt quotes and gaps (HITL).</summary>
    [HttpGet("rct/{rctStamp}/ranking")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<RctRankingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SingleItemResponseDTO<RctRankingDto>>> GetRanking(
        string rctStamp,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRctRankingQuery(rctStamp), ct);
        return Ok(new SingleItemResponseDTO<RctRankingDto> { Item = result });
    }

    /// <summary>Why #1 vs #k using top 2–3 crt diffs with quotes.</summary>
    [HttpGet("rct/{rctStamp}/compare")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<CandidateComparisonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SingleItemResponseDTO<CandidateComparisonDto>>> Compare(
        string rctStamp,
        [FromQuery] string firstSrtStamp,
        [FromQuery] string otherSrtStamp,
        [FromQuery] int topDiffCount = 3,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new CompareCandidatesQuery(rctStamp, firstSrtStamp, otherSrtStamp, topDiffCount),
            ct);
        return Ok(new SingleItemResponseDTO<CandidateComparisonDto> { Item = result });
    }

    /// <summary>
    /// Legacy reprocess with three stamps. Product path is POST /{idrct}/reprocess.
    /// Does not erase human PHC decisions (BR-06 / BR-09).
    /// </summary>
    [Obsolete("Use POST /api/recruitment/{idrct}/reprocess.")]
    [HttpPost("reprocess")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<EnqueueAnalysisResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SingleItemResponseDTO<EnqueueAnalysisResultDto>>> Reprocess(
        [FromBody] ReprocessRequest body,
        CancellationToken ct)
    {
        MarkFlatRouteDeprecated("/api/recruitment/{idrct}/reprocess");
        var result = await _mediator.Send(
            new ReprocessAnalysisCommand(
                body.CveStamp,
                body.RctStamp,
                body.SrtStamp,
                body.RequestedBy,
                body.LabGoRef),
            ct);
        return Ok(new SingleItemResponseDTO<EnqueueAnalysisResultDto> { Item = result });
    }

    /// <summary>HITL copy constants for UI clients.</summary>
    [HttpGet("hitl-copy")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    public ActionResult<object> HitlCopyMeta()
    {
        return Ok(new
        {
            title = HitlCopy.RankingTitle,
            footer = HitlCopy.Footer,
            semEvidencia = HitlCopy.SemEvidencia,
            note = "Proibido: seleccionado/rejeitado/avancado pela IA (AH-04)."
        });
    }

    private void MarkFlatRouteDeprecated(string successor)
    {
        Response.Headers.Append("Deprecation", "true");
        Response.Headers.Append("Link", $"<{successor}>; rel=\"successor-version\"");
    }

    public sealed class AnalyzeVacancyRequest
    {
        public string? RequestedBy { get; set; }

        public string? LabGoRef { get; set; }
    }

    public sealed class ReprocessVacancyRequest
    {
        public string SrtStamp { get; set; } = string.Empty;

        public string RequestedBy { get; set; } = string.Empty;

        public string? LabGoRef { get; set; }
    }

    public sealed class EnqueueRequest
    {
        public string CveStamp { get; set; } = string.Empty;
        public string RctStamp { get; set; } = string.Empty;
        public string SrtStamp { get; set; } = string.Empty;
        public string? LabGoRef { get; set; }
    }

    public sealed class ReprocessRequest
    {
        public string CveStamp { get; set; } = string.Empty;
        public string RctStamp { get; set; } = string.Empty;
        public string SrtStamp { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string? LabGoRef { get; set; }
    }
}
