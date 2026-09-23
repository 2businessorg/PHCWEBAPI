using System.Text.Json;
using MediatR;
using Recruitment.Application.DTOs;
using Recruitment.Application.Scoring;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Repositories;

namespace Recruitment.Application.Features.GetRctRanking;

public sealed record GetRctRankingQuery(string RctStamp) : IRequest<RctRankingDto>;

public sealed class GetRctRankingQueryHandler : IRequestHandler<GetRctRankingQuery, RctRankingDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ISrtScoreRepository _srt;
    private readonly ICvAnexoRepository _anexos;

    public GetRctRankingQueryHandler(ISrtScoreRepository srt, ICvAnexoRepository anexos)
    {
        _srt = srt;
        _anexos = anexos;
    }

    public async Task<RctRankingDto> Handle(GetRctRankingQuery request, CancellationToken cancellationToken)
    {
        var rows = await _srt.GetByRctAsync(request.RctStamp, cancellationToken);
        var ordered = rows
            .OrderByDescending(r => r.ScoreIa ?? -1m)
            .ThenBy(r => r.CandidateName)
            .ToList();

        var candidates = new List<RankedCandidateDto>();
        var position = 1;
        foreach (var row in ordered)
        {
            var breakdown = ParseBreakdown(row.JustificationJson);
            // Rubric quotes are re-checked against anexos.u_texto.
            // Qwen quotes were checked against the pseudonymized text at score time
            // and are not expected to appear in the raw OCR body.
            if (!JustificationUsesLlm(row.JustificationJson))
                await AssertQuotesAgainstCurrentUTextoAsync(row.CveStamp, breakdown, cancellationToken);
            candidates.Add(new RankedCandidateDto
            {
                Position = position++,
                SrtStamp = row.SrtStamp,
                CveStamp = row.CveStamp,
                CandidateName = row.CandidateName,
                ScoreTotal = row.ScoreIa,
                EstadoIa = row.EstadoIa,
                Condp = row.Condp,
                Modelo = row.ModeloIa,
                PromptVer = row.PromptVerIa,
                StampIa = row.StampIa,
                Breakdown = breakdown
            });
        }

        var dto = new RctRankingDto
        {
            Title = HitlCopy.RankingTitle,
            Footer = HitlCopy.Footer,
            RctStamp = request.RctStamp,
            Candidates = candidates
        };

        ForbiddenCopyGuard.ThrowIfForbidden(dto.Title, "title");
        ForbiddenCopyGuard.ThrowIfForbidden(dto.Footer, "footer");
        ForbiddenCopyGuard.ThrowIfForbidden(dto.PreSelectionNote, "preSelectionNote");
        return dto;
    }

    private async Task AssertQuotesAgainstCurrentUTextoAsync(
        string cveStamp,
        IReadOnlyList<CriterionBreakdownDto> breakdown,
        CancellationToken ct)
    {
        var quotes = breakdown.Where(b => !string.IsNullOrEmpty(b.Quote)).ToList();
        if (quotes.Count == 0)
            return;

        var anexo = await _anexos.GetCvAnexoAsync(cveStamp, ct);
        var uTexto = anexo?.Texto ?? string.Empty;
        foreach (var row in quotes)
            RubricEvidenceScorer.AssertQuoteIsSubset(uTexto, row.Quote);
    }

    internal static bool JustificationUsesLlm(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            var payload = JsonSerializer.Deserialize<JustificationPayloadDto>(json, JsonOptions);
            return payload?.UsedLlm == true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    internal static IReadOnlyList<CriterionBreakdownDto> ParseBreakdown(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<CriterionBreakdownDto>();

        try
        {
            var payload = JsonSerializer.Deserialize<JustificationPayloadDto>(json, JsonOptions);
            return payload?.Breakdown ?? Array.Empty<CriterionBreakdownDto>();
        }
        catch (JsonException)
        {
            return Array.Empty<CriterionBreakdownDto>();
        }
    }
}