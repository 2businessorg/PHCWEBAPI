using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Invoices.Application.DTOs
{
    /// <summary>
    /// DTO para linha de fatura em requisição de criação
    /// </summary>
    public class CreateInvoiceLineInputDTO
    {
        /// <summary>
        /// REF - Referência do produto (obrigatório)
        /// </summary>
        [JsonPropertyName("productCode")]
        public string Ref { get; set; }

        /// <summary>
        /// DESIGN - Descrição da linha/produto (opcional)
        /// </summary>
        [JsonPropertyName("productName")]
        [AllowNull]
        public string? Design { get; set; }

        /// <summary>
        /// QTT - Quantidade (obrigatório)
        /// </summary>
        [JsonPropertyName("quantity")]
        public decimal Qtt { get; set; }

        /// <summary>
        /// ARMAZEM - Número do armazém (opcional)
        /// Padrão: 1
        /// </summary>
        [JsonPropertyName("warehouse")]
        public int? Armazem { get; set; }

        /// <summary>
        /// PV - Preço de venda unitário (opcional)
        /// Auto-resolvido do catálogo se omitido
        /// </summary>
        [JsonPropertyName("unitPrice")]
        public decimal? PV { get; set; }

        /// <summary>
        /// TABIVA - Código da tabela de IVA (opcional)
        /// Auto-resolvido do produto se omitido
        /// </summary>
        [JsonPropertyName("vatCode")]
        public int? TabIva { get; set; }

        /// <summary>
        /// IVAINCL - Indica se IVA está incluído no preço (opcional)
        /// Padrão: false (preço exclui IVA)
        /// </summary>
        [JsonPropertyName("vatIncluded")]
        public bool? IvaIncl { get; set; }

        /// <summary>
        /// <summary>
        /// Campos adicionais da linha configurados no tenant (fi/fi2)
        /// Chave = alias definido em u_addfields, Valor = valor a gravar
        /// </summary>
        [JsonPropertyName("addFields")]
        public Dictionary<string, object?>? AddFields { get; set; }

        /// <summary>
        /// Transporte interno: addFieldsByTable resolvido pelo handler. Não serializado.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }

        /// LOTE - Número de lote (opcional)
        /// </summary>
        [JsonPropertyName("batch")]
        [AllowNull]
        public string? Lote { get; set; }

        /// <summary>
        /// NROSERIE - Número de série (opcional)
        /// </summary>
        [JsonPropertyName("serialNumber")]
        [AllowNull]
        public string? NroSerie { get; set; }
    }
}
