using MediatR;
using Providers.Application.DTOs.Providers;
using Providers.Application.Mappings;
using Providers.Domain.Entities;
using Providers.Domain.Repositories;
using Providers.Domain.Services;

namespace Providers.Application.Features.CreateProvider;

public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, ProviderOutputDTO>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IEncryptionService _encryptionService;

    public CreateProviderCommandHandler(
        IProviderRepository providerRepository,
        IEncryptionService encryptionService)
    {
        _providerRepository = providerRepository;
        _encryptionService = encryptionService;
    }

    public async Task<ProviderOutputDTO> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe
        var exists = await _providerRepository.ExistsProviderEnvironmentAsync(
            request.Dto.Provedor, 
            request.Dto.Environment, 
            cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Provider '{request.Dto.Provedor}' already exists for environment '{request.Dto.Environment}'");

        // Criar o cabeçalho
        var provider = request.Dto.ToEntity(request.CriadoPor);

        // Adicionar as linhas (values)
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
                criadoPor: request.CriadoPor
            );

            provider.AddValue(providerValue);
        }

        // Salvar tudo junto (EF Core vai fazer o cascade)
        var savedProvider = await _providerRepository.AddAsync(provider, cancellationToken);

        return savedProvider.ToDto<ProviderOutputDTO>();
    }
}
