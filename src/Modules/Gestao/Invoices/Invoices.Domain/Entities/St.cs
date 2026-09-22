namespace Invoices.Domain.Entities;

/// <summary>
/// Entidade ST - Artigo/Produto (minimizada para o módulo Invoices)
/// Armazena dados mínimos do artigo necessários para validação de faturas
/// </summary>
public class St
{
    /// <summary>
    /// REF - Referência do artigo (chave primária)
    /// Identificador único do artigo no PHC Web
    /// </summary>
    public string Ref { get; set; } = string.Empty;

    /// <summary>
    /// DESIGN - Descrição do artigo
    /// Nome/descrição do produto
    /// </summary>
    public string? Design { get; set; }

    /// <summary>
    /// STSTAMP - Identificador único do registo
    /// Gerado pelo PHC Web
    /// </summary>
    public string? StStamp { get; set; }
}
