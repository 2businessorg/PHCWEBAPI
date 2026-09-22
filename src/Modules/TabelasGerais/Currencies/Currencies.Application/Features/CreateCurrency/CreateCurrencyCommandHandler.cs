using MediatR;
using Currencies.Application.DTOs;
using Currencies.Domain.Repositories;

namespace Currencies.Application.Features.CreateCurrency;

/// <summary>
/// Handler para CreateCurrencyCommand.
/// Cria uma nova moeda na tabela cb.
/// </summary>
public sealed class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, CurrencyOutputDTO>
{
    private readonly ICbRepository _cbRepository;

    /// <summary>
    /// Inicializa uma nova instância do handler.
    /// </summary>
    public CreateCurrencyCommandHandler(ICbRepository cbRepository)
    {
        _cbRepository = cbRepository;
    }

    /// <summary>
    /// Executa a criação da moeda.
    /// </summary>
    public async Task<CurrencyOutputDTO> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        await _cbRepository.CreateAsync(request.Dto.Pais, request.Dto.Moeda, request.CreatedBy, cancellationToken);

        return new CurrencyOutputDTO { Moeda = request.Dto.Moeda.ToUpper(), Pais = request.Dto.Pais };
    }
}
