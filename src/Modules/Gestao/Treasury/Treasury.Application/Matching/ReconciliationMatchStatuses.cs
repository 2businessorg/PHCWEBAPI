namespace Treasury.Application.Matching;

/// <summary>
/// Status values returned on each suggested combination. Read-only proposal — does not write <c>reco</c>.
/// </summary>
public static class ReconciliationMatchStatuses
{
    public const string Matched = "MATCHED";

    public const string PartialMatched = "PARTIAL MATCHED";

    public const string Unmatched = "UNMATCHED";
}
