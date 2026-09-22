using Currencies.Application.DTOs;
using Currencies.Domain.ValueObjects;

namespace Currencies.Application.Mappings;

public static class CurrencyMappings
{
    public static IEnumerable<CurrencyOutputDTO> ToOutputDtos(this IEnumerable<CurrencyData> currencies)
        => currencies.Select(c => new CurrencyOutputDTO { Moeda = c.Moeda, Pais = c.Pais });

    public static CurrencyOutputDTO ToOutputDto(this CurrencyData currency)
        => new() { Moeda = currency.Moeda, Pais = currency.Pais };
}
