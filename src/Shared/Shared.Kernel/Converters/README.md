# Decimal Format Converter

## Overview

O `DecimalFormatConverter` é um custom JSON converter centralizado para formatar valores `decimal` com um número específico de casas decimais durante a serialização JSON.

## Localização

- **Arquivo**: `src/Shared/Shared.Kernel/Shared.Kernel/Converters/DecimalFormatConverter.cs`
- **Namespace**: `Shared.Kernel.Converters`

## Converters Disponíveis

### 1. `DecimalFormatConverter`
Formata valores `decimal` não-nulos com 2 casas decimais (padrão).

```csharp
[JsonConverter(typeof(DecimalFormatConverter))]
public decimal Stock { get; set; }
```

**Comportamento:**
- Input: `25.000` → Output: `"25.00"`
- Input: `10.5` → Output: `"10.50"`
- Input: `0.0` → Output: `"0.00"`

### 2. `NullableDecimalFormatConverter`
Formata valores `decimal?` (nullable) com 2 casas decimais. Retorna `null` se o valor for nulo.

```csharp
[JsonConverter(typeof(NullableDecimalFormatConverter))]
public decimal? OptionalPrice { get; set; }
```

**Comportamento:**
- Input: `25.000` → Output: `"25.00"`
- Input: `null` → Output: `null`

## Exemplo de Uso

### Em um DTO de aplicação:

```csharp
using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace YourModule.Application.DTOs;

public class ProductDTO
{
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = "";

    [JsonPropertyName("price")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Price { get; set; }

    [JsonPropertyName("discountPrice")]
    [JsonConverter(typeof(NullableDecimalFormatConverter))]
    public decimal? DiscountPrice { get; set; }

    [JsonPropertyName("quantity")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Quantity { get; set; }
}
```

### Resposta JSON esperada:

```json
{
  "reference": "PROD-001",
  "price": "99.99",
  "discountPrice": "89.50",
  "quantity": "100.00"
}
```

## Casos de Uso

- ✅ Preços e valores monetários
- ✅ Quantidades e volumes
- ✅ Percentuais e taxas
- ✅ Pesos e medidas
- ✅ Qualquer valor decimal que precise ser formatado

## Módulos que Usam

- **Stocks** - Preços, quantidades, custos
- **Dossiers** - Valores de linhas, descontos, impostos
- *Outros módulos conforme necessário*

## Customização

Se precisar de um número diferente de casas decimais (ex: 3, 4, etc), você pode:

1. **Criar um novo converter específico** herdando de `DecimalFormatConverter`:

```csharp
namespace YourModule.Application.Converters;

public class CurrencyFormatConverter : Shared.Kernel.Converters.DecimalFormatConverter
{
    // Mantém 2 casas decimais por padrão
    // Customize se necessário
}
```

2. **Ou modificar a constante** em `DecimalFormatConverter.cs`:

```csharp
private const int DecimalPlaces = 3; // Altere conforme necessário
```

## Notas Importantes

- O converter é aplicado **apenas na serialização** (escrita para JSON)
- A **desserialização** (leitura de JSON) mantém o valor original sem formatação
- Isso garante que valores internamente são precisos, mas a API retorna formatado
