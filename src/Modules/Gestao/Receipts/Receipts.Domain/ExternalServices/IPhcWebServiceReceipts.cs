namespace Receipts.Domain.ExternalServices;

/// <summary>
/// Contrato para integração com o PHC WEB para criação de Recibos
/// </summary>
public interface IPhcWebServiceReceipts
{
    /// <summary>
    /// Cria um recibo no PHC WEB via script configurado em PhcWeb:Scripts:InsertRe:Code.
    /// As credenciais e o nome do script são resolvidos internamente pela implementação de Infrastructure.
    /// </summary>
    Task<string> CreateReceiptAsync(
        string parameter,
        CancellationToken ct = default);
}
