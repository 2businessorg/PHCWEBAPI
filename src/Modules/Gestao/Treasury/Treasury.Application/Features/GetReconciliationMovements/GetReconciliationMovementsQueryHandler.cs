using MediatR;
using Microsoft.Extensions.Logging;
using Treasury.Application.DTOs;
using Treasury.Application.Errors;
using Treasury.Application.Mappings;
using Treasury.Domain.Repositories;

namespace Treasury.Application.Features.GetReconciliationMovements;

/// <summary>
/// Loads unreconciled BR/BA movements for a treasury account and date window.
/// Read-only: does not change <c>reco</c> or persist anything.
/// </summary>
public class GetReconciliationMovementsQueryHandler
    : IRequestHandler<GetReconciliationMovementsQuery, ReconciliationMovementsOutputDTO>
{
    private readonly ITreasuryAccountRepository _accountRepository;
    private readonly IBankReconciliationRepository _reconciliationRepository;
    private readonly ILogger<GetReconciliationMovementsQueryHandler> _logger;

    public GetReconciliationMovementsQueryHandler(
        ITreasuryAccountRepository accountRepository,
        IBankReconciliationRepository reconciliationRepository,
        ILogger<GetReconciliationMovementsQueryHandler> logger)
    {
        _accountRepository = accountRepository;
        _reconciliationRepository = reconciliationRepository;
        _logger = logger;
    }

    public async Task<ReconciliationMovementsOutputDTO> Handle(
        GetReconciliationMovementsQuery request,
        CancellationToken cancellationToken)
    {
        var accountName = request.AccountName.Trim();
        var account = await _accountRepository.GetByNameAsync(accountName, cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException(
                string.Format(TreasuryErrorCatalog.AccountNotFound.Description, accountName));
        }

        var dateFrom = request.DateFrom.ToDateTime(TimeOnly.MinValue);
        var dateToExclusive = request.DateTo.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var bank = await _reconciliationRepository.GetImportedBankMovementsAsync(
            account.AccountCode,
            dateFrom,
            dateToExclusive,
            request.Page,
            request.PageSize,
            cancellationToken);

        var treasury = await _reconciliationRepository.GetTreasuryAccountMovementsAsync(
            account.AccountCode,
            dateFrom,
            dateToExclusive,
            request.Page,
            request.PageSize,
            cancellationToken);

        var bankDtos = bank.Items.Select(x => x.ToDto()).ToList();
        var treasuryDtos = treasury.Items.Select(x => x.ToDto()).ToList();
        var totals = ReconciliationTotals.From(bankDtos, treasuryDtos);

        _logger.LogInformation(
            "Reconciliation movements loaded. Account={Account} Period={From:yyyy-MM-dd}..{To:yyyy-MM-dd} BRCount={BRCount} BACount={BACount}",
            accountName,
            request.DateFrom,
            request.DateTo,
            bankDtos.Count,
            treasuryDtos.Count);

        return new ReconciliationMovementsOutputDTO
        {
            Account = account.ToDto(),
            Period = new ReconciliationPeriodOutputDTO
            {
                From = request.DateFrom,
                To = request.DateTo
            },
            BankMovements = bankDtos,
            TreasuryMovements = treasuryDtos,
            BankMovementCount = bankDtos.Count,
            TreasuryMovementCount = treasuryDtos.Count,
            BankAmountTotal = totals.BankAmountTotal,
            BankCreditTotal = totals.BankCreditTotal,
            BankDebitTotal = totals.BankDebitTotal,
            TreasuryInflowTotal = totals.TreasuryInflowTotal,
            TreasuryOutflowTotal = totals.TreasuryOutflowTotal,
            TreasuryNetTotal = totals.TreasuryNetTotal,
            BankMovementTotalCount = bank.TotalCount,
            TreasuryMovementTotalCount = treasury.TotalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
