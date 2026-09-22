using Stocks.Domain.DTOs;
using Stocks.Domain.Entities;

namespace Stocks.Domain.Repositories;

/// <summary>
/// Interface do repositório para operações com stocks.
/// </summary>
public interface IStockRepository
{
    Task AddAsync(St stock, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<St> stocks, CancellationToken cancellationToken = default);
    Task<St?> GetByRefAsync(string referencia, CancellationToken cancellationToken = default);
    Task<(int TotalItems, int CurrentPage, int PageSize, List<St> Items)> GetPagedAsync(
        string? referencia,
        string? descricao,
        string? familia,
        bool? inactivo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(int TotalItems, int CurrentPage, int PageSize, List<BatchDetailDTO> Items)> GetBatchesPagedAsync(
        string? referencia,
        string? lote,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<List<StockByWarehouseDTO>> GetBatchesByReferenceAsync(
        string referencia,
        string? lote,
        CancellationToken cancellationToken = default);
    Task<List<StockByWarehouseDetailsDTO>> GetStockByWarehouseAsync(
        string referencia,
        CancellationToken cancellationToken = default);
    Task<List<WarehouseDTO>> GetAllWarehousesAsync(
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByRefAsync(string referencia, CancellationToken cancellationToken = default);

    Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string ststamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        IEnumerable<string> ststamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default);

    Task UpdateUserFieldsAsync(
        string ststamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(St stock, CancellationToken cancellationToken = default);
    Task<bool> DeleteByRefAsync(string referencia, CancellationToken cancellationToken = default);
}
