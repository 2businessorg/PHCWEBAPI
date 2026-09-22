using Dossiers.Domain.Entities;
using Dossiers.Domain.Repositories;
using Dossiers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Dossiers.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de dossiers
/// </summary>
public class DossierRepositoryEFCore : IDossierRepository
{
    private readonly DossiersDbContextEFCore _context;

    public DossierRepositoryEFCore(DossiersDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<DossierAggregate> AddAsync(DossierAggregate dossier, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var stamp = dossier.Bo.Bostamp;
            if (string.IsNullOrWhiteSpace(stamp))
            {
                throw new InvalidOperationException("Bostamp deve ser definido antes de persistir o dossier.");
            }

            if (!string.Equals(dossier.Bo2.Bo2stamp, stamp, StringComparison.Ordinal) ||
                !string.Equals(dossier.Bo3.Bo3stamp, stamp, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Bo2stamp e Bo3stamp devem ser iguais ao Bostamp do cabeçalho.");
            }

            if (dossier.Lines2.Count > 0)
            {
                if (dossier.Lines2.Any(line2 => !string.Equals(line2.Bostamp, stamp, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException("Todas as linhas BI2 devem ter o mesmo Bostamp do dossier.");
                }

                _context.Bi2.AddRange(dossier.Lines2);
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (dossier.Lines.Count > 0)
            {
                if (dossier.Lines.Any(line => !string.Equals(line.Bostamp, stamp, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException("Todas as linhas BI devem ter o mesmo Bostamp do dossier.");
                }

                _context.Bi.AddRange(dossier.Lines);
                await _context.SaveChangesAsync(cancellationToken);
            }

            _context.Bo3.Add(dossier.Bo3);
            await _context.SaveChangesAsync(cancellationToken);

            _context.Bo2.Add(dossier.Bo2);
            await _context.SaveChangesAsync(cancellationToken);

            _context.Bo.Add(dossier.Bo);
            await _context.SaveChangesAsync(cancellationToken);

            if (dossier.TaxTotals.Count > 0)
            {
                foreach (var total in dossier.TaxTotals)
                {
                    total.Bostamp = stamp;
                    if (string.IsNullOrWhiteSpace(total.Botstamp))
                    {
                        total.Botstamp = GenerateStamp();
                    }
                }

                _context.Bot.AddRange(dossier.TaxTotals);
                await _context.SaveChangesAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);

            return dossier;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<decimal> GetNextObranoAsync(decimal ndos, decimal boano, CancellationToken cancellationToken = default)
    {
        var max = await _context.Bo
            .Where(x => x.Ndos == ndos && x.Boano == boano)
            .MaxAsync(x => (decimal?)x.Obrano, cancellationToken) ?? 0;

        return max + 1;
    }

    public async Task<(int TotalItems, int CurrentPage, int PageSize, List<DossierAggregate> Items)> GetPagedAsync(
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
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query =
            from bo in _context.Bo.AsNoTracking()
            join bo2 in _context.Bo2.AsNoTracking() on bo.Bostamp equals bo2.Bo2stamp
            join bo3 in _context.Bo3.AsNoTracking() on bo.Bostamp equals bo3.Bo3stamp
            select new { bo, bo2, bo3 };

        if (ndos.HasValue)
        {
            query = query.Where(x => x.bo.Ndos == ndos.Value);
        }

        if (!string.IsNullOrWhiteSpace(nmdos))
        {
            var nmdosFilter = nmdos.Trim().ToLower();
            query = query.Where(x => (x.bo.Nmdos ?? string.Empty).ToLower().Contains(nmdosFilter));
        }

        if (obrano.HasValue)
        {
            query = query.Where(x => x.bo.Obrano == obrano.Value);
        }

        if (boano.HasValue)
        {
            query = query.Where(x => x.bo.Boano == boano.Value);
        }

        if (no.HasValue)
        {
            query = query.Where(x => x.bo.No == no.Value);
        }

        if (estab.HasValue)
        {
            query = query.Where(x => x.bo.Estab == estab.Value);
        }

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var nomeFilter = nome.Trim().ToLower();
            query = query.Where(x => (x.bo.Nome ?? string.Empty).ToLower().Contains(nomeFilter));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var headers = await query
            .OrderBy(x => x.bo.Ndos)
            .ThenBy(x => x.bo.Obrano)
            .ThenBy(x => x.bo.Boano)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var linesByBoStamp = new Dictionary<string, List<Bi>>();
        if (includeLines && headers.Count > 0)
        {
            var boStamps = headers.Select(h => h.bo.Bostamp).Distinct().ToList();

            var lines = await _context.Bi.AsNoTracking()
                .Where(x => boStamps.Contains(x.Bostamp))
                .OrderBy(x => x.Bostamp)
                .ThenBy(x => x.Bistamp)
                .ToListAsync(cancellationToken);

            linesByBoStamp = lines
                .GroupBy(x => x.Bostamp)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        var items = headers.Select(h => new DossierAggregate
        {
            Bo = h.bo,
            Bo2 = h.bo2,
            Bo3 = h.bo3,
            Lines = includeLines && linesByBoStamp.TryGetValue(h.bo.Bostamp, out var dossierLines)
                ? dossierLines
                : new List<Bi>()
        }).ToList();

        return (totalItems, page, pageSize, items);
    }

    public Task<bool> ExistsByKeyAsync(decimal ndos, decimal obrano, decimal boano, CancellationToken cancellationToken = default)
    {
        if (ndos <= 0 || obrano <= 0 || boano <= 0) return Task.FromResult(false);
        return _context.Bo.AnyAsync(x => x.Ndos == ndos && x.Obrano == obrano && x.Boano == boano, cancellationToken);
    }

    public async Task<DossierAggregate?> GetByKeyAsync(decimal ndos, decimal obrano, decimal boano, CancellationToken cancellationToken = default)
    {
        if (ndos <= 0) return null;

        var header = await (
            from bo in _context.Bo.AsNoTracking()
            join bo2 in _context.Bo2.AsNoTracking() on bo.Bostamp equals bo2.Bo2stamp
            join bo3 in _context.Bo3.AsNoTracking() on bo.Bostamp equals bo3.Bo3stamp
            where bo.Ndos == ndos && bo.Obrano == obrano && bo.Boano == boano
            select new { bo, bo2, bo3 }
        ).FirstOrDefaultAsync(cancellationToken);

        if (header is null) return null;

        var lines = await _context.Bi.AsNoTracking()
            .Where(x => x.Bostamp == header.bo.Bostamp)
            .OrderBy(x => x.Bistamp)
            .ToListAsync(cancellationToken);

        var lines2 = await _context.Bi2.AsNoTracking()
            .Where(x => x.Bostamp == header.bo.Bostamp)
            .OrderBy(x => x.Bi2stamp)
            .ToListAsync(cancellationToken);

        var taxTotals = await _context.Bot.AsNoTracking()
            .Where(x => x.Bostamp == header.bo.Bostamp)
            .ToListAsync(cancellationToken);

        var aggregate = new DossierAggregate
        {
            Bo = header.bo,
            Bo2 = header.bo2,
            Bo3 = header.bo3,
            Lines = lines,
            Lines2 = lines2,
            TaxTotals = taxTotals
        };
        return aggregate;
    }

    public async Task<bool> DeleteByKeyAsync(decimal ndos, decimal obrano, decimal boano, CancellationToken cancellationToken = default)
    {
        var dossier = await GetByKeyAsync(ndos, obrano, boano, cancellationToken);
        if (dossier is null) return false;

        return await DeleteByStampAsync(dossier.Bo.Bostamp, cancellationToken);
    }

    private async Task<bool> DeleteByStampAsync(string bostamp, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var bo = await _context.Bo.FirstOrDefaultAsync(x => x.Bostamp == bostamp, cancellationToken);
        var bo2 = await _context.Bo2.FirstOrDefaultAsync(x => x.Bo2stamp == bostamp, cancellationToken);
        var bo3 = await _context.Bo3.FirstOrDefaultAsync(x => x.Bo3stamp == bostamp, cancellationToken);

        if (bo is null || bo2 is null || bo3 is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var lines = await _context.Bi.Where(x => x.Bostamp == bostamp).ToListAsync(cancellationToken);

        try
        {
            if (lines.Count > 0)
            {
                _context.Bi.RemoveRange(lines);
            }

            _context.Bo3.Remove(bo3);
            _context.Bo2.Remove(bo2);
            _context.Bo.Remove(bo);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateStamp()
    {
        // PHC usa stamps de 25 chars; removemos hífens e truncamos
        return Guid.NewGuid().ToString("N")[..25];
    }

    public async Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string tableName,
        string stampColumn,
        string stamp,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var safeColumns = GetSafeColumns(columns);
        if (safeColumns.Count == 0 || string.IsNullOrWhiteSpace(stamp))
            return [];

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var existingColumns = await GetTableColumnsAsync(connection, tableName, cancellationToken);
            var validColumns = safeColumns.Where(existingColumns.Contains).ToList();
            if (validColumns.Count == 0)
                return [];

            var select = string.Join(", ", validColumns.Select(c => $"[{c}]"));
            var sql = $"SELECT {select} FROM [dbo].[{tableName}] WHERE [{stampColumn}] = @stamp";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@stamp", stamp);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return [];

            var result = new Dictionary<string, object?>(validColumns.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < validColumns.Count; i++)
                result[validColumns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);

            return result;
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    public async Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        string tableName,
        string stampColumn,
        IEnumerable<string> stamps,
        IReadOnlyList<string> columns,
        CancellationToken cancellationToken = default)
    {
        var safeColumns = GetSafeColumns(columns);
        var stampList = stamps
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (safeColumns.Count == 0 || stampList.Count == 0)
            return [];

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var existingColumns = await GetTableColumnsAsync(connection, tableName, cancellationToken);
            var validColumns = safeColumns.Where(existingColumns.Contains).ToList();
            if (validColumns.Count == 0)
                return [];

            var select = string.Join(", ", validColumns.Select(c => $"[{c}]"));
            var inParams = string.Join(", ", stampList.Select((_, i) => $"@p{i}"));
            var sql = $"SELECT [{stampColumn}], {select} FROM [dbo].[{tableName}] WHERE [{stampColumn}] IN ({inParams})";

            await using var command = new SqlCommand(sql, connection);
            for (var i = 0; i < stampList.Count; i++)
                command.Parameters.AddWithValue($"@p{i}", stampList[i]);

            var result = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var rowStamp = reader.GetString(0).Trim();
                var row = new Dictionary<string, object?>(validColumns.Count, StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < validColumns.Count; i++)
                    row[validColumns[i]] = reader.IsDBNull(i + 1) ? null : reader.GetValue(i + 1);

                result[rowStamp] = row;
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
        string tableName,
        string stampColumn,
        string stamp,
        Dictionary<string, object?> fields,
        CancellationToken cancellationToken = default)
    {
        if (fields == null || fields.Count == 0 || string.IsNullOrWhiteSpace(stamp))
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
            var existingColumns = await GetTableColumnsAsync(connection, tableName, cancellationToken);
            var validFields = safeFields
                .Where(f => existingColumns.Contains(f.Key))
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

    private static List<string> GetSafeColumns(IReadOnlyList<string> columns)
        => columns
            .Where(c => !string.IsNullOrWhiteSpace(c) && c.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

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
