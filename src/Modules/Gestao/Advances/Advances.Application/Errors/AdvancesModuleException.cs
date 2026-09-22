namespace Advances.Application.Errors;

/// <summary>
/// Excepção de domínio para erros de negócio do módulo Advances
/// </summary>
public class AdvancesModuleException : Exception
{
    public string ErrorCode { get; }

    public AdvancesModuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
