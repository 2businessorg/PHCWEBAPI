using Invoices.Application.DTOs;
using System.Threading.Tasks;

namespace Invoices.Application.ExternalServices
{
    /// <summary>
    /// Interface para integração com PHC Web para criação de faturas.
    /// </summary>
    public interface IFtService
    {
        Task<InvoiceOutputDTO> CreateFaturaAsync(CreateInvoiceInputDTO request);
    }
}
