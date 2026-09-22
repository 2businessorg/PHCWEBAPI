namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório para validação de moedas usadas no módulo Dossiers.
/// </summary>
public interface IMoedaRepository
{
    /// <summary>
    /// Verifica se a moeda informada existe na configuração disponível para o sistema.
    /// </summary>
    Task<bool> ExistsAsync(string moeda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a moeda padrão do sistema definida em para1 (descricao = ge_pteoueuro).
    /// </summary>
    Task<string?> GetDefaultAsync(CancellationToken cancellationToken = default);
}
