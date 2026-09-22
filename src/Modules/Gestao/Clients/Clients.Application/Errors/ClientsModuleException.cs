namespace Clients.Application.Errors;

/// <summary>
/// Exceção de domínio/aplicação do módulo Clients com código padronizado.
/// </summary>
public sealed class ClientsModuleException : Exception
{
    public string Code { get; }

    public ClientsModuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
