using MediatR;
using Invoices.Application.DTOs;
using Invoices.Domain.Repositories;

namespace Invoices.Application.Features.GetInvoiceTypes;

/// <summary>
/// Handler para obter tipos de série de facturação (TD)
/// </summary>
public class GetInvoiceTypesQueryHandler : IRequestHandler<GetInvoiceTypesQuery, IReadOnlyList<InvoiceTypeOutputDTO>>
{
    private readonly ITDRepository _repository;

    public GetInvoiceTypesQueryHandler(ITDRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<InvoiceTypeOutputDTO>> Handle(GetInvoiceTypesQuery request, CancellationToken cancellationToken)
    {
        var rows = await _repository.GetAllAsync(cancellationToken);

        return rows
            .Select(x => new InvoiceTypeOutputDTO
            {
                Ndoc = x.Ndoc,
                Nmdoc = (x.NmDoc ?? string.Empty).Trim()
            })
            .ToList();
    }
}
