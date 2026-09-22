using System;

namespace Invoices.Domain.Entities
{
    /// <summary>
    /// Entidade FI - Linhas de Fatura
    /// Representa cada linha de um documento de fatura
    /// </summary>
    public class Fi
    {
        public string Fistamp { get; set; } = string.Empty;
        public string Nmdoc { get; set; } = string.Empty;
        public decimal Fno { get; set; }
        public string Ref { get; set; } = string.Empty;
        public string Design { get; set; } = string.Empty;
        public decimal Qtt { get; set; }
        public decimal Tiliquido { get; set; }
        public decimal Etiliquido { get; set; }
        public decimal Iva { get; set; }
        public bool Ivaincl { get; set; }
        public decimal Tabiva { get; set; }
        public decimal Ndoc { get; set; }
        public decimal Armazem { get; set; }
        public string Lote { get; set; } = string.Empty;
        public string Usr1 { get; set; } = string.Empty;
        public string Usr2 { get; set; } = string.Empty;
        public string Usr3 { get; set; } = string.Empty;
        public string Usr4 { get; set; } = string.Empty;
        public string Usr5 { get; set; } = string.Empty;
        public string Usr6 { get; set; } = string.Empty;
        public string Ftstamp { get; set; } = string.Empty;
        public decimal Pv { get; set; }
        public decimal Pvmoeda { get; set; }
        public decimal Epv { get; set; }
        public decimal Tmoeda { get; set; }
        public string Ousrinis { get; set; } = string.Empty;
        public DateTime Ousrdata { get; set; }
        public string Ousrhora { get; set; } = string.Empty;
        public string Usrinis { get; set; } = string.Empty;
        public DateTime Usrdata { get; set; }
        public string Usrhora { get; set; } = string.Empty;
        public bool Marcada { get; set; }
        
    }
}
