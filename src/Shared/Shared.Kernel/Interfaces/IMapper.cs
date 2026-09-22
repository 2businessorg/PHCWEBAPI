namespace Shared.Kernel.Interfaces;

/// <summary>
/// Interface genérica para mappers que convertem entre diferentes camadas
/// </summary>
/// <typeparam name="TDomain">Entidade de domínio (regras de negócio)</typeparam>
/// <typeparam name="TPersistence">Entidade de persistência (SQL, MongoDB, etc)</typeparam>
/// <typeparam name="TDto">Data Transfer Object (camada de aplicação/API)</typeparam>
public interface IMapper<TDomain, TPersistence, TDto>
{
    /// <summary>
    /// Converte documento de persistência para entidade de domínio
    /// </summary>
    TDomain ToDomain(TPersistence doc);

    /// <summary>
    /// Converte entidade de domínio para formato de persistência
    /// </summary>
    TPersistence ToPersistence(TDomain entity);

    /// <summary>
    /// Converte entidade de domínio para DTO
    /// </summary>
    TDto ToDto(TDomain entity);

    /// <summary>
    /// Converte documento de persistência diretamente para DTO
    /// </summary>
    TDto PersistenceToDto(TPersistence doc);
}
