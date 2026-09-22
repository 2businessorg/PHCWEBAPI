using MediatR;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.GetRctRanking;
using Recruitment.Application.Scoring;
using Recruitment.Domain.Constants;

namespace Recruitment.Application.Features.CompareCandidates;

public sealed record CompareCandidatesQuery(
    string RctStamp,
    string FirstSrtStamp,
    string OtherSrtStamp,
    int TopDiffCount = 3) : IRequest<CandidateComparisonDto>;

public sealed class CompareCandidatesQueryHandler
    : IRequestHandler<CompareCandidatesQuery, CandidateComparisonDto>
{
    private readonly IMediator _mediator;

    public CompareCandidatesQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<CandidateComparisonDto> Handle(
        CompareCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var ranking = await _mediator.Send(new GetRctRankingQuery(request.RctStamp), cancellationToken);
        var first = ranking.Candidates.FirstOrDefault(c => c.SrtStamp == request.FirstSrtStamp)
            ?? throw new KeyNotFoundException($"SRT {request.FirstSrtStamp} nao encontrada no ranking.");
        var other = ranking.Candidates.FirstOrDefault(c => c.SrtStamp == request.OtherSrtStamp)
            ?? throw new KeyNotFoundException($"SRT {request.OtherSrtStamp} nao encontrada no ranking.");

        var top = request.TopDiffCount <= 0 ? 3 : Math.Min(request.TopDiffCount, 3);
        var diffs = BuildTopDiffs(first, other, top);

        var dto = new CandidateComparisonDto
        {
            Title = HitlCopy.RankingTitle,
            Footer = HitlCopy.Footer,
            First = first,
            Other = other,
            TopDiffs = diffs
        };

        ForbiddenCopyGuard.ThrowIfForbidden(dto.Title, "title");
        return dto;
    }

    public static IReadOnlyList<CriterionDiffDto> BuildTopDiffs(
        RankedCandidateDto first,
        RankedCandidateDto other,
        int top)
    {
        var otherByCode = other.Breakdown.ToDictionary(b => b.Code, StringComparer.OrdinalIgnoreCase);
        var diffs = new List<CriterionDiffDto>();

        foreach (var fb in first.Breakdown)
        {
            otherByCode.TryGetValue(fb.Code, out var ob);
            var otherNote = ob?.Note ?? 0m;
            var delta = fb.Note - otherNote;
            if (delta <= 0)
                continue;

            diffs.Add(new CriterionDiffDto
            {
                Code = fb.Code,
                Label = fb.Label,
                FirstNote = fb.Note,
                OtherNote = otherNote,
                FirstQuote = fb.Quote ?? (fb.SemEvidencia ? HitlCopy.SemEvidencia : null),
                OtherQuote = ob?.Quote ?? HitlCopy.SemEvidencia
            });
        }

        return diffs
            .OrderByDescending(d => d.FirstNote - d.OtherNote)
            .Take(top)
            .ToArray();
    }
}
