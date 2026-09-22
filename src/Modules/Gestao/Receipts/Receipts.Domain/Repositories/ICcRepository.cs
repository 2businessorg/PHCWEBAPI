using Receipts.Domain.Entities;

namespace Receipts.Domain.Repositories;

/// <summary>
/// Repositório de Conta Corrente de Clientes (tabela cc)
/// </summary>
public interface ICcRepository
{
    /// <summary>
    /// Obtém o registo CC de uma factura identificada por número, ano, série e cliente.
    /// O ccstamp resultante é o ftstamp da factura e é usado internamente para chamar o PHC WEB.
    /// </summary>
    Task<Cc?> GetByInvoiceAsync(
        decimal no,
        decimal nrdoc,
        decimal ano,
        string serie,
        decimal? ndoc = null,
        CancellationToken ct = default);
}
