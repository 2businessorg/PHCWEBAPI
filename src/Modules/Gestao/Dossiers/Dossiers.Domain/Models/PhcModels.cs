namespace Dossiers.Domain.Models;

/// <summary>
/// Modelo de linha para o script PHC (sem atributos JSON no Domain)
/// Inclui campos opcionais: Descricao, PrecoUnitario, TabIva, IvaIncl
/// </summary>
public class PhcLinhaModel
{
    public string Ref { get; set; } = string.Empty;
    public decimal Qtt { get; set; }
    public string? Design { get; set; }
    public decimal? PrecoUnitario { get; set; }
    public int? TabIva { get; set; }
    public bool? IvaIncl { get; set; }
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }
}

/// <summary>
/// Modelo de requisição para o script insertBoAPI do PHC WEB
/// Inclui campos opcionais: Boano, Estab, Nome, Data, Moeda
/// </summary>
public class PhcInsertBoRequest
{
    public int Ndos { get; set; }
    public int No { get; set; }
    public int? Boano { get; set; }
    public int? Estab { get; set; }
    public string? Nome { get; set; }
    public string? Data { get; set; }
    public string? Moeda { get; set; }
    public string? CreatedBy { get; set; }
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }
    public List<PhcLinhaModel> LstBi { get; set; } = new();
}

/// <summary>
/// Modelo de resposta do script PHC WEB
/// </summary>
public class PhcInsertBoResponse
{
    public string Nmdos { get; set; } = string.Empty;
    public string Ndos { get; set; } = string.Empty;
    public string Obrano { get; set; } = string.Empty;
    public string Boano { get; set; } = string.Empty;
    public string No { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Estab { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string Moeda { get; set; } = string.Empty;
    public decimal Total { get; set; }
    /// <summary>Campos de utilizador do cabeçalho devolvidos pelo PHC (chave = tableName, valor = {columnName: value})</summary>
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }
    public List<PhcLinhaResposta> Linhas { get; set; } = new();
}

/// <summary>
/// Modelo de linha na resposta do PHC
/// </summary>
public class PhcLinhaResposta
{
    public string Ref { get; set; } = string.Empty;
    public string Design { get; set; } = string.Empty;
    public decimal Qtt { get; set; }
    public string Tabiva { get; set; } = string.Empty;
    public decimal Iva { get; set; }
    public bool Ivaincl { get; set; }
    public decimal Debito { get; set; }
    public decimal Ttdeb { get; set; }
    /// <summary>Campos de utilizador da linha devolvidos pelo PHC (chave = tableName, valor = {columnName: value})</summary>
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }
}
