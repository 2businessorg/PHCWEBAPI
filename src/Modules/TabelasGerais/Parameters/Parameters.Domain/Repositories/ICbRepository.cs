namespace Parameters.Domain.Repositories;

/// <summary>
/// Repositório para consulta de moedas disponíveis.
/// A lista resulta do union entre para1 (ge_pteoueuro) e as moedas distintas de cb.
/// </summary>
public interface ICbRepository
{
    /// <summary>
    /// Retorna todas as moedas disponíveis (para1 + cb).
    /// </summary>
    Task<IEnumerable<string>> GetMoedasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se uma moeda existe.
    /// </summary>
    Task<bool> ExistsMoedaAsync(string moeda, CancellationToken cancellationToken = default);
}
