using Shared.Abstractions.Entities;

namespace Parameters.Domain.Entities;

public class Para1 : AuditableEntity
{
    public string Para1Stamp { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Valor { get; private set; } = string.Empty;
    public string Tipo { get; private set; } = string.Empty;
    public decimal? Dec { get; private set; }
    public decimal? Tam { get; private set; }

    private Para1() { }

    public Para1(string para1Stamp, string descricao, string valor, string tipo, decimal? dec = null, decimal? tam = null, string? criadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(para1Stamp))
            throw new ArgumentException("Para1Stamp cannot be empty", nameof(para1Stamp));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descricao cannot be empty", nameof(descricao));

        Para1Stamp = para1Stamp;
        Descricao = descricao;
        Valor = valor ?? string.Empty;
        Tipo = tipo ?? string.Empty;
        Dec = dec;
        Tam = tam;

        // Auditoria de criação
        SetCreatedAudit(criadoPor);
    }

    public void Update(string descricao, string valor, string tipo, int? dec, int? tam, string? atualizadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descricao cannot be empty", nameof(descricao));

        Descricao = descricao;
        Valor = valor ?? string.Empty;
        Tipo = tipo ?? string.Empty;
        Dec = dec;
        Tam = tam;

        // Auditoria de atualização
        SetUpdatedAudit(atualizadoPor);
    }
}
