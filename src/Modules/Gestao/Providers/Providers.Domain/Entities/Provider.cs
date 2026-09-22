using Shared.Abstractions.Entities;

namespace Providers.Domain.Entities;

/// <summary>
/// Entidade Provider (u_provider) - Cabeçalho de configurações de provedores externos
/// </summary>
public class Provider : AuditableEntity
{
    public string UProviderStamp { get; private set; } = string.Empty;
    public int Codigo { get; private set; }
    public string Provedor { get; private set; } = string.Empty;
    public string Environment { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    // Navigation property
    private readonly List<ProviderValue> _values = new();
    public IReadOnlyCollection<ProviderValue> Values => _values.AsReadOnly();

    private Provider() { }

    /// <summary>
    /// Adiciona um valor ao Provider
    /// </summary>
    public void AddValue(ProviderValue value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        _values.Add(value);
    }

    /// <summary>
    /// Remove todos os valores do Provider (usado no update)
    /// </summary>
    public void ClearValues()
    {
        _values.Clear();
    }

    public Provider(
        string uProviderStamp,
        int codigo,
        string provedor,
        string environment,
        string descricao,
        bool ativo = true,
        string? criadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(uProviderStamp))
            throw new ArgumentException("UProviderStamp cannot be empty", nameof(uProviderStamp));

        if (string.IsNullOrWhiteSpace(provedor))
            throw new ArgumentException("Provedor cannot be empty", nameof(provedor));

        if (string.IsNullOrWhiteSpace(environment))
            throw new ArgumentException("Environment cannot be empty", nameof(environment));

        // Validar environment
        if (environment != "Development" && environment != "Staging" && environment != "Production")
            throw new ArgumentException("Environment must be Development, Staging, or Production", nameof(environment));

        UProviderStamp = uProviderStamp;
        Codigo = codigo;
        Provedor = provedor;
        Environment = environment;
        Descricao = descricao ?? string.Empty;
        Ativo = ativo;

        SetCreatedAudit(criadoPor);
    }

    public void Update(
        int codigo,
        string provedor,
        string environment,
        string descricao,
        bool ativo,
        string? atualizadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(provedor))
            throw new ArgumentException("Provedor cannot be empty", nameof(provedor));

        if (string.IsNullOrWhiteSpace(environment))
            throw new ArgumentException("Environment cannot be empty", nameof(environment));

        // Validar environment
        if (environment != "Development" && environment != "Staging" && environment != "Production")
            throw new ArgumentException("Environment must be Development, Staging, or Production", nameof(environment));

        Codigo = codigo;
        Provedor = provedor;
        Environment = environment;
        Descricao = descricao ?? string.Empty;
        Ativo = ativo;

        SetUpdatedAudit(atualizadoPor);
    }

    public void Activate(string? atualizadoPor = null)
    {
        Ativo = true;
        SetUpdatedAudit(atualizadoPor);
    }

    public void Deactivate(string? atualizadoPor = null)
    {
        Ativo = false;
        SetUpdatedAudit(atualizadoPor);
    }
}
