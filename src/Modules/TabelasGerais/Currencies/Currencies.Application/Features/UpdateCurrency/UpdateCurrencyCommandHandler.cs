using MediatR;
using Currencies.Application.DTOs;
using Currencies.Domain.Repositories;

namespace Currencies.Application.Features.UpdateCurrency;

/// <summary>
/// Handler para UpdateCurrencyCommand.
/// </summary>
public sealed class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, CurrencyOutputDTO?>
{
    private readonly ICbRepository _cbRepository;

    /// <summary>
    /// Inicializa uma nova instância do handler.
    /// </summary>
    public UpdateCurrencyCommandHandler(ICbRepository cbRepository)
    {
        _cbRepository = cbRepository;
    }

    /// <summary>
    /// Executa a atualização da moeda.
    /// </summary>
    public async Task<CurrencyOutputDTO?> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var updated = await _cbRepository.UpdateAsync(request.Moeda, request.Pais, request.UpdatedBy, cancellationToken);

        if (!updated)
            return null;

        return new CurrencyOutputDTO
        {
            Moeda = request.Moeda.Trim().ToUpper(),
            Pais = request.Pais.Trim()
        };
    }
}
