using System;

namespace Invoices.Domain.Entities
{
    /// <summary>
    /// Entidade FT3 - Dados Adicionais de Fatura
    /// Complementa informações adicionais e metadados do documento de fatura
    /// </summary>
    public class FT3
    {
        public string Ft3Stamp { get; set; } = string.Empty;
        public string OUsrInis { get; set; } = string.Empty;
        public DateTime OUsrData { get; set; }
        public string OUsrHora { get; set; } = string.Empty;
        public string UsrInis { get; set; } = string.Empty;
        public DateTime UsrData { get; set; }
        public string UsrHora { get; set; } = string.Empty;
        public bool Marcada { get; set; }
    }
}
