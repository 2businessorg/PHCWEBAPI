using Invoices.Application.DTOs;
using MediatR;

namespace Invoices.Application.Features.GetInvoiceById;

/// <summary>
/// Query para obter fatura pela chave composta (ndoc, invoiceNumber, year).
/// </summary>
public record GetInvoiceByIdQuery(int Ndoc, int InvoiceNumber, int Year) : IRequest<InvoiceOutputDTO?>;
