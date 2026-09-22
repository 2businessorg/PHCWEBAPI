using MediatR;
using Treasury.Application.DTOs;

namespace Treasury.Application.Features.GetTreasuryAccounts;

/// <summary>
/// Lists PHC treasury accounts (<c>bl</c>) for discovery by name.
/// </summary>
public record GetTreasuryAccountsQuery(
    string? NameContains = null,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = 50
) : IRequest<TreasuryAccountListOutputDTO>;
