using System;

namespace Invoices.Domain.Entities
{
    /// <summary>
    /// Entidade FT2 - Dados Secundários de Fatura
    /// Complementa informações adicionais do documento de fatura
    /// </summary>
    public class FT2
    {
        public string Ft2Stamp { get; set; } = string.Empty;
        public string LocalEntrega { get; set; } = string.Empty;
        public DateTime DataEntrega { get; set; }
        public string HoraEntrega { get; set; } = string.Empty;
        public string MoradaEntrega { get; set; } = string.Empty;
        public string LocalLocEnt { get; set; } = string.Empty;
        public string CodPEntrega { get; set; } = string.Empty;
        public string ObsDoc { get; set; } = string.Empty;
        public string FormaPag { get; set; } = string.Empty;
        public string Ousrinis { get; set; } = string.Empty;
        public DateTime Ousrdata { get; set; }
        public string Ousrhora { get; set; } = string.Empty;
        public string Usrinis { get; set; } = string.Empty;
        public DateTime Usrdata { get; set; }
        public string Usrhora { get; set; } = string.Empty;
        public bool Marcada { get; set; }
    }
}
