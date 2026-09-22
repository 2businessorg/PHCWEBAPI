using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Mappings;

/// <summary>
/// Mapeamentos para saída de moedas.
/// </summary>
public static class MoedaMappings
{
    public static MoedaOutputDTO ToOutputDto(this string moeda)
    {
        return new MoedaOutputDTO
        {
            Moeda = moeda?.Trim() ?? string.Empty
        };
    }

    public static IEnumerable<MoedaOutputDTO> ToOutputDtos(this IEnumerable<string> moedas)
    {
        return moedas
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(m => m.ToOutputDto())
            .DistinctBy(m => m.Moeda)
            .OrderBy(m => m.Moeda)
            .ToList();
    }
}