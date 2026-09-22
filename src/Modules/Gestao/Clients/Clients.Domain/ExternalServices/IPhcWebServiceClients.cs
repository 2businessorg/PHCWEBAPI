namespace Clients.Domain.ExternalServices;

/// <summary>
/// Contrato para integração com PHC WEB para criação de clientes (CL).
/// </summary>
public interface IPhcWebServiceClients
{
    /// <summary>
    /// Cria um cliente no PHC WEB via script configurado em PhcWeb:Scripts:InsertCl:Code.
    /// </summary>
    Task<string> CreateClientAsync(string parameter, CancellationToken ct = default);
}
