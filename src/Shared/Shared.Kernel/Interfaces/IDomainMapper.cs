namespace Shared.Kernel.Interfaces;

/// <summary>
/// Interface para mappers específicos de domínio para DTO
/// Implementação simplificada focada em conversão Domain -> DTO
/// </summary>
/// <typeparam name="TEntity">Entidade de domínio</typeparam>
/// <typeparam name="TOutputDto">DTO de saída principal</typeparam>
public interface IDomainMapper<TEntity, TOutputDto>
    where TEntity : class
    where TOutputDto : class
{
    /// <summary>
    /// Converte uma entidade de domínio para DTO genérico
    /// </summary>
    TDto ToDto<TDto>(TEntity entity) where TDto : class;

    /// <summary>
    /// Converte uma coleção de entidades para DTOs genéricos
    /// </summary>
    IEnumerable<TDto> ToDtos<TDto>(IEnumerable<TEntity> entities) where TDto : class;

    /// <summary>
    /// Converte uma entidade para o DTO de saída principal
    /// </summary>
    TOutputDto ToOutputDto(TEntity entity);

    /// <summary>
    /// Converte uma coleção de entidades para DTOs de saída principais
    /// </summary>
    IEnumerable<TOutputDto> ToOutputDtos(IEnumerable<TEntity> entities);
}
