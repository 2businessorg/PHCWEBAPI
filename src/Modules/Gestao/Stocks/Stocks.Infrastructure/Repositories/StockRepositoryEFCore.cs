using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Stocks.Domain.DTOs;
using Stocks.Domain.Entities;
using Stocks.Domain.Repositories;
using Stocks.Infrastructure.Persistence;
using System.Text.Json;

namespace Stocks.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de stocks.
/// </summary>
public class StockRepositoryEFCore : IStockRepository
{
    private readonly StocksDbContextEFCore _context;

    public StockRepositoryEFCore(StocksDbContextEFCore context)
    {
        _context = context;
    }

    public async Task AddAsync(St stock, CancellationToken cancellationToken = default)
    {
        _context.St.Add(stock);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<St> stocks, CancellationToken cancellationToken = default)
    {
        _context.St.AddRange(stocks);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<St?> GetByRefAsync(string referencia, CancellationToken cancellationToken = default)
    {
        return await _context.St
            .FirstOrDefaultAsync(x => x.Ref == referencia, cancellationToken);
    }

    public async Task<(int TotalItems, int CurrentPage, int PageSize, List<St> Items)> GetPagedAsync(
        string? referencia,
        string? descricao,
        string? familia,
        bool? inactivo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query = _context.St.AsQueryable();

        if (!string.IsNullOrWhiteSpace(referencia))
        {
            var refFilter = referencia.Trim().ToLower();
            query = query.Where(x => (x.Ref ?? string.Empty).Trim().ToLower() == refFilter);
        }

        if (!string.IsNullOrWhiteSpace(descricao))
        {
            var descFilter = descricao.Trim().ToLower();
            query = query.Where(x => (x.Design ?? string.Empty).ToLower().Contains(descFilter));
        }

        if (!string.IsNullOrWhiteSpace(familia))
        {
            var famFilter = familia.Trim().ToLower();
            query = query.Where(x => (x.Familia ?? string.Empty).Trim().ToLower() == famFilter);
        }

        if (inactivo.HasValue)
        {
            query = query.Where(x => x.Inactivo == inactivo.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var skip = (page - 1) * pageSize;
        var items = await query
            .OrderBy(x => x.Ref)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (totalItems, page, pageSize, items);
    }

    public async Task<(int TotalItems, int CurrentPage, int PageSize, List<BatchDetailDTO> Items)> GetBatchesPagedAsync(
        string? referencia,
        string? lote,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        // Query da tabela SE (Lotes)
        var query = from se in _context.Se
                    select new BatchDetailDTO
                    {
                        Lote = se.Lote,
                        Referencia = se.Ref,
                        Design = se.Design,
                        Forlote = se.Forlote,
                        Stock = se.Stock,
                        Qttacout = se.Qttacout,
                        Qttacin = se.Qttacin,
                        Uintr = se.Uintr,
                        Validade = se.Validade,
                        Datafact = se.Datafact,
                        Pcult = se.Pcult
                    };

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(referencia))
        {
            var refFilter = referencia.Trim().ToLower();
            query = query.Where(x => (x.Referencia ?? string.Empty).Trim().ToLower() == refFilter);
        }

        if (!string.IsNullOrWhiteSpace(lote))
        {
            var loteFilter = lote.Trim().ToLower();
            query = query.Where(x => (x.Lote ?? string.Empty).Trim().ToLower().Contains(loteFilter));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var skip = (page - 1) * pageSize;
        var items = await query
            .OrderBy(x => x.Referencia)
            .ThenBy(x => x.Lote)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (totalItems, page, pageSize, items);
    }

    public async Task<List<StockByWarehouseDTO>> GetBatchesByReferenceAsync(
        string referencia,
        string? lote,
        CancellationToken cancellationToken = default)
    {
        // Query da tabela SAL (stock por lote/armazém) com join em SZ (warehouses)
        var query = from sal in _context.Sal
                    join sz in _context.Sz on sal.Armazem equals sz.No
                    where sal.Ref == referencia
                    select new StockByWarehouseDTO
                    {
                        Lote = sal.Lote,
                        Referencia = sal.Ref,
                        Armazem = sal.Armazem,
                        NomeArmazem = sz.Nome,
                        Stock = sal.Stock,
                        Localizacao = string.Empty
                    };

        // Filtro por lote se fornecido
        if (!string.IsNullOrWhiteSpace(lote))
        {
            var loteFilter = lote.Trim().ToLower();
            query = query.Where(x => (x.Lote ?? string.Empty).Trim().ToLower() == loteFilter);
        }

        var items = await query
            .OrderBy(x => x.Lote)
            .ThenBy(x => x.Armazem)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<List<StockByWarehouseDetailsDTO>> GetStockByWarehouseAsync(
        string referencia,
        CancellationToken cancellationToken = default)
    {
        // Query da tabela SA (stock por armazém, agregado) com join em SZ (warehouses)
        var query = from sa in _context.Sa
                    join sz in _context.Sz on sa.Armazem equals sz.No
                    where sa.Ref == referencia
                    select new StockByWarehouseDetailsDTO
                    {
                        Armazem = sa.Armazem,
                        NomeArmazem = sz.Nome,
                        Stock = sa.Stock,
                        CustoStock = sa.Pcpond, // Preço de Custo Ponderado
                        Localizacao = sa.Local,
                        EncomendadoPorClientes = sa.Rescli, // Encomendado por Clientes
                        EncomendadoAFornecedores = sa.Resfor, // Encomendado a Fornecedores
                        StockMinimo = sa.Stmin, // Stock Mínimo
                        QuantidadeEmRecepcao = sa.Qttrec, // Quantidade em receção
                        QuantidadeCativada = sa.Rescat // Quantidade Cativada
                    };

        var items = await query
            .OrderBy(x => x.Armazem)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<bool> ExistsByRefAsync(string referencia, CancellationToken cancellationToken = default)
    {
        return await _context.St.AnyAsync(x => x.Ref == referencia, cancellationToken);
    }

    public async Task<List<WarehouseDTO>> GetAllWarehousesAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await _context.Sz
            .Select(sz => new WarehouseDTO
            {
                No = sz.No,
                NomeArmazem = sz.Nome
            })
            .OrderBy(x => x.No)
            .ToListAsync(cancellationToken);

        return warehouses;
    }

    public async Task UpdateAsync(St stock, CancellationToken cancellationToken = default)
    {
        _context.St.Update(stock);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string ststamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var safeColumns = GetSafeUserFieldColumns(columns);
        if (safeColumns.Count == 0)
            return [];

        var columnList = string.Join(", ", safeColumns.Select(c => $"[{c}]"));
        var sql = $"SELECT {columnList} FROM st WHERE ststamp = @ststamp";

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ststamp", ststamp);

            var result = new Dictionary<string, object?>(safeColumns.Count);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                for (var i = 0; i < reader.FieldCount; i++)
                    result[safeColumns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            return result;
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    public async Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        IEnumerable<string> ststamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var stampList = ststamps.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        var safeColumns = GetSafeUserFieldColumns(columns);

        var result = new Dictionary<string, Dictionary<string, object?>>(stampList.Count);
        if (stampList.Count == 0 || safeColumns.Count == 0)
            return result;

        var paramNames = stampList.Select((_, i) => $"@p{i}").ToList();
        var columnList = string.Join(", ", safeColumns.Select(c => $"[{c}]"));
        var inClause = string.Join(", ", paramNames);
        var sql = $"SELECT ststamp, {columnList} FROM st WHERE ststamp IN ({inClause})";

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = new SqlCommand(sql, connection);
            for (var i = 0; i < stampList.Count; i++)
                command.Parameters.AddWithValue(paramNames[i], stampList[i]);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var stamp = reader.GetString(0).Trim();
                var row = new Dictionary<string, object?>(safeColumns.Count);
                for (var i = 0; i < safeColumns.Count; i++)
                    row[safeColumns[i]] = reader.IsDBNull(i + 1) ? null : reader.GetValue(i + 1);

                result[stamp] = row;
            }

            return result;
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    public async Task UpdateUserFieldsAsync(
        string ststamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default)
    {
        if (fields is null || fields.Count == 0)
            return;

        var safeFields = fields
            .Where(f => !string.IsNullOrWhiteSpace(f.Key) && f.Key.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        if (safeFields.Count == 0)
            return;

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var tableColumns = await GetTableColumnsAsync(connection, "st", cancellationToken);
            var validFields = safeFields
                .Where(f => tableColumns.Contains(f.Key))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            if (validFields.Count == 0)
                return;

            var setClause = string.Join(", ", validFields.Keys.Select(k => $"[{k}] = @{k}"));
            var sql = $"UPDATE [dbo].[st] SET {setClause} WHERE [ststamp] = @ststamp";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ststamp", ststamp);

            foreach (var field in validFields)
            {
                var sqlValue = NormalizeSqlParameterValue(field.Value);
                command.Parameters.AddWithValue($"@{field.Key}", sqlValue ?? DBNull.Value);
            }

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    private static List<string> GetSafeUserFieldColumns(IReadOnlyList<string> columns)
    {
        return columns
            .Where(c =>
                !string.IsNullOrWhiteSpace(c) &&
                c.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<HashSet<string>> GetTableColumnsAsync(
        SqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        const string sql = "SELECT [name] FROM sys.columns WHERE object_id = OBJECT_ID(@objName)";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@objName", $"dbo.{tableName}");

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(reader.GetString(0));

        return result;
    }

    private static object? NormalizeSqlParameterValue(object? value)
    {
        if (value is null)
            return null;

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number =>
                    element.TryGetInt64(out var l) ? l :
                    element.TryGetDecimal(out var d) ? d :
                    element.TryGetDouble(out var dbl) ? dbl :
                    element.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        return value;
    }

    public async Task<bool> DeleteByRefAsync(string referencia, CancellationToken cancellationToken = default)
    {
        var stock = await _context.St.FirstOrDefaultAsync(x => x.Ref == referencia, cancellationToken);
        if (stock is null) return false;

        _context.St.Remove(stock);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
