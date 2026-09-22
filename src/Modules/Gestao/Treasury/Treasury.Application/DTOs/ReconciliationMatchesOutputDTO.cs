namespace Treasury.Application.DTOs;

/// <summary>
/// Precomputed BR/BA match proposals for an account and period.
/// </summary>
public record ReconciliationMatchesOutputDTO
{
    public TreasuryAccountOutputDTO Account { get; init; } = new();

    public ReconciliationPeriodOutputDTO Period { get; init; } = new();

    public int BankMovementCount { get; init; }

    public int TreasuryMovementCount { get; init; }

    public int MatchedCount { get; init; }

    public int PartialMatchedCount { get; init; }

    public int UnmatchedBankCount { get; init; }

    public int UnmatchedTreasuryCount { get; init; }

    public string Instruction { get; init; } =
        "Copia combinations e status. Nao pares movimentos. Nao inventes MATCHED. " +
        "MATCHED = valor igual e datas ate 7 dias. PARTIAL MATCHED = valor igual mas datas longe ou texto parecido. " +
        "UNMATCHED = sem par. Isto e uma proposta; nao grava reco.";

    public IReadOnlyList<ReconciliationMatchCombinationDTO> Combinations { get; init; } =
        Array.Empty<ReconciliationMatchCombinationDTO>();
}
