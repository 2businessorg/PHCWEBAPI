using Mapster;
using Audit.Domain.Entities;
using Audit.Application.DTOs;
using Shared.Kernel.Interfaces;

namespace Audit.Application.Mappings;


public sealed class AuditLogMapper : IDomainMapper<AuditLog, AuditLogOutputDTO>
{

    public TDto ToDto<TDto>(AuditLog entity) where TDto : class
    {
        return entity.Adapt<TDto>();
    }


    public IEnumerable<TDto> ToDtos<TDto>(IEnumerable<AuditLog> entities) where TDto : class
    {
        return entities.Select(e => e.Adapt<TDto>());
    }


    public AuditLogOutputDTO ToOutputDto(AuditLog entity)
    {
        return entity.Adapt<AuditLogOutputDTO>();
    }

    
    public IEnumerable<AuditLogOutputDTO> ToOutputDtos(IEnumerable<AuditLog> entities)
    {
        return entities.Select(e => e.Adapt<AuditLogOutputDTO>());
    }
}


public static class AuditLogMappingExtensions
{
    private static readonly AuditLogMapper _mapper = new();

   
    public static TDto ToDto<TDto>(this AuditLog entity) where TDto : class
    {
        return _mapper.ToDto<TDto>(entity);
    }

    public static IEnumerable<TDto> ToDtos<TDto>(this IEnumerable<AuditLog> entities) where TDto : class
    {
        return _mapper.ToDtos<TDto>(entities);
    }

    public static AuditLogOutputDTO ToOutputDto(this AuditLog entity)
    {
        return _mapper.ToOutputDto(entity);
    }

    public static IEnumerable<AuditLogOutputDTO> ToOutputDtos(this IEnumerable<AuditLog> entities)
    {
        return _mapper.ToOutputDtos(entities);
    }
}
