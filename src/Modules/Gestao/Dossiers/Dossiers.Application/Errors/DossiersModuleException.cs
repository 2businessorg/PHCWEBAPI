namespace Dossiers.Application.Errors;

/// <summary>
/// Exceção de domínio/aplicação do módulo Dossiers com código padronizado.
/// </summary>
public sealed class DossiersModuleException : Exception
{
    public string Code { get; }

    public DossiersModuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
