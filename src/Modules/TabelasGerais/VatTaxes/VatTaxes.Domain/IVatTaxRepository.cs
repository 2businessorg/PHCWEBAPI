namespace VatTaxes.Domain;

/// <summary>
/// Interface do Repository para TaxasIva
/// </summary>
public interface IVatTaxRepository
{
    /// <summary>
    /// Obter taxa de IVA por código
    /// </summary>
    Task<TaxasIva?> GetByCodeAsync(int code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obter taxa de IVA por stamp (identificador único)
    /// </summary>
    Task<TaxasIva?> GetByStampAsync(string stamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obter todas as taxas de IVA
    /// </summary>
    Task<List<TaxasIva>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obter taxas de IVA com paginação
    /// </summary>
    Task<List<TaxasIva>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obter total de registros
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verificar se existe taxa com o código especificado
    /// </summary>
    Task<bool> ExistsByCodeAsync(int code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adicionar nova taxa
    /// </summary>
    Task AddAsync(TaxasIva taxasIva, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualizar taxa existente
    /// </summary>
    Task UpdateAsync(TaxasIva taxasIva, CancellationToken cancellationToken = default);

    /// <summary>
    /// Eliminar taxa por código
    /// </summary>
    Task DeleteByCodeAsync(int code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obter percentagem da taxa por código
    /// </summary>
    Task<decimal> GetRateByCodeAsync(int code, CancellationToken cancellationToken = default);
}
