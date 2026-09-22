using MediatR;
using Providers.Application.DTOs.Providers;

namespace Providers.Application.Features.GetProviderConfig;

/// <summary>
/// ⭐ QUERY PRINCIPAL - Obtém configuração completa de um endpoint
/// </summary>
public record GetProviderConfigQuery(
    string Provedor,
    string OperationCode,
    string Environment
) : IRequest<ProviderConfigDTO?>;
