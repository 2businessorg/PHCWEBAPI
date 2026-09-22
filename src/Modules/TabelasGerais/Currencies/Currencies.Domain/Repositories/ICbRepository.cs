namespace Currencies.Domain.Repositories;

/// <summary>
/// Repositório da tabela cb.
/// </summary>
public interface ICbRepository
{
    /// <summary>
    /// Obtém lista distinta de moedas na tabela cb.
    /// </summary>
    Task<IReadOnlyList<string>> GetDistinctMoedasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma nova moeda na tabela cb.
    /// </summary>
    Task CreateAsync(string pais, string moeda, string? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma moeda existente na tabela cb.
    /// </summary>
    Task<bool> UpdateAsync(string moeda, string pais, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta uma moeda da tabela cb.
    /// </summary>
    Task<bool> DeleteAsync(string moeda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma taxa de conversão na tabela cb.
    /// </summary>
    Task CreateExchangeRateAsync(
        string pais,
        string moeda,
        decimal buyRate,
        decimal sellRate,
        DateTime data,
        string? createdBy = null,
        CancellationToken cancellationToken = default);
}
