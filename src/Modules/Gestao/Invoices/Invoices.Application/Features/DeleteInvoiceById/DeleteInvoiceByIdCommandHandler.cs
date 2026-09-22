using Invoices.Domain.Repositories;
using MediatR;

namespace Invoices.Application.Features.DeleteInvoiceById;

/// <summary>
/// Handler para eliminar fatura por chave composta.
/// </summary>
public class DeleteInvoiceByIdCommandHandler : IRequestHandler<DeleteInvoiceByIdCommand, bool>
{
    private readonly IFaturaRepository _repository;

    public DeleteInvoiceByIdCommandHandler(IFaturaRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> Handle(DeleteInvoiceByIdCommand request, CancellationToken cancellationToken)
        => _repository.DeleteAsync(request.Ndoc, request.InvoiceNumber, request.Year);
}
