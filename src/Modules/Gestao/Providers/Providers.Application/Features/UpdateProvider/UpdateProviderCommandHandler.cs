using MediatR;
using Providers.Application.DTOs.Providers;
using Providers.Application.Mappings;
using Providers.Domain.Entities;
using Providers.Domain.Repositories;
using Providers.Domain.Services;

namespace Providers.Application.Features.UpdateProvider;

public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, ProviderOutputDTO>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IProviderValueRepository _providerValueRepository;
    private readonly IEncryptionService _encryptionService;

    public UpdateProviderCommandHandler(
        IProviderRepository providerRepository,
        IProviderValueRepository providerValueRepository,
        IEncryptionService encryptionService)
    {
        _providerRepository = providerRepository;
        _providerValueRepository = providerValueRepository;
        _encryptionService = encryptionService;
    }

    public async Task<ProviderOutputDTO> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providerRepository.GetByStampAsync(request.UProviderStamp, cancellationToken);

        if (provider == null)
            throw new InvalidOperationException($"Provider with stamp '{request.UProviderStamp}' not found");

        // Atualizar o cabeçalho
        provider.UpdateEntity(request.Dto, request.AtualizadoPor);

        // Remover todos os valores antigos
        await _providerValueRepository.DeleteByProviderStampAsync(request.UProviderStamp, cancellationToken);
        provider.ClearValues();

        // Adicionar os novos valores
        foreach (var valueDto in request.Dto.Values)
        {
            var valorFinal = valueDto.Encriptado
                ? _encryptionService.Encrypt(valueDto.Valor)
                : valueDto.Valor;

            var providerValue = new ProviderValue(
                uProviderValuesStamp: Guid.NewGuid().ToString(),
                uProviderStamp: provider.UProviderStamp,
                operationCode: valueDto.OperationCode,
                chave: valueDto.Chave,
                valor: valorFinal,
                encriptado: valueDto.Encriptado,
                ordem: valueDto.Ordem,
                ativo: valueDto.Ativo,
                criadoPor: request.AtualizadoPor
            );

            provider.AddValue(providerValue);
        }

        await _providerRepository.UpdateAsync(provider, cancellationToken);

        return provider.ToDto<ProviderOutputDTO>();
    }
}
