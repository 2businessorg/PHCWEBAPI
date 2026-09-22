using MediatR;

namespace Invoices.Application.Features.DeleteInvoiceById;

/// <summary>
/// Command para eliminar fatura pela chave composta (ndoc, invoiceNumber, year).
/// </summary>
public record DeleteInvoiceByIdCommand(int Ndoc, int InvoiceNumber, int Year) : IRequest<bool>;
