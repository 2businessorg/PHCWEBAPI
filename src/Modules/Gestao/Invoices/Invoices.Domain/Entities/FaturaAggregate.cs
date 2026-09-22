using System.Collections.Generic;

namespace Invoices.Domain.Entities;

/// <summary>
/// Agregado de fatura com cabeçalho, dados secundários, dados adicionais e linhas
/// </summary>
public class FaturaAggregate
{
    public Ft Fatura { get; set; } = null!;
    public FT2 DadosSecundarios { get; set; } = null!;
    public FT3 DadosAdicionais { get; set; } = null!;
    public List<Fi> Linhas { get; set; } = new();
    public List<Fi2> LinhasAdicionais { get; set; } = new();
}