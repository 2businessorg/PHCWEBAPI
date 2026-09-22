namespace Stocks.Application.Mappers;

using Stocks.Application.DTOs;

/// <summary>
/// Mapper para normalizar e transformar dados de Stock.
/// </summary>
public static class StockMapper
{
    /// <summary>
    /// Normaliza campos de string (trim) e decimal (2 casas decimais).
    /// </summary>
    /// <param name="referencia">Referência do stock (com espaços em branco)</param>
    /// <param name="descricao">Descrição do stock (com espaços em branco)</param>
    /// <param name="pCusto">Preço de custo com múltiplas casas decimais</param>
    /// <returns>Tupla com valores normalizados</returns>
    public static (string Referencia, string Descricao, decimal PCusto) NormalizeStockData(
        string referencia,
        string descricao,
        decimal pCusto)
    {
        var normalizedRef = (referencia ?? string.Empty).Trim();
        var normalizedDesc = (descricao ?? string.Empty).Trim();
        var normalizedPrice = Math.Round(pCusto, 2, MidpointRounding.AwayFromZero);

        return (normalizedRef, normalizedDesc, normalizedPrice);
    }

    /// <summary>
    /// Normaliza um decimal para 2 casas decimais.
    /// </summary>
    public static decimal NormalizeDecimal(decimal value, int decimalPlaces = 2)
        => Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Normaliza um string com trim.
    /// </summary>
    public static string NormalizeString(string? value)
        => (value ?? string.Empty).Trim();

    /// <summary>
    /// Converte array de PrecoTabelaDTO para campos individuais Pv1-5 e Iva1-5incl.
    /// Garante que existem exatamente 5 preços (um por tabela).
    /// </summary>
    public static (decimal pv1, bool iva1, decimal pv2, bool iva2, decimal pv3, bool iva3, decimal pv4, bool iva4, decimal pv5, bool iva5) ConvertPrecosArrayToFields(List<PrecoTabelaDTO>? precos)
    {
        if (precos is null || precos.Count == 0)
        {
            // Retorna valores padrão (zeros para preços, falso para IVA)
            return (0m, false, 0m, false, 0m, false, 0m, false, 0m, false);
        }

        // Cria dicionário para fácil acesso por tabela
        var precoDict = precos.ToDictionary(p => p.Tabela, p => p);

        decimal GetPreco(int tabela) => precoDict.ContainsKey(tabela) ? NormalizeDecimal(precoDict[tabela].Valor) : 0m;
        bool GetIva(int tabela) => precoDict.ContainsKey(tabela) && precoDict[tabela].IvaIncluido;

        return (
            GetPreco(1), GetIva(1),
            GetPreco(2), GetIva(2),
            GetPreco(3), GetIva(3),
            GetPreco(4), GetIva(4),
            GetPreco(5), GetIva(5)
        );
    }

    /// <summary>
    /// Converte campos individuais Pv1-5 e Iva1-5incl para array de PrecoTabelaDTO.
    /// </summary>
    public static List<PrecoTabelaDTO> ConvertFieldsToPrecosArray(
        decimal pv1, bool iva1,
        decimal pv2, bool iva2,
        decimal pv3, bool iva3,
        decimal pv4, bool iva4,
        decimal pv5, bool iva5)
    {
        return new List<PrecoTabelaDTO>
        {
            new PrecoTabelaDTO { Tabela = 1, Valor = NormalizeDecimal(pv1), IvaIncluido = iva1 },
            new PrecoTabelaDTO { Tabela = 2, Valor = NormalizeDecimal(pv2), IvaIncluido = iva2 },
            new PrecoTabelaDTO { Tabela = 3, Valor = NormalizeDecimal(pv3), IvaIncluido = iva3 },
            new PrecoTabelaDTO { Tabela = 4, Valor = NormalizeDecimal(pv4), IvaIncluido = iva4 },
            new PrecoTabelaDTO { Tabela = 5, Valor = NormalizeDecimal(pv5), IvaIncluido = iva5 }
        };
    }

    /// <summary>
    /// Normaliza um StockOutputDTO completo (trim em strings, 2 casas decimais em valores).
    /// </summary>
    public static StockOutputDTO NormalizeOutput(StockOutputDTO? dto)
    {
        if (dto is null)
            return new StockOutputDTO();

        return new StockOutputDTO
        {
            Referencia = NormalizeString(dto.Referencia),
            Descricao = NormalizeString(dto.Descricao),
            Eservico = dto.Eservico,
            Precos = dto.Precos
                .Select(p => new PrecoTabelaDTO
                {
                    Tabela = p.Tabela,
                    Valor = NormalizeDecimal(p.Valor),
                    IvaIncluido = p.IvaIncluido
                })
                .ToList(),
            Stock = NormalizeDecimal(dto.Stock),
            TabIva = dto.TabIva,
            FamiliaRef = NormalizeString(dto.FamiliaRef),
            FamiliaNome = NormalizeString(dto.FamiliaNome),
            Obs = NormalizeString(dto.Obs),
            Inactivo = dto.Inactivo,
            UsaLote = dto.UsaLote,
            AddFields = dto.AddFields
        };
    }
}
