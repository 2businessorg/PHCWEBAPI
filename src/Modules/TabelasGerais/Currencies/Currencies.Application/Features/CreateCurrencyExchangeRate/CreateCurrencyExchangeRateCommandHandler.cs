using Currencies.Application.DTOs;
using Currencies.Application.Errors;
using Currencies.Domain.Repositories;
using MediatR;

namespace Currencies.Application.Features.CreateCurrencyExchangeRate;

/// <summary>
/// Handler para CreateCurrencyExchangeRateCommand.
/// </summary>
public sealed class CreateCurrencyExchangeRateCommandHandler : IRequestHandler<CreateCurrencyExchangeRateCommand, CurrencyExchangeRateOutputDTO>
{
    private readonly ICbRepository _cbRepository;
    private readonly ICurrencyRepository _currencyRepository;

    /// <summary>
    /// Inicializa uma nova instância do handler.
    /// </summary>
    public CreateCurrencyExchangeRateCommandHandler(ICbRepository cbRepository, ICurrencyRepository currencyRepository)
    {
        _cbRepository = cbRepository;
        _currencyRepository = currencyRepository;
    }

    /// <summary>
    /// Executa a criação da taxa de conversão.
    /// </summary>
    public async Task<CurrencyExchangeRateOutputDTO> Handle(CreateCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var currencyExists = await _currencyRepository.ExistsAsync(request.Dto.Moeda, cancellationToken);

        if (!currencyExists)
        {
            throw new CurrenciesModuleException(
                CurrenciesErrorCatalog.CurrencyNotFound.Code,
                $"Moeda '{request.Dto.Moeda}' não existe");
        }

        // Verifica se a combinação moeda + país já foi criada antes
        var exchangeRateExists = await _currencyRepository.ExchangeRateExistsAsync(
            request.Dto.Moeda, 
            request.Dto.Pais, 
            cancellationToken);

        if (!exchangeRateExists)
        {
            throw new CurrenciesModuleException(
                CurrenciesErrorCatalog.InvalidExchangeRateCombination.Code,
                $"Combinação de moeda '{request.Dto.Moeda}' e país '{request.Dto.Pais}' não foi criada anteriormente");
        }

        var data = request.Dto.Data ?? DateTime.Now.Date;

        await _cbRepository.CreateExchangeRateAsync(
            request.Dto.Pais,
            request.Dto.Moeda,
            request.Dto.UCambioC,
            request.Dto.UCambioV,
            data,
            request.CreatedBy,
            cancellationToken);

        return new CurrencyExchangeRateOutputDTO
        {
            Moeda = request.Dto.Moeda.Trim().ToUpper(),
            Pais = request.Dto.Pais.Trim(),
            CambioCompra = request.Dto.UCambioC,
            CambioVenda = request.Dto.UCambioV,
            Data = data
        };
    }
}
