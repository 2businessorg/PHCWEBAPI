using System;

namespace Invoices.Domain.Entities
{
    /// <summary>
    /// Entidade FI2 - Dados Adicionais de Linhas de Fatura
    /// Complementa informações adicionais para cada linha de fatura
    /// </summary>
    public class Fi2
    {
        public string Fi2Stamp { get; set; } = string.Empty;
        public string Ftstamp { get; set; } = string.Empty;
        public string Ousrinis { get; set; } = string.Empty;
        public DateTime Ousrdata { get; set; }
        public string Ousrhora { get; set; } = string.Empty;
        public string Usrinis { get; set; } = string.Empty;
        public DateTime Usrdata { get; set; }
        public string Usrhora { get; set; } = string.Empty;
        public bool Marcada { get; set; }
    }
}
