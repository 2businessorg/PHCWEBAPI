using Dossiers.Domain.Models;
using Newtonsoft.Json;

namespace Dossiers.Infrastructure.ExternalServices;

/// <summary>
/// Envelope de resposta do script PHC (com success flag e código de erro)
/// </summary>
public class PhcScriptResponseEnvelope
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("data")]
    public PhcInsertBoResponseDto? Data { get; set; }
}

/// <summary>
/// DTOs para JSON com atributos de serialização (Infrastructure layer)
/// </summary>

/// <summary>
/// DTO de requisição para serializar para JSON
/// Inclui campos opcionais: Boano, Estab, Nome, Data, Moeda
/// </summary>
public class PhcInsertBoRequestDto
{
    [JsonProperty("Ndos")]
    public int Ndos { get; set; }

    [JsonProperty("No")]
    public decimal No { get; set; }

    [JsonProperty("Boano")]
    public int? Boano { get; set; }

    [JsonProperty("Estab")]
    public int? Estab { get; set; }

    [JsonProperty("Nome")]
    public string? Nome { get; set; }

    [JsonProperty("Data")]
    public string? Data { get; set; }

    [JsonProperty("Moeda")]
    public string? Moeda { get; set; }

    [JsonProperty("CreatedBy")]
    public string? CreatedBy { get; set; }

    [JsonProperty("AddFieldsByTable")]
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }

    [JsonProperty("LstBi")]
    public List<PhcLinhaModelDto> LstBi { get; set; } = new();

    /// <summary>
    /// Converte modelo do Domain para DTO de serialização
    /// </summary>
    public static PhcInsertBoRequestDto FromDomain(PhcInsertBoRequest request)
    {
        return new PhcInsertBoRequestDto
        {
            Ndos = request.Ndos,
            No = request.No,
            Boano = request.Boano,
            Estab = request.Estab,
            Nome = request.Nome,
            Data = request.Data,
            Moeda = request.Moeda,
            CreatedBy = request.CreatedBy,
            AddFieldsByTable = request.AddFieldsByTable,
            LstBi = request.LstBi.Select(l => PhcLinhaModelDto.FromDomain(l)).ToList()
        };
    }
}

/// <summary>
/// DTO de linha para requisição
/// Inclui campos opcionais: Design, PrecoUnitario, TabIva, IvaIncl
/// </summary>
public class PhcLinhaModelDto
{
    [JsonProperty("Ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonProperty("Qtt")]
    public decimal Qtt { get; set; }

    [JsonProperty("Design")]
    public string? Design { get; set; }

    [JsonProperty("PrecoUnitario")]
    public decimal? PrecoUnitario { get; set; }

    [JsonProperty("TabIva")]
    public int? TabIva { get; set; }

    [JsonProperty("IvaIncl")]
    public bool? IvaIncl { get; set; }

    [JsonProperty("AddFieldsByTable")]
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }

    public static PhcLinhaModelDto FromDomain(PhcLinhaModel model)
    {
        return new PhcLinhaModelDto
        {
            Ref = model.Ref,
            Qtt = model.Qtt,
            Design = model.Design,
            PrecoUnitario = model.PrecoUnitario,
            TabIva = model.TabIva,
            IvaIncl = model.IvaIncl,
            AddFieldsByTable = model.AddFieldsByTable
        };
    }
}

/// <summary>
/// DTO de resposta para desserializar do JSON
/// </summary>
public class PhcInsertBoResponseDto
{
    [JsonProperty("nmdos")]
    public string Nmdos { get; set; } = string.Empty;

    [JsonProperty("ndos")]
    public string Ndos { get; set; } = string.Empty;

    [JsonProperty("obrano")]
    public string Obrano { get; set; } = string.Empty;

    [JsonProperty("boano")]
    public string Boano { get; set; } = string.Empty;

    [JsonProperty("no")]
    public string No { get; set; } = string.Empty;

    [JsonProperty("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonProperty("estab")]
    public string Estab { get; set; } = string.Empty;

    [JsonProperty("data")]
    public string Data { get; set; } = string.Empty;

    [JsonProperty("moeda")]
    public string Moeda { get; set; } = string.Empty;

    [JsonProperty("total")]
    public decimal Total { get; set; }

    [JsonProperty("linhas")]
    public List<PhcLinhaRespostaDto> Linhas { get; set; } = new();

    [JsonProperty("addFieldsByTable")]
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }

    /// <summary>
    /// Converte DTO para modelo do Domain
    /// </summary>
    public PhcInsertBoResponse ToDomain()
    {
        return new PhcInsertBoResponse
        {
            Nmdos = Nmdos,
            Ndos = Ndos,
            Obrano = Obrano,
            Boano = Boano,
            No = No,
            Nome = Nome,
            Estab = Estab,
            Data = Data,
            Moeda = Moeda,
            Total = Total,
            AddFieldsByTable = AddFieldsByTable,
            Linhas = Linhas.Select(l => l.ToDomain()).ToList()
        };
    }
}

/// <summary>
/// DTO de linha de resposta
/// </summary>
public class PhcLinhaRespostaDto
{
    [JsonProperty("ref")]
    public string Ref { get; set; } = string.Empty;

    [JsonProperty("design")]
    public string Design { get; set; } = string.Empty;

    [JsonProperty("qtt")]
    public decimal Qtt { get; set; }

    [JsonProperty("tabiva")]
    public string Tabiva { get; set; } = string.Empty;

    [JsonProperty("iva")]
    public decimal Iva { get; set; }

    [JsonProperty("ivaincl")]
    public bool Ivaincl { get; set; }

    [JsonProperty("debito")]
    public decimal Debito { get; set; }

    [JsonProperty("ttdeb")]
    public decimal Ttdeb { get; set; }

    [JsonProperty("addFieldsByTable")]
    public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }

    public PhcLinhaResposta ToDomain()
    {
        return new PhcLinhaResposta
        {
            Ref = Ref,
            Design = Design,
            Qtt = Qtt,
            Tabiva = Tabiva,
            Iva = Iva,
            Ivaincl = Ivaincl,
            Debito = Debito,
            Ttdeb = Ttdeb,
            AddFieldsByTable = AddFieldsByTable
        };
    }
}
