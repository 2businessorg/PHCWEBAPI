using Shared.Kernel.Responses;

namespace Clients.Presentation.REST.Services;

/// <summary>
/// Builder para centralizar geração de links HATEOAS do módulo Clients
/// </summary>
public interface IClientsLinkBuilder
{
    /// <summary>
    /// Link para listar todos os clientes com paginação e filtros
    /// </summary>
    HATEOASLink GetAllLink(int page, int pageSize, string? filterSuffix = null);

    /// <summary>
    /// Link para próxima página
    /// </summary>
    HATEOASLink GetNextLink(int nextPage, int pageSize, string? filterSuffix = null);

    /// <summary>
    /// Link para última página
    /// </summary>
    HATEOASLink GetLastLink(int lastPage, int pageSize, string? filterSuffix = null);

    /// <summary>
    /// Link para criar novo cliente
    /// </summary>
    HATEOASLink GetCreateLink();

    /// <summary>
    /// Link para obter cliente específico (self)
    /// </summary>
    HATEOASLink GetDetailLink(decimal no, decimal estab = 0);

    /// <summary>
    /// Link para atualizar cliente
    /// </summary>
    HATEOASLink GetUpdateLink(decimal no, decimal estab = 0);

    /// <summary>
    /// Link para eliminar cliente
    /// </summary>
    HATEOASLink GetDeleteLink(decimal no, decimal estab = 0);

    /// <summary>
    /// Link para listar clientes (referência genérica)
    /// </summary>
    HATEOASLink GetListLink();

    /// <summary>
    /// Link SELF para busca paginada completa
    /// </summary>
    HATEOASLink GetSelfPaginationLink(int page, int pageSize, string? filterSuffix = null);
}
