using MediatR;
using Treasury.Application.DTOs;

namespace Treasury.Application.Features.GetTreasuryAccount;

/// <summary>
/// Loads one treasury account by display name (<c>bl.banco</c>).
/// </summary>
public record GetTreasuryAccountQuery(string AccountName) : IRequest<TreasuryAccountOutputDTO>;
