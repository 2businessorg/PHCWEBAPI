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

    public GetRctRankingQueryHandler(ISrtScoreRepository srt)
    {
        _srt = srt;
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
        return dto;
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
