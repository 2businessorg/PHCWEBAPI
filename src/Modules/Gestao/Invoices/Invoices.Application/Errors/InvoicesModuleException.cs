namespace Invoices.Application.Errors;

/// <summary>
/// Exceção de domínio/aplicação do módulo Invoices com código padronizado
/// </summary>
public sealed class InvoicesModuleException : Exception
{
    public string Code { get; }

    public InvoicesModuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
