using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Invoices.Application.DTOs
{
    /// <summary>
    /// DTO para resposta de fatura criada
    /// </summary>
    public class InvoiceOutputDTO
    {
        /// <summary>
        /// NDOC - Número interno do documento/tipo de documento
        /// </summary>
        [JsonPropertyName("docTypeId")]
        public int Ndoc { get; set; }

        /// <summary>
        /// NMDOC - Nome do tipo de documento
        /// </summary>
        [JsonPropertyName("docTypeName")]
        public string NmDoc { get; set; } = string.Empty;

        /// <summary>
        /// FNO - Número de fatura
        /// </summary>
        [JsonPropertyName("invoiceNumber")]
        public int Fno { get; set; }

        /// <summary>
        /// FTANO - Ano da fatura
        /// </summary>
        [JsonPropertyName("year")]
        public int Ftano { get; set; }

        /// <summary>
        /// NO - Número do cliente
        /// </summary>
        [JsonPropertyName("clientId")]
        public int No { get; set; }

        /// <summary>
        /// NOME - Nome do cliente
        /// </summary>
        [JsonPropertyName("clientName")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// ESTAB - Estabelecimento do cliente
        /// </summary>
        [JsonPropertyName("clientBranch")]
        public int Estab { get; set; }

        /// <summary>
        /// CDATA - Data da fatura
        /// </summary>
        [JsonPropertyName("date")]
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// MOEDA - Código de moeda
        /// </summary>
        [JsonPropertyName("currency")]
        public string Moeda { get; set; } = string.Empty;

        /// <summary>
        /// TTIVA - Total de IVA
        /// </summary>
        [JsonPropertyName("totalTax")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Ttiva { get; set; }

        /// <summary>
        /// TMIVA - Moeda estrangeira: Total de IVA
        /// </summary>
        [JsonPropertyName("totalTaxForeignCurrency")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal TMIva { get; set; }

        /// <summary>
        /// TILIQUIDO - Total com IVA (líquido)
        /// </summary>
        [JsonPropertyName("total")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal Total { get; set; }

        /// <summary>
        /// TOTALMOEDA - Moeda estrangeira: Total do Documento
        /// </summary>
        [JsonPropertyName("totalForeignCurrency")]
        [JsonConverter(typeof(DecimalFormatConverter))]
        public decimal TotalMoeda { get; set; }

        /// <summary>
        /// Observações da fatura
        /// </summary>
        [JsonPropertyName("notes")]
        public string Observacoes { get; set; } = string.Empty;

        /// <summary>
        /// <summary>
        /// Campos adicionais de cabeçalho devolvidos pelo PHC (alias = valor gravado)
        /// </summary>
        [JsonPropertyName("addFields")]
        public Dictionary<string, object?>? AddFields { get; set; }

        /// <summary>
        /// Transporte interno: addFieldsByTable raw do PHC (tableName → {columnName: value}).
        /// Populado pelo FtService, traduzido para AddFields pelo handler. Não serializado.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTableRaw { get; set; }

        /// Array de linhas da fatura
        /// </summary>
        [JsonPropertyName("lines")]
        public List<InvoiceLineOutputDTO> Linhas { get; set; } = new List<InvoiceLineOutputDTO>();
    }
}
