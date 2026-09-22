using VatTaxes.Domain.Entities;

namespace VatTaxes.Domain.Repositories;

/// <summary>
/// Contrato de persistência para taxas de IVA
/// </summary>
public interface IVatTaxesRepository
{
    Task<(int TotalItems, int CurrentPage, int PageSize, List<VatTax> Items)> GetPagedAsync(
        decimal? tabiva,
        decimal? taxa,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<VatTax?> GetByTabivaAsync(decimal tabiva, CancellationToken cancellationToken = default);

    Task<VatTax?> GetByTaxaAsync(decimal taxa, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTabivaAndTaxaAsync(decimal tabiva, decimal taxa, CancellationToken cancellationToken = default);

    Task<VatTax?> UpdateTaxaByTabivaAsync(decimal tabiva, decimal taxa, string? updatedBy = null, CancellationToken cancellationToken = default);
}
