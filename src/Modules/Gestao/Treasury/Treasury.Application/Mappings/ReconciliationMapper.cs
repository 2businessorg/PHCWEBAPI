using Treasury.Application.DTOs;
using Treasury.Domain.Entities;

namespace Treasury.Application.Mappings;

/// <summary>
/// Maps Treasury domain entities to simplified output DTOs.
/// </summary>
public static class ReconciliationMapper
{
    public static TreasuryAccountOutputDTO ToDto(this TreasuryAccount account)
        => new()
        {
            Name = (account.Name ?? string.Empty).Trim(),
            Code = account.AccountCode,
            AccountNumber = (account.AccountNumber ?? string.Empty).Trim(),
            Inactive = account.Inactive,
            Currency = (account.Currency ?? string.Empty).Trim(),
            Balance = account.Balance
        };

    public static ImportedBankMovementOutputDTO ToDto(this ImportedBankMovement movement)
        => new()
        {
            Id = movement.Id.Trim(),
            Date = DateOnly.FromDateTime(movement.Date),
            ValueDate = DateOnly.FromDateTime(movement.ValueDate),
            Description = movement.Description.Trim(),
            Amount = movement.Amount,
            Document = movement.Document.Trim(),
            Cheque = movement.Cheque.Trim()
        };

    public static TreasuryAccountMovementOutputDTO ToDto(this TreasuryAccountMovement movement)
        => new()
        {
            Id = movement.Id.Trim(),
            Date = DateOnly.FromDateTime(movement.Date),
            ValueDate = DateOnly.FromDateTime(movement.ValueDate),
            Document = movement.Document.Trim(),
            Description = movement.Description.Trim(),
            Inflow = movement.Inflow,
            Outflow = movement.Outflow,
            Cheque = movement.Cheque.Trim()
        };
}
