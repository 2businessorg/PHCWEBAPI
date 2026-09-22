namespace Treasury.Application.DTOs;

/// <summary>
/// Paged treasury account list.
/// </summary>
public record TreasuryAccountListOutputDTO
{
    public IReadOnlyList<TreasuryAccountOutputDTO> Items { get; init; } =
        Array.Empty<TreasuryAccountOutputDTO>();

    public int TotalCount { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }
}
