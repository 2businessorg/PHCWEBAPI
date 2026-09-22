namespace Advances.Domain.ExternalServices;

/// <summary>
/// Contrato para integração com PHC WEB no módulo Advances.
/// Executa o script insertRdAPI via SOAP para criação de adiantamentos.
/// </summary>
public interface IPhcWebServiceAdvances
{
    /// <summary>
    /// Cria um adiantamento (RD) via script PHC WEB.
    /// </summary>
    /// <param name="parameter">JSON com os dados do adiantamento</param>
    /// <param name="ct">Token de cancelamento</param>
    /// <returns>Resposta JSON do script PHC WEB</returns>
    Task<string> CreateAdvanceAsync(string parameter, CancellationToken ct = default);
}
