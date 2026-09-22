using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Repositório de taxas de IVA
/// </summary>
public interface ITaxasIvaRepository
{
    Task<Taxasiva?> GetByTaxRateAsync(decimal taxRate, CancellationToken cancellationToken = default);
    Task<Taxasiva?> GetByCodeAsync(int code, CancellationToken cancellationToken = default);
    Task<decimal> GetTaxRateByCodeAsync(int code, CancellationToken cancellationToken = default);
    (decimal BaseInc, decimal TaxValue) Calculate(int code, bool ivaIncluded, decimal grossAmount, decimal taxRateOverride = -1m);
}
