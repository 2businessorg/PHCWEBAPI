using System;

namespace Invoices.Domain.Entities
{
    /// <summary>
    /// Entidade FT - Cabeçalho de Fatura (Documento de Faturação)
    /// Representa o cabeçalho de um documento de fatura no sistema PHC Web
    /// </summary>
    public class Ft
    {
        public string FtStamp { get; set; } = string.Empty;
        public decimal Pais { get; set; }
        public string NmDoc { get; set; } = string.Empty;
        public decimal Fno { get; set; }
        public decimal No { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime FData { get; set; }
        public decimal FtAno { get; set; }
        public decimal Ndoc { get; set; }
        public string Moeda { get; set; } = string.Empty;
        public decimal Estab { get; set; }
        public decimal Total { get; set; }
        public decimal TotalMoeda { get; set; }
        public decimal TtIva { get; set; }
        public decimal TMIva { get; set; }
        public string OUsrInis { get; set; } = string.Empty;
        public DateTime OUsrData { get; set; }
        public string OUsrHora { get; set; } = string.Empty;
        public string UsrInis { get; set; } = string.Empty;
        public DateTime UsrData { get; set; }
        public string UsrHora { get; set; } = string.Empty;
        public bool Marcada { get; set; }
    }
}
