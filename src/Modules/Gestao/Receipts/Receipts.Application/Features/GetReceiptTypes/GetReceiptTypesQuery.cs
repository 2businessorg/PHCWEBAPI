using MediatR;
using Receipts.Application.DTOs;

namespace Receipts.Application.Features.GetReceiptTypes;

/// <summary>
/// Query para listar as séries/tipos de recibo disponíveis
/// </summary>
public record GetReceiptTypesQuery : IRequest<IReadOnlyList<ReceiptTypeOutputDTO>>;
