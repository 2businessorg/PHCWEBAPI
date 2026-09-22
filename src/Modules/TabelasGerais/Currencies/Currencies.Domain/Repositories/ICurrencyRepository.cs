using Currencies.Domain.ValueObjects;

namespace Currencies.Domain.Repositories;

/// <summary>
/// Repositório para consulta agregada de moedas (para1 + cb).
/// </summary>
public interface ICurrencyRepository
{
    /// <summary>
    /// Obtém todas as moedas disponíveis.
    /// </summary>
    Task<IReadOnlyList<CurrencyData>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma moeda específica pelo código.
    /// </summary>
    Task<CurrencyData?> GetByCodeAsync(string moeda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se uma moeda existe pelo código.
    /// </summary>
    Task<bool> ExistsAsync(string moeda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém taxas de conversão por código de moeda.
    /// </summary>
    Task<IReadOnlyList<CurrencyExchangeRateData>> GetExchangeRatesByCodeAsync(string moeda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as taxas de conversão.
    /// </summary>
    Task<IReadOnlyList<CurrencyExchangeRateData>> GetAllExchangeRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe uma taxa de conversão com a moeda e país especificados.
    /// </summary>
    Task<bool> ExchangeRateExistsAsync(string moeda, string pais, CancellationToken cancellationToken = default);
}
