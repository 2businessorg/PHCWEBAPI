using Mapster;
using Parameters.Domain.Entities;
using Shared.Kernel.Extensions;
using Shared.Kernel.Interfaces;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Mappings;


public sealed class Para1Mapper : IDomainMapper<Para1, ParameterOutputDTO>
{
  
    public TDto ToDto<TDto>(Para1 entity) where TDto : class
    {
        return entity.Adapt<TDto>();
    }

   
    public IEnumerable<TDto> ToDtos<TDto>(IEnumerable<Para1> entities) where TDto : class
    {
        return entities.Select(e => e.Adapt<TDto>());
    }

 
    public ParameterOutputDTO ToOutputDto(Para1 entity)
    {
        return entity.Adapt<ParameterOutputDTO>();
    }


    public IEnumerable<ParameterOutputDTO> ToOutputDtos(IEnumerable<Para1> entities)
    {
        return entities.Select(e => e.Adapt<ParameterOutputDTO>());
    }
}


public static class Para1MappingExtensions
{
    private static readonly Para1Mapper _mapper = new();

 
    public static Para1 ToEntity(this CreateParameterInputDTO dto, string? createdBy)
    {
        return new Para1(
            25.GenerateStamp(),
            dto.Descricao,
            dto.Valor,
            dto.Tipo,
            dto.Dec,
            dto.Tam,
            createdBy
        );
    }


    public static void UpdateEntity(this Para1 entity, UpdateParameterInputDTO dto, string? updatedBy)
    {
        entity.Update(
            dto.Descricao,
            dto.Valor,
            dto.Tipo,
            dto.Dec,
            dto.Tam,
            updatedBy
        );
    }


    public static TDto ToDto<TDto>(this Para1 entity) where TDto : class
    {
        return _mapper.ToDto<TDto>(entity);
    }

    public static IEnumerable<TDto> ToDtos<TDto>(this IEnumerable<Para1> entities) where TDto : class
    {
        return _mapper.ToDtos<TDto>(entities);
    }

    public static ParameterOutputDTO ToOutputDto(this Para1 entity)
    {
        return _mapper.ToOutputDto(entity);
    }

    public static IEnumerable<ParameterOutputDTO> ToOutputDtos(this IEnumerable<Para1> entities)
    {
        return _mapper.ToOutputDtos(entities);
    }
}
