namespace Dossiers.Domain.Entities;

/// <summary>
/// Tabela genérica para dados de dossier a partir de qualquer tabela (CL, FL, AG, EM)
/// Normaliza os campos para uma estrutura comum
/// </summary>
public class DossierEntity
{
    /// <summary>Número da Tabela a ser utilizada</summary>
    public decimal No { get; set; }

    /// <summary>Estabelecimento (0 para AG e EM)</summary>
    public decimal Estab { get; set; }

    /// <summary>Nome da Tabela a ser utilizada</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>NUIT/Contribuinte</summary>
    public string Ncont { get; set; } = string.Empty;

    /// <summary>Morada</summary>
    public string Morada { get; set; } = string.Empty;

    /// <summary>Localidade</summary>
    public string Local { get; set; } = string.Empty;

    /// <summary>Código postal</summary>
    public string Codpost { get; set; } = string.Empty;

    /// <summary>Tabela de preço (1-5, ou 0 se não aplicável)</summary>
    public decimal Preco { get; set; }

    /// <summary>Segmento (vazio se não aplicável)</summary>
    public string Segmento { get; set; } = string.Empty;

    /// <summary>Telefone</summary>
    public string Telefone { get; set; } = string.Empty;

    /// <summary>Contacto</summary>
    public string Contacto { get; set; } = string.Empty;

    /// <summary>Email</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Stamp da Tabela a ser utilizada (para consultar dados adicionais)</summary>
    public string Stamp { get; set; } = string.Empty;
}
