using Shared.Abstractions.Privacy.Pseudonymization;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Detection;

/// <summary>
/// Simple entity resolution: cluster by type + normalized value (Union-Find lite).
/// Never merges different entity types even when strings match.
/// </summary>
public sealed class SimpleEntityResolver : IEntityResolver
{
    public IReadOnlyList<ResolvedEntity> Resolve(IReadOnlyList<ScoredEntity> scored)
    {
        var groups = scored
            .GroupBy(s => (s.EntityType, s.NormalizedValue), EntityKeyComparer.Instance)
            .Select(g =>
            {
                var best = g.OrderByDescending(x => x.Score).ThenByDescending(x => x.Text.Length).First();
                var mentions = g
                    .Select(x => (x.Start, x.End, x.Text))
                    .OrderBy(m => m.Start)
                    .ToList();
                return new ResolvedEntity(
                    best.EntityType,
                    best.Text,
                    best.NormalizedValue,
                    best.Score,
                    best.SourceRecognizer,
                    mentions);
            })
            .ToList();

        return groups;
    }

    private sealed class EntityKeyComparer : IEqualityComparer<(string EntityType, string NormalizedValue)>
    {
        public static readonly EntityKeyComparer Instance = new();

        public bool Equals((string EntityType, string NormalizedValue) x, (string EntityType, string NormalizedValue) y) =>
            string.Equals(x.EntityType, y.EntityType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.NormalizedValue, y.NormalizedValue, StringComparison.Ordinal);

        public int GetHashCode((string EntityType, string NormalizedValue) obj) =>
            HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.EntityType),
                StringComparer.Ordinal.GetHashCode(obj.NormalizedValue));
    }
}
