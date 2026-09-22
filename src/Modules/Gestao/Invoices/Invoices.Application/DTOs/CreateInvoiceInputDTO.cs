using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Invoices.Application.DTOs
{
    /// <summary>
    /// DTO para requisição de criação de fatura
    /// Estrutura semelhante a Dossiers mas específica para Faturas
    /// </summary>
    public class CreateInvoiceInputDTO
    {
        /// <summary>
        /// NDOC - Número interno do documento/tipo de documento (obrigatório)
        /// Tipo de fatura a criar (ex: 21 para "Venda a Dinheiro")
        /// </summary>
        [JsonPropertyName("docTypeId")]
        public int Ndoc { get; set; }

        /// <summary>
        /// NO - Número do cliente (obrigatório)
        /// Referência na tabela CL - Clientes
        /// </summary>
        [JsonPropertyName("clientId")]
        public int No { get; set; }

        /// <summary>
        /// FTANO - Ano da fatura (opcional)
        /// Padrão: ano atual
        /// </summary>
        [JsonPropertyName("year")]
        public int? Ftano { get; set; }

        /// <summary>
        /// ESTAB - Estabelecimento do cliente (opcional)
        /// Padrão: 0 (estabelecimento principal)
        /// </summary>
        [JsonPropertyName("clientBranch")]
        public int? Estab { get; set; }

        /// <summary>
        /// NOME - Nome do cliente/entidade na fatura (opcional)
        /// Auto-resolvido da tabela de clientes se omitido
        /// </summary>
        [JsonPropertyName("clientName")]
        [AllowNull]
        public string? Nome { get; set; }

        /// <summary>
        /// MOEDA - Código ISO de moeda (opcional)
        /// Ex: MZN, USD, EUR
        /// Auto-resolvido da configuração se omitido
        /// </summary>
        [JsonPropertyName("currency")]
        [AllowNull]
        public string? Moeda { get; set; }

        /// <summary>
        /// CDATA - Data da fatura (opcional)
        /// Padrão: data atual
        /// Obrigatório em guias de remessa
        /// Formato: YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("date")]
        [AllowNull]
        public string? Data { get; set; }

        /// <summary>
        /// Array de linhas de fatura (obrigatório)
        /// Mínimo uma linha
        /// </summary>
        [JsonPropertyName("lines")]
        public List<CreateInvoiceLineInputDTO> Linhas { get; set; } = new List<CreateInvoiceLineInputDTO>();

        /// <summary>
        /// Observações gerais (opcional)
        /// </summary>
        [JsonPropertyName("notes")]
        [AllowNull]
        public string? Observacoes { get; set; }

        /// <summary>
        /// Campos adicionais de cabeçalho configurados no tenant (ft/ft2/ft3)
        /// Chave = alias definido em u_addfields, Valor = valor a gravar
        /// </summary>
        [JsonPropertyName("addFields")]
        public Dictionary<string, object?>? AddFields { get; set; }

        /// <summary>
        /// Transporte interno: addFieldsByTable resolvido pelo handler (tableName → {columnName: value}).
        /// Populado em CreateFaturaCommandHandler, consumido pelo FtService. Não serializado.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Dictionary<string, Dictionary<string, object?>>? AddFieldsByTable { get; set; }

        /// <summary>
        /// Transporte interno: utilizador que criou o documento. Populado pelo handler, não serializado.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string? CreatedBy { get; set; }
    }
}
