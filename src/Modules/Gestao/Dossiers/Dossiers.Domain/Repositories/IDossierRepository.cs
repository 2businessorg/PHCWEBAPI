using Dossiers.Domain.Entities;

namespace Dossiers.Domain.Repositories;

/// <summary>
/// Contrato de persistência para dossiers
/// </summary>
public interface IDossierRepository
{
    Task<DossierAggregate> AddAsync(DossierAggregate dossier, CancellationToken cancellationToken = default);

    Task<decimal> GetNextObranoAsync(decimal ndos, decimal boano, CancellationToken cancellationToken = default);

    Task<(int TotalItems, int CurrentPage, int PageSize, List<DossierAggregate> Items)> GetPagedAsync(
        decimal? ndos,
        string? nmdos,
        decimal? obrano,
        decimal? boano,
        decimal? no,
        decimal? estab,
        string? nome,
        int page,
        int pageSize,
        bool includeLines = false,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByKeyAsync(decimal ndos, decimal obrano, decimal boano, CancellationToken cancellationToken = default);

    Task<DossierAggregate?> GetByKeyAsync(decimal ndos, decimal obrano, decimal boano, CancellationToken cancellationToken = default);

    Task<bool> DeleteByKeyAsync(decimal ndos, decimal obrano, decimal boano, CancellationToken cancellationToken = default);

    Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string tableName,
        string stampColumn,
        string stamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        string tableName,
        string stampColumn,
        IEnumerable<string> stamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    Task UpdateUserFieldsAsync(
        string tableName,
        string stampColumn,
        string stamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default);
}
