using MediatR;
using Currencies.Domain.Repositories;

namespace Currencies.Application.Features.DeleteCurrency;

/// <summary>
/// Handler para DeleteCurrencyCommand.
/// </summary>
public sealed class DeleteCurrencyCommandHandler : IRequestHandler<DeleteCurrencyCommand, bool>
{
    private readonly ICbRepository _cbRepository;

    /// <summary>
    /// Inicializa uma nova instância do handler.
    /// </summary>
    public DeleteCurrencyCommandHandler(ICbRepository cbRepository)
    {
        _cbRepository = cbRepository;
    }

    /// <summary>
    /// Executa a eliminação da moeda.
    /// </summary>
    public async Task<bool> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        return await _cbRepository.DeleteAsync(request.Moeda, cancellationToken);
    }
}
