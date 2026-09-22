namespace Invoices.Domain.Entities;

/// <summary>
/// Entidade CL - Cliente (minimizada para o módulo Invoices)
/// Armazena dados mínimos do cliente necessários para validação de faturas
/// </summary>
public class Cl
{
    /// <summary>
    /// NO - Número do cliente (chave primária)
    /// Identificador único do cliente no PHC Web
    /// </summary>
    public decimal No { get; set; }

    /// <summary>
    /// ESTAB - Estabelecimento do cliente
    /// Identificador do estabelecimento (padrão: 0 para principal)
    /// Chave composta: No + Estab
    /// </summary>
    public decimal Estab { get; set; }

    /// <summary>
    /// NOME - Nome do cliente
    /// Armazenado localmente para referência
    /// </summary>
    public string? Nome { get; set; }

    /// <summary>
    /// MOEDA - Moeda padrão do cliente
    /// Código ISO (ex: MZN, USD)
    /// </summary>
    public string? Moeda { get; set; }

    /// <summary>
    /// CLSTAMP - Identificador único do registo
    /// Gerado pelo PHC Web
    /// </summary>
    public string? ClStamp { get; set; }
}
