using Invoices.Application.DTOs;
using MediatR;

namespace Invoices.Application.Features.CreateFatura;

/// <summary>
/// Command para criar uma nova fatura via PHC Web
/// </summary>
public class CreateFaturaCommand : IRequest<InvoiceOutputDTO>
{
    public CreateInvoiceInputDTO Request { get; set; } = new();
    public string? CreatedBy { get; set; }
}
