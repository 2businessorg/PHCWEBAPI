namespace Currencies.Domain.ValueObjects;

/// <summary>
/// Dados de taxa de conversão de uma moeda.
/// </summary>
public sealed record CurrencyExchangeRateData(
    string Moeda,
    string Pais,
    decimal? CambioCompra,
    decimal? CambioVenda,
    DateTime Data
);
