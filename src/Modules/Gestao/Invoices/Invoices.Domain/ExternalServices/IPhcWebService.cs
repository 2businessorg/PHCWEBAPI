namespace Invoices.Domain.ExternalServices;

/// <summary>
/// Invoices-specific PHC Web service interface
/// Implements methods for invoice-specific operations
/// </summary>
public interface IPhcWebService
{
    /// <summary>
    /// Executes a script on PHC WEB using provided credentials
    /// </summary>
    /// <param name="url">URL do web service PHC</param>
    /// <param name="username">Utilizador para autenticação</param>
    /// <param name="password">Password para autenticação</param>
    /// <param name="code">Código do script a executar</param>
    /// <param name="parameter">Parâmetro JSON a enviar</param>
    /// <returns>Resposta JSON do PHC Web</returns>
    Task<string> ExecuteAsync(string url, string username, string password, string code, string parameter);
}
