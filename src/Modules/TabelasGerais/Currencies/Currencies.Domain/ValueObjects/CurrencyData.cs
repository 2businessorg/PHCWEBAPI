namespace Currencies.Domain.ValueObjects;

/// <summary>
/// Value Object contendo moeda e país.
/// </summary>
public sealed record CurrencyData(string Moeda, string Pais);
