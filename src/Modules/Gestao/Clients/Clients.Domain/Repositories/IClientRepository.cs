using Clients.Domain.Entities;

namespace Clients.Domain.Repositories;

/// <summary>
/// Interface do repositório para operações com clientes (Cl + Cl2)
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Adiciona um novo cliente (Cl + Cl2)
    /// </summary>
    Task AddAsync(Cl cl, Cl2 cl2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cliente por stamp (identificador único)
    /// </summary>
    Task<(Cl? cl, Cl2? cl2)> GetByStampAsync(string clstamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cliente por No (identificador canônico do servidor)
    /// Retorna todos os branches para este No
    /// </summary>
    Task<List<(Cl cl, Cl2 cl2)>> GetByNoAsync(decimal no, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cliente por No + Estab
    /// </summary>
    Task<(Cl? cl, Cl2? cl2)> GetByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cliente por Ncont (número de contribuinte)
    /// </summary>
    Task<Cl?> GetByNcontAsync(string ncont, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos os clientes
    /// </summary>
    Task<List<(Cl cl, Cl2 cl2)>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna clientes paginados com filtros e total de registros
    /// </summary>
    Task<(int TotalItems, int CurrentPage, int PageSize, List<(Cl cl, Cl2 cl2)> Items)> GetPagedAsync(
        decimal? no,
        string? ncont,
        string? nome,
        string? telefone,
        string? morada,
        decimal? estab,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se cliente existe pelo No
    /// </summary>
    Task<bool> ExistsByNoAsync(decimal no, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se cliente existe pela combinação No + Estab
    /// </summary>
    Task<bool> ExistsByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se cliente existe pelo Ncont
    /// </summary>
    Task<bool> ExistsByNcontAsync(string ncont, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    Task UpdateAsync(Cl cl, Cl2 cl2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta um cliente pelo stamp
    /// </summary>
    Task<bool> DeleteByStampAsync(string clstamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o próximo número (No) disponível para CL
    /// </summary>
    Task<decimal> GetNextNoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the values of the specified user-defined columns for a single client (by clstamp).
    /// Returns a dictionary keyed by column name.
    /// Column names are validated to prevent SQL injection (must start with "u_").
    /// </summary>
    Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string clstamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the values of the specified user-defined columns for multiple clients.
    /// Returns a dictionary keyed by clstamp, with a nested dictionary of column -> value.
    /// Avoids N+1 queries when rendering a paged list.
    /// </summary>
    Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        IEnumerable<string> clstamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the values of the specified user-defined columns from cl2
    /// for a single client (by cl2stamp).
    /// </summary>
    Task<Dictionary<string, object?>> GetUserFieldValuesFromCl2Async(
        string cl2stamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the values of the specified user-defined columns from cl2
    /// for multiple clients (by cl2stamp).
    /// </summary>
    Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesFromCl2BatchAsync(
        IEnumerable<string> cl2stamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza campos adicionais (user fields) em um cliente
    /// Os campos são atualizados diretamente na tabela cl ou cl2 conforme a coluna
    /// </summary>
    Task UpdateUserFieldsAsync(
        string clstamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza campos adicionais (user fields) na tabela cl2 para um cliente
    /// </summary>
    Task UpdateUserFieldsFromCl2Async(
        string cl2stamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default);
}
