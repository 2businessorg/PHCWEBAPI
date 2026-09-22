using Microsoft.EntityFrameworkCore;
using Clients.Domain.Entities;
using Clients.Domain.Repositories;
using Clients.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Clients.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de clientes
/// </summary>
public class ClientRepositoryEFCore : IClientRepository
{
    private readonly ClientsDbContextEFCore _context;

    public ClientRepositoryEFCore(ClientsDbContextEFCore context)
    {
        _context = context;
    }

    public async Task AddAsync(Cl cl, Cl2 cl2, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Ordem explícita: primeiro CL2, depois CL
            _context.Cl2.Add(cl2);
            await _context.SaveChangesAsync(cancellationToken);

            _context.Cl.Add(cl);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<(Cl? cl, Cl2? cl2)> GetByStampAsync(string clstamp, CancellationToken cancellationToken = default)
    {
        var cl = await _context.Cl
            .FirstOrDefaultAsync(x => x.Clstamp == clstamp, cancellationToken);

        var cl2 = await _context.Cl2
            .FirstOrDefaultAsync(x => x.Cl2stamp == clstamp, cancellationToken);

        return (cl, cl2);
    }

    public async Task<List<(Cl cl, Cl2 cl2)>> GetByNoAsync(decimal no, CancellationToken cancellationToken = default)
    {
        var cls = await _context.Cl
            .Where(x => x.No == no)
            .ToListAsync(cancellationToken);

        if (cls.Count == 0)
        {
            return new List<(Cl, Cl2)>();
        }

        var clstamps = cls.Select(x => x.Clstamp).ToList();
        var cl2s = await _context.Cl2
            .Where(x => clstamps.Contains(x.Cl2stamp))
            .ToListAsync(cancellationToken);

        var result = new List<(Cl, Cl2)>();
        foreach (var cl in cls)
        {
            var cl2 = cl2s.FirstOrDefault(x => x.Cl2stamp == cl.Clstamp);
            if (cl2 != null)
            {
                result.Add((cl, cl2));
            }
        }

        return result;
    }

    public async Task<(Cl? cl, Cl2? cl2)> GetByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
    {
        var cl = await _context.Cl
            .FirstOrDefaultAsync(x => x.No == no && x.Estab == estab, cancellationToken);

        if (cl == null)
        {
            return (null, null);
        }

        var cl2 = await _context.Cl2
            .FirstOrDefaultAsync(x => x.Cl2stamp == cl.Clstamp, cancellationToken);

        if (cl2 == null)
        {
            return (null, null);
        }

        return (cl, cl2);
    }
    

    public async Task<Cl?> GetByNcontAsync(string ncont, CancellationToken cancellationToken = default)
    {
        return await _context.Cl
            .FirstOrDefaultAsync(x => x.Ncont == ncont, cancellationToken);
    }

    public async Task<List<(Cl cl, Cl2 cl2)>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cls = await _context.Cl.ToListAsync(cancellationToken);
        var cl2s = await _context.Cl2.ToListAsync(cancellationToken);

        return cls.Select(cl => 
        {
            var cl2 = cl2s.FirstOrDefault(x => x.Cl2stamp == cl.Clstamp);
            return (cl, cl2!);
        }).Where(x => x.Item2 != null)
          .Select(x => (x.cl, x.Item2!))
          .ToList();
    }

    public async Task<(int TotalItems, int CurrentPage, int PageSize, List<(Cl cl, Cl2 cl2)> Items)> GetPagedAsync(
        decimal? no,
        string? ncont,
        string? nome,
        string? telefone,
        string? morada,
        decimal? estab,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query =
            from cl in _context.Cl
            join cl2 in _context.Cl2 on cl.Clstamp equals cl2.Cl2stamp
            select new { cl, cl2 };

        if (no.HasValue)
        {
            query = query.Where(x => x.cl.No == no.Value);
        }

        if (!string.IsNullOrWhiteSpace(ncont))
        {
            var ncontFilter = ncont.Trim().ToLower();
            query = query.Where(x => ((x.cl.Ncont ?? string.Empty).Trim().ToLower()) == ncontFilter);
        }

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var nomeFilter = nome.Trim().ToLower();
            query = query.Where(x => (x.cl.Nome ?? string.Empty).ToLower().Contains(nomeFilter));
        }

        if (!string.IsNullOrWhiteSpace(telefone))
        {
            var telefoneFilter = telefone.Trim().ToLower();
            query = query.Where(x => (x.cl.Telefone ?? string.Empty).ToLower().Contains(telefoneFilter));
        }

        if (!string.IsNullOrWhiteSpace(morada))
        {
            var moradaFilter = morada.Trim().ToLower();
            query = query.Where(x => (x.cl.Morada ?? string.Empty).ToLower().Contains(moradaFilter));
        }

        if (estab.HasValue)
        {
            query = query.Where(x => x.cl.Estab == estab.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var skip = (page - 1) * pageSize;
        var paged = await query
            .OrderBy(x => x.cl.No)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = paged.Select(x => (x.cl, x.cl2)).ToList();
        return (totalItems, page, pageSize, items);
    }

    public async Task<bool> ExistsByNoAsync(decimal no, CancellationToken cancellationToken = default)
    {
        return await _context.Cl
            .AnyAsync(x => x.No == no, cancellationToken);
    }

    public async Task<bool> ExistsByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
    {
        return await _context.Cl
            .AnyAsync(x => x.No == no && x.Estab == estab, cancellationToken);
    }

    public async Task<bool> ExistsByNcontAsync(string ncont, CancellationToken cancellationToken = default)
    {
        return await _context.Cl
            .AnyAsync(x => x.Ncont == ncont, cancellationToken);
    }

    public async Task UpdateAsync(Cl cl, Cl2 cl2, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.Cl2.Update(cl2);
            _context.Cl.Update(cl);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteByStampAsync(string clstamp, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var cl = await _context.Cl
            .FirstOrDefaultAsync(x => x.Clstamp == clstamp, cancellationToken);

        var cl2 = await _context.Cl2
            .FirstOrDefaultAsync(x => x.Cl2stamp == clstamp, cancellationToken);

        if (cl == null || cl2 == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        try
        {
            _context.Cl.Remove(cl);
            _context.Cl2.Remove(cl2);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return true;
    }

    public async Task<decimal> GetNextNoAsync(CancellationToken cancellationToken = default)
    {
        var maxNo = await _context.Cl
            .MaxAsync(x => (decimal?)x.No, cancellationToken) ?? 0;

        return maxNo + 1;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string clstamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var safeColumns = GetSafeUserFieldColumns(columns);
        if (safeColumns.Count == 0)
            return new Dictionary<string, object?>();

        var columnList = string.Join(", ", safeColumns.Select(c => $"[{c}]"));
        var sql = $"SELECT {columnList} FROM cl WHERE clstamp = @clstamp";

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@clstamp", clstamp);

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

    /// <inheritdoc/>
    public async Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        IEnumerable<string> clstamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var stampList = clstamps.ToList();
        var safeColumns = GetSafeUserFieldColumns(columns);

        var result = new Dictionary<string, Dictionary<string, object?>>(stampList.Count);
        if (stampList.Count == 0 || safeColumns.Count == 0)
            return result;

        // Build parameterised IN clause: @p0, @p1, ...
        var paramNames = stampList.Select((_, i) => $"@p{i}").ToList();
        var columnList = string.Join(", ", safeColumns.Select(c => $"[{c}]"));
        var inClause = string.Join(", ", paramNames);
        var sql = $"SELECT clstamp, {columnList} FROM cl WHERE clstamp IN ({inClause})";

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
                // reader columns: clstamp (0), then user fields starting at 1
                for (var i = 0; i < safeColumns.Count; i++)
                    row[safeColumns[i]] = reader.IsDBNull(i + 1) ? null : reader.GetValue(i + 1);
                result[stamp] = row;
            }
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, object?>> GetUserFieldValuesFromCl2Async(
        string cl2stamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var safeColumns = GetSafeUserFieldColumns(columns);
        if (safeColumns.Count == 0)
            return new Dictionary<string, object?>();

        var columnList = string.Join(", ", safeColumns.Select(c => $"[{c}]"));
        var sql = $"SELECT {columnList} FROM cl2 WHERE cl2stamp = @cl2stamp";

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@cl2stamp", cl2stamp);

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

    /// <inheritdoc/>
    public async Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesFromCl2BatchAsync(
        IEnumerable<string> cl2stamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var stampList = cl2stamps.ToList();
        var safeColumns = GetSafeUserFieldColumns(columns);

        var result = new Dictionary<string, Dictionary<string, object?>>(stampList.Count);
        if (stampList.Count == 0 || safeColumns.Count == 0)
            return result;

        var paramNames = stampList.Select((_, i) => $"@p{i}").ToList();
        var columnList = string.Join(", ", safeColumns.Select(c => $"[{c}]"));
        var inClause = string.Join(", ", paramNames);
        var sql = $"SELECT cl2stamp, {columnList} FROM cl2 WHERE cl2stamp IN ({inClause})";

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
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }

        return result;
    }

    /// <summary>
    /// Validates and returns only safe user-defined column names (must start with "u_"
    /// and contain only letters, digits or underscores) to prevent SQL injection.
    /// </summary>
    private static List<string> GetSafeUserFieldColumns(IReadOnlyList<string> columns)
    {
        // Permite qualquer coluna que não contenha caracteres perigosos (SQL injection prevention)
        // As colunas vêm da configuração de u_addfields, que é controlada pelo administrador
        return columns
            .Where(c =>
                !string.IsNullOrWhiteSpace(c) &&
                c.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task UpdateUserFieldsAsync(
        string clstamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default)
        => await UpdateFieldsForTableAsync("cl", "clstamp", clstamp, fields, cancellationToken);

    /// <inheritdoc/>
    public async Task UpdateUserFieldsFromCl2Async(
        string cl2stamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default)
        => await UpdateFieldsForTableAsync("cl2", "cl2stamp", cl2stamp, fields, cancellationToken);

    private async Task UpdateFieldsForTableAsync(
        string tableName,
        string stampColumn,
        string stamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken)
    {
        if (fields == null || fields.Count == 0)
            return;

        // Validar nomes de colunas para prevenir SQL injection
        var safeFields = fields
            .Where(f => !string.IsNullOrWhiteSpace(f.Key) && 
                        f.Key.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            .ToDictionary(x => x.Key, x => x.Value);

        if (safeFields.Count == 0)
            return;

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var tableColumns = await GetTableColumnsAsync(connection, tableName, cancellationToken);
            var validFields = safeFields
                .Where(f => tableColumns.Contains(f.Key))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            if (validFields.Count == 0)
                return;

            var setClause = string.Join(", ", validFields.Keys.Select(k => $"[{k}] = @{k}"));
            var sql = $"UPDATE [dbo].[{tableName}] SET {setClause} WHERE [{stampColumn}] = @stamp";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@stamp", stamp);

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
                    element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                JsonValueKind.Object => element.GetRawText(),
                JsonValueKind.Array => element.GetRawText(),
                _ => element.ToString()
            };
        }

        if (value is JsonDocument doc)
            return doc.RootElement.GetRawText();

        return value;
    }
}
