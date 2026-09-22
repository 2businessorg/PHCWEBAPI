using System;

namespace Invoices.Domain.Entities
{
    /// <summary>
    /// Entidade TD - Configuração de Documentos (Tipos de Fatura)
    /// Armazena definições e configurações para cada tipo de documento de faturação
    /// </summary>
    public class TD
    {
        /// <summary>
        /// NDOC - Número único do documento (chave primária)
        /// Identificador interno do tipo de fatura
        /// </summary>
        public int Ndoc { get; set; }

        /// <summary>
        /// NMDOC - Nome do documento no singular
        /// Ex: "Fatura", "Guia de Remessa"
        /// </summary>
        public string NmDoc { get; set; }

        /// <summary>
        /// NMDOCP - Nome do documento no plural
        /// Ex: "Faturas", "Guias de Remessa"
        /// </summary>
        public string NmDocP { get; set; }

        /// <summary>
        /// NMDOCA - Abreviatura do nome do documento
        /// Ex: "FT", "GR"
        /// </summary>
        public string NmDocA { get; set; }

        /// <summary>
        /// Série actual do documento
        /// </summary>
        public int Serie { get; set; }

        /// <summary>
        /// GUIAREMESSA - Indica se é um documento de guia de remessa
        /// </summary>
        public bool GuiaRemessa { get; set; }

        /// <summary>
        /// AUTOFAT - É um documento de autofaturação?
        /// </summary>
        public bool AutoFat { get; set; }

        /// <summary>
        /// LANCACC - Lança movimentos em conta corrente?
        /// </summary>
        public bool LancaCc { get; set; }

        /// <summary>
        /// LANCASL - Lança movimentos em stocks?
        /// </summary>
        public bool LancaSl { get; set; }

        /// <summary>
        /// LANCAOL - Lança movimentos em tesouraria?
        /// </summary>
        public bool LancaOl { get; set; }

        /// <summary>
        /// AUTOML - Documentos integrados na contabilidade automaticamente?
        /// </summary>
        public bool AutoMl { get; set; }

        /// <summary>
        /// FECHADA - Série de documentos fechada?
        /// </summary>
        public bool Fechada { get; set; }

        /// <summary>
        /// EXCLUISAFT - Excluir do SAF-T?
        /// </summary>
        public bool ExcluiSaft { get; set; }

        /// <summary>
        /// Limite para emissão em valor
        /// </summary>
        public decimal LimiteSimp { get; set; }

        /// <summary>
        /// Número de decimais nos valores
        /// </summary>
        public int PreDec { get; set; }

        /// <summary>
        /// Número de decimais na quantidade
        /// </summary>
        public int QttDec { get; set; }

        /// <summary>
        /// Data de criação do registo
        /// </summary>
        public DateTime OusrData { get; set; }

        /// <summary>
        /// Hora de criação do registo
        /// </summary>
        public string OusrHora { get; set; }

        /// <summary>
        /// Utilizador que criou o registo
        /// </summary>
        public string OusrInis { get; set; }

        /// <summary>
        /// Id do utilizador autenticado (AspNetUsersId)
        /// Usado para multi-tenancy
        /// </summary>
        public string AspNetUsersId { get; set; }
    }
}
