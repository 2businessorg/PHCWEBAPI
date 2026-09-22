using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.CompareCandidates;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Application.Features.GetRctRanking;
using Recruitment.Application.Features.ReprocessAnalysis;
using Recruitment.Domain.Constants;
using Shared.Kernel.Authorization;
using Shared.Kernel.DTOs;

namespace Recruitment.Presentation.REST.Controllers;

/// <summary>
/// HITL surface for Recrutamento x IA. Score is input; human decides in PHC (BR-09).
/// No auto-advance / auto-reject endpoints (AH-04 / BR-03).
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

    /// <summary>BR-01 enqueue when crt≥1 and RCTCLB≥1.</summary>
    [HttpPost("enqueue")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<EnqueueAnalysisResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SingleItemResponseDTO<EnqueueAnalysisResultDto>>> Enqueue(
        [FromBody] EnqueueRequest body,
        CancellationToken ct)
    {
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

    /// <summary>BR-06 audited reprocess. Does not erase human PHC decisions.</summary>
    [HttpPost("reprocess")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [ProducesResponseType(typeof(SingleItemResponseDTO<EnqueueAnalysisResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SingleItemResponseDTO<EnqueueAnalysisResultDto>>> Reprocess(
        [FromBody] ReprocessRequest body,
        CancellationToken ct)
    {
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
