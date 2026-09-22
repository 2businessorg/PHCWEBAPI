using MediatR;
using Treasury.Application.DTOs;
using Treasury.Application.Errors;
using Treasury.Application.Mappings;
using Treasury.Domain.Repositories;

namespace Treasury.Application.Features.GetTreasuryAccount;

/// <summary>
/// Loads one treasury account by name.
/// </summary>
public class GetTreasuryAccountQueryHandler
    : IRequestHandler<GetTreasuryAccountQuery, TreasuryAccountOutputDTO>
{
    private readonly ITreasuryAccountRepository _accountRepository;

    public GetTreasuryAccountQueryHandler(ITreasuryAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<TreasuryAccountOutputDTO> Handle(
        GetTreasuryAccountQuery request,
        CancellationToken cancellationToken)
    {
        var accountName = request.AccountName.Trim();
        var account = await _accountRepository.GetByNameAsync(accountName, cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException(
                string.Format(TreasuryErrorCatalog.AccountNotFound.Description, accountName));
        }

        return account.ToDto();
    }
}
