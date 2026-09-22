namespace Stocks.Application.Errors;

/// <summary>
/// Exceção de domínio/aplicação do módulo Stocks com código padronizado.
/// </summary>
public sealed class StocksModuleException : Exception
{
    public string Code { get; }

    public StocksModuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
