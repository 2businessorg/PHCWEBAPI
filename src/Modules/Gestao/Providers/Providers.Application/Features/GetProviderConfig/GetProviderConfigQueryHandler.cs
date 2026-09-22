using MediatR;
using Providers.Application.DTOs.Providers;
using Providers.Domain.Repositories;
using Providers.Domain.Services;

namespace Providers.Application.Features.GetProviderConfig;

/// <summary>
/// ⭐ HANDLER PRINCIPAL - Retorna config do endpoint com valores desencriptados
/// </summary>
public class GetProviderConfigQueryHandler : IRequestHandler<GetProviderConfigQuery, ProviderConfigDTO?>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IProviderValueRepository _providerValueRepository;
    private readonly IEncryptionService _encryptionService;

    public GetProviderConfigQueryHandler(
        IProviderRepository providerRepository,
        IProviderValueRepository providerValueRepository,
        IEncryptionService encryptionService)
    {
        _providerRepository = providerRepository;
        _providerValueRepository = providerValueRepository;
        _encryptionService = encryptionService;
    }

    public async Task<ProviderConfigDTO?> Handle(GetProviderConfigQuery request, CancellationToken cancellationToken)
    {
        // Buscar provider por nome e environment
        var provider = await _providerRepository.GetByProviderAndEnvironmentAsync(
            request.Provedor, 
            request.Environment, 
            cancellationToken);

        if (provider == null || !provider.Ativo)
            return null;

        // Buscar valores para a operação específica
        var values = await _providerValueRepository.GetByProviderAndOperationAsync(
            provider.UProviderStamp,
            request.OperationCode,
            includeInactive: false,
            cancellationToken);

        if (!values.Any())
            return null;

        // Montar dictionary com valores desencriptados
        var properties = new Dictionary<string, string>();
        
        foreach (var value in values.OrderBy(v => v.Ordem))
        {
            var valorFinal = value.Encriptado 
                ? _encryptionService.Decrypt(value.Valor) 
                : value.Valor;
            
            properties[value.Chave] = valorFinal;
        }

        return new ProviderConfigDTO
        {
            Provedor = provider.Provedor,
            Environment = provider.Environment,
            OperationCode = request.OperationCode,
            Properties = properties
        };
    }
}
