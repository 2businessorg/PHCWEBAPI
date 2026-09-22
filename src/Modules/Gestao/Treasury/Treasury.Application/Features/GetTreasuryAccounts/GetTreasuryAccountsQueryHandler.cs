using MediatR;
using Treasury.Application.DTOs;
using Treasury.Application.Mappings;
using Treasury.Domain.Repositories;

namespace Treasury.Application.Features.GetTreasuryAccounts;

/// <summary>
/// Loads a paged list of treasury accounts.
/// </summary>
public class GetTreasuryAccountsQueryHandler
    : IRequestHandler<GetTreasuryAccountsQuery, TreasuryAccountListOutputDTO>
{
    private readonly ITreasuryAccountRepository _accountRepository;

    public GetTreasuryAccountsQueryHandler(ITreasuryAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<TreasuryAccountListOutputDTO> Handle(
        GetTreasuryAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _accountRepository.ListAsync(
            request.NameContains,
            request.IncludeInactive,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new TreasuryAccountListOutputDTO
        {
            Items = result.Items.Select(a => a.ToDto()).ToList(),
            TotalCount = result.TotalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
