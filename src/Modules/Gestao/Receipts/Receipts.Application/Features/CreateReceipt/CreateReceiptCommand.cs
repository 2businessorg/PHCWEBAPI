using MediatR;
using Receipts.Application.DTOs;

namespace Receipts.Application.Features.CreateReceipt;

/// <summary>
/// Command para criação de um Recibo via PHC WEB (script insertReAPI)
/// </summary>
public record CreateReceiptCommand(CreateReceiptInputDTO Dto, string? CreatedBy = null) : IRequest<CreateReceiptOutputDTO>;
