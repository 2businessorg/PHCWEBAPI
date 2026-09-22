using MediatR;
using Receipts.Application.DTOs;
using Receipts.Domain.Repositories;

namespace Receipts.Application.Features.GetReceiptTypes;

/// <summary>
/// Handler para listagem de séries de Recibo (tsre)
/// </summary>
public class GetReceiptTypesQueryHandler : IRequestHandler<GetReceiptTypesQuery, IReadOnlyList<ReceiptTypeOutputDTO>>
{
    private readonly IReceiptTypeRepository _receiptTypeRepository;

    public GetReceiptTypesQueryHandler(IReceiptTypeRepository receiptTypeRepository)
    {
        _receiptTypeRepository = receiptTypeRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReceiptTypeOutputDTO>> Handle(GetReceiptTypesQuery request, CancellationToken cancellationToken)
    {
        var types = await _receiptTypeRepository.GetAllAsync(cancellationToken);

        return types
            .Select(t => new ReceiptTypeOutputDTO
            {
                Ndoc = t.Ndoc,
                Nmdoc = t.Nmdoc
            })
            .ToList();
    }
}
