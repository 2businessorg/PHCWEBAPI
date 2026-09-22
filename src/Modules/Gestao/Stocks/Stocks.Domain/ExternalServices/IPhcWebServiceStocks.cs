namespace Stocks.Domain.ExternalServices;

/// <summary>
/// Contrato para integração com PHC WEB para criação de artigos de stock.
/// </summary>
public interface IPhcWebServiceStocks
{
    /// <summary>
    /// Cria um artigo de stock no PHC WEB via script configurado em PhcWeb:Scripts:InsertSt:Code.
    /// </summary>
    Task<string> CreateStockAsync(string parameter, CancellationToken ct = default);
}
