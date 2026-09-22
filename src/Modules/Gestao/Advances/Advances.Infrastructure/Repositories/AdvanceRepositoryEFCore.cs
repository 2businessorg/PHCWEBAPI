using Advances.Domain.Entities;
using Advances.Domain.Repositories;
using Advances.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Advances.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Adiantamentos
/// </summary>
public class AdvanceRepositoryEFCore : IAdvanceRepository
{
    private readonly AdvancesDbContext _db;

    public AdvanceRepositoryEFCore(AdvancesDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<Rd?> GetByKeyAsync(decimal ndoc, decimal rno, decimal rdano, CancellationToken ct = default)
    {
        return await _db.Rd
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Ndoc == ndoc && r.Rno == rno && r.Rdano == rdano, ct);
    }

    /// <inheritdoc />
    public async Task<(int TotalItems, IReadOnlyList<Rd> Items)> GetAllAsync(
        decimal? ndoc, decimal? rno, decimal? rdano, decimal? no,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Rd.AsNoTracking();

        if (ndoc.HasValue)  query = query.Where(r => r.Ndoc == ndoc.Value);
        if (rno.HasValue)   query = query.Where(r => r.Rno == rno.Value);
        if (rdano.HasValue) query = query.Where(r => r.Rdano == rdano.Value);
        if (no.HasValue)    query = query.Where(r => r.No == no.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.Rdano)
            .ThenByDescending(r => r.Rno)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (total, items);
    }

    /// <inheritdoc />
    public async Task<string> GetBankAccountNameAsync(decimal noconta, CancellationToken ct = default)
    {
        var bl = await _db.Bl
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Noconta == noconta, ct);

        return bl is null ? string.Empty : $"{bl.Banco}|{bl.Conta}".Trim();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByKeyAsync(decimal ndoc, decimal rno, decimal rdano, CancellationToken ct = default)
    {
        return await _db.Rd
            .AnyAsync(r => r.Ndoc == ndoc && r.Rno == rno && r.Rdano == rdano, ct);
    }

    // -------------------------------------------------------------------------
    // User fields — tabela rd
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string rdstamp,
        IReadOnlyList<string> columns,
        CancellationToken ct = default)
    {
        var safe = GetSafeColumns(columns);
        if (safe.Count == 0) return [];

        var colList = string.Join(", ", safe.Select(c => $"[{c}]"));
        var sql = $"SELECT {colList} FROM rd WHERE rdstamp = @rdstamp";

        var conn = (SqlConnection)_db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@rdstamp", rdstamp);
            var result = new Dictionary<string, object?>(safe.Count);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                for (var i = 0; i < reader.FieldCount; i++)
                    result[safe[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            return result;
        }
        finally { if (!wasOpen) await conn.CloseAsync(); }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesBatchAsync(
        IEnumerable<string> rdstamps,
        IReadOnlyList<string> columns,
        CancellationToken ct = default)
    {
        var stampList = rdstamps.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        var safe = GetSafeColumns(columns);
        var result = new Dictionary<string, Dictionary<string, object?>>(stampList.Count);
        if (stampList.Count == 0 || safe.Count == 0) return result;

        var paramNames = stampList.Select((_, i) => $"@p{i}").ToList();
        var colList = string.Join(", ", safe.Select(c => $"[{c}]"));
        var sql = $"SELECT rdstamp, {colList} FROM rd WHERE rdstamp IN ({string.Join(", ", paramNames)})";

        var conn = (SqlConnection)_db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(sql, conn);
            for (var i = 0; i < stampList.Count; i++)
                cmd.Parameters.AddWithValue(paramNames[i], stampList[i]);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var stamp = reader.GetString(0).Trim();
                var row = new Dictionary<string, object?>(safe.Count);
                for (var i = 0; i < safe.Count; i++)
                    row[safe[i]] = reader.IsDBNull(i + 1) ? null : reader.GetValue(i + 1);
                result[stamp] = row;
            }
            return result;
        }
        finally { if (!wasOpen) await conn.CloseAsync(); }
    }

    private static List<string> GetSafeColumns(IReadOnlyList<string> columns) =>
        columns
            .Where(c => !string.IsNullOrWhiteSpace(c) && c.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
