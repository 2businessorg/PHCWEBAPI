using MediatR;
using Receipts.Application.DTOs;

namespace Receipts.Application.Features.GetReceiptById;

/// <summary>
/// Query para obter um Recibo por chave composta (ndoc + rno + reano)
/// </summary>
public record GetReceiptByIdQuery(decimal Ndoc, decimal Rno, decimal Reano) : IRequest<ReceiptOutputDTO?>;
