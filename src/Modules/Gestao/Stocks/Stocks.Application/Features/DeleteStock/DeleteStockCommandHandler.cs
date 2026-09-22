using MediatR;
using Stocks.Domain.Repositories;

namespace Stocks.Application.Features.DeleteStock;

public class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand, bool>
{
    private readonly IStockRepository _repository;

    public DeleteStockCommandHandler(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteStockCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteByRefAsync(request.Referencia, cancellationToken);
    }
}
