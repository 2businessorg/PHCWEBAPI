using Shared.Abstractions.Entities;

namespace Providers.Domain.Entities;

/// <summary>
/// Entidade ProviderValue (u_providervalues) - Propriedades key-value para cada provider/operação
/// </summary>
public class ProviderValue : AuditableEntity
{
    public string UProviderValuesStamp { get; private set; } = string.Empty;
    public string UProviderStamp { get; private set; } = string.Empty;
    public string OperationCode { get; private set; } = string.Empty;
    public string Chave { get; private set; } = string.Empty;
    public string Valor { get; private set; } = string.Empty;
    public bool Encriptado { get; private set; }
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Navigation property
    public Provider? Provider { get; private set; }

    private ProviderValue() { }

    public ProviderValue(
        string uProviderValuesStamp,
        string uProviderStamp,
        string operationCode,
        string chave,
        string valor,
        bool encriptado = false,
        int ordem = 0,
        bool ativo = true,
        string? criadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(uProviderValuesStamp))
            throw new ArgumentException("UProviderValuesStamp cannot be empty", nameof(uProviderValuesStamp));

        if (string.IsNullOrWhiteSpace(uProviderStamp))
            throw new ArgumentException("UProviderStamp cannot be empty", nameof(uProviderStamp));

        if (string.IsNullOrWhiteSpace(operationCode))
            throw new ArgumentException("OperationCode cannot be empty", nameof(operationCode));

        if (string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("Chave cannot be empty", nameof(chave));

        UProviderValuesStamp = uProviderValuesStamp;
        UProviderStamp = uProviderStamp;
        OperationCode = operationCode;
        Chave = chave;
        Valor = valor ?? string.Empty;
        Encriptado = encriptado;
        Ordem = ordem;
        Ativo = ativo;

        SetCreatedAudit(criadoPor);
    }

    public void Update(
        string operationCode,
        string chave,
        string valor,
        bool encriptado,
        int ordem,
        bool ativo,
        string? atualizadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(operationCode))
            throw new ArgumentException("OperationCode cannot be empty", nameof(operationCode));

        if (string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("Chave cannot be empty", nameof(chave));

        OperationCode = operationCode;
        Chave = chave;
        Valor = valor ?? string.Empty;
        Encriptado = encriptado;
        Ordem = ordem;
        Ativo = ativo;

        SetUpdatedAudit(atualizadoPor);
    }

    public void UpdateValor(string valor, string? atualizadoPor = null)
    {
        Valor = valor ?? string.Empty;
        SetUpdatedAudit(atualizadoPor);
    }

    public void MarkAsEncrypted(string? atualizadoPor = null)
    {
        Encriptado = true;
        SetUpdatedAudit(atualizadoPor);
    }
}
