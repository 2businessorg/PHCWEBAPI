namespace Currencies.Application.Errors;

/// <summary>
/// Exceção de domínio/aplicação do módulo Currencies com código padronizado.
/// </summary>
public sealed class CurrenciesModuleException : Exception
{
    public string Code { get; }

    public CurrenciesModuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
