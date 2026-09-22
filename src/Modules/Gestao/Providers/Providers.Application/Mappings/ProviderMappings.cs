using Mapster;
using Providers.Domain.Entities;
using Shared.Kernel.Extensions;
using Shared.Kernel.Interfaces;
using Providers.Application.DTOs.Providers;

namespace Providers.Application.Mappings;

/// <summary>
/// Configuração do Mapster para Provider
/// </summary>
public static class ProviderMappingConfig
{
    public static void Configure()
    {
        // ✅ Provider → ProviderOutputDTO (incluindo Values)
        TypeAdapterConfig<Provider, ProviderOutputDTO>
            .NewConfig()
            .Map(dest => dest.Values, src => src.Values);

        // ✅ ProviderValue → ProviderValueItemDTO
        TypeAdapterConfig<ProviderValue, ProviderValueItemDTO>
            .NewConfig();
    }
}

public sealed class ProviderMapper : IDomainMapper<Provider, ProviderOutputDTO>
{
    public TDto ToDto<TDto>(Provider entity) where TDto : class
    {
        return entity.Adapt<TDto>();
    }

    public IEnumerable<TDto> ToDtos<TDto>(IEnumerable<Provider> entities) where TDto : class
    {
        return entities.Select(e => e.Adapt<TDto>());
    }

    public ProviderOutputDTO ToOutputDto(Provider entity)
    {
        return entity.Adapt<ProviderOutputDTO>();
    }

    public IEnumerable<ProviderOutputDTO> ToOutputDtos(IEnumerable<Provider> entities)
    {
        return entities.Select(e => e.Adapt<ProviderOutputDTO>());
    }
}

public static class ProviderMappingExtensions
{
    private static readonly ProviderMapper _mapper = new();

    public static Provider ToEntity(this CreateProviderInputDTO dto, string? createdBy)
    {
        return new Provider(
            25.GenerateStamp(),
            dto.Codigo,
            dto.Provedor,
            dto.Environment,
            dto.Descricao,
            dto.Ativo,
            createdBy
        );
    }

    public static void UpdateEntity(this Provider entity, UpdateProviderInputDTO dto, string? updatedBy)
    {
        entity.Update(
            dto.Codigo,
            dto.Provedor,
            dto.Environment,
            dto.Descricao,
            dto.Ativo,
            updatedBy
        );
    }

    public static TDto ToDto<TDto>(this Provider entity) where TDto : class
    {
        return _mapper.ToDto<TDto>(entity);
    }

    public static IEnumerable<TDto> ToDtos<TDto>(this IEnumerable<Provider> entities) where TDto : class
    {
        return _mapper.ToDtos<TDto>(entities);
    }

    public static ProviderOutputDTO ToOutputDto(this Provider entity)
    {
        return _mapper.ToOutputDto(entity);
    }

    public static IEnumerable<ProviderOutputDTO> ToOutputDtos(this IEnumerable<Provider> entities)
    {
        return _mapper.ToOutputDtos(entities);
    }
}
