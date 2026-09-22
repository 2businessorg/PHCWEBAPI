namespace Receipts.Application.Errors;

/// <summary>
/// Exceção de domínio do módulo Receipts com código e mensagem estruturados
/// </summary>
public class ReceiptsModuleException : Exception
{
    public string ErrorCode { get; }

    public ReceiptsModuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
