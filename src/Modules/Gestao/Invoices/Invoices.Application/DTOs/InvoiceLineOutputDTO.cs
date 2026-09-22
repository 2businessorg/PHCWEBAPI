using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Invoices.Application.DTOs
{
    /// <summary>
    /// DTO para linha de fatura em resposta
    /// </summary>
    public class InvoiceLineOutputDTO
    {
        /// <summary>
        /// REF - Referência do produto
        /// </summary>
        [JsonPropertyName("productCode")]
        public string Ref { get; set; } = string.Empty;

        /// <summary>
        /// DESIGN - Descrição da linha/produto
        /// </summary>
        [JsonPropertyName("productName")]
        public string Design { get; set; } = string.Empty;

        /// <summary>
        /// QTT - Quantidade
        /// </summary>
        [JsonPropertyName("quantity")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Qtt { get; set; }

        /// <summary>
        /// PV - Preço de venda unitário
        /// </summary>
        [JsonPropertyName("unitPrice")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Pv { get; set; }

        /// <summary>
        /// PVMOEDA - Moeda estrangeira: Preço unitário
        /// </summary>
        [JsonPropertyName("unitPriceForeignCurrency")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Pvmoeda { get; set; }

        /// <summary>
        /// TABIVA - Código da tabela de IVA
        /// </summary>
        [JsonPropertyName("vatCode")]
        public int Tabiva { get; set; }

        /// <summary>
        /// IVA - Percentagem de IVA
        /// </summary>
        [JsonPropertyName("vatRate")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Iva { get; set; }


        /// <summary>
        /// IvaIncl - Iva Incluso
        /// </summary>
        [JsonPropertyName("vatIncluded")]
        public bool IvaIncl { get; set; }

        /// <summary>
        /// TTDEB - Total com IVA
        /// </summary>
        [JsonPropertyName("total")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Ttdeb { get; set; }

        /// <summary>
        /// TMOEDA - Moeda estrangeira: Total
        /// </summary>
        [JsonPropertyName("totalForeignCurrency")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Tmoeda { get; set; }

        /// <summary>
        /// ARMAZEM - Número do armazém
        /// </summary>
        [JsonPropertyName("warehouse")]
        public int Armazem { get; set; }

        /// <summary>
        /// LOTE - Número de lote
        /// </summary>
        [JsonPropertyName("batch")]
        public string Lote { get; set; } = string.Empty;

        /// <summary>
        /// Campos adicionais da linha devolvidos pelo PHC (alias = valor gravado)
        /// </summary>
        [JsonPropertyName("addFields")]
        public Dictionary<string, object?>? AddFields { get; set; }

        /// <summary>
        /// Transporte interno: addFieldsByTable raw do PHC. Não serializado.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTableRaw { get; set; }
    }
}
