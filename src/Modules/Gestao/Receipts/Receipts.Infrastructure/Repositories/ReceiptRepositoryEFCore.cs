using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Receipts.Domain.Entities;
using Receipts.Domain.Repositories;
using Receipts.Infrastructure.Persistence;

namespace Receipts.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Recibos
/// </summary>
public class ReceiptRepositoryEFCore : IReceiptRepository
{
    private readonly ReceiptsDbContext _db;

    public ReceiptRepositoryEFCore(ReceiptsDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<ReceiptAggregate?> GetByKeyAsync(decimal ndoc, decimal rno, decimal reano, CancellationToken ct = default)
    {
        var header = await _db.Re
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Ndoc == ndoc && r.Rno == rno && r.Reano == reano, ct);

        if (header is null)
            return null;

        var lines = await _db.Rl
            .AsNoTracking()
            .Where(l => l.Restamp == header.Restamp)
            .ToListAsync(ct);

        return new ReceiptAggregate { Header = header, Lines = lines };
    }

    /// <inheritdoc />
    public async Task<(int TotalItems, IReadOnlyList<Re> Items)> GetAllAsync(
        decimal? ndoc, decimal? rno, decimal? reano, decimal? no,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Re.AsNoTracking();

        if (ndoc.HasValue)   query = query.Where(r => r.Ndoc == ndoc.Value);
        if (rno.HasValue)    query = query.Where(r => r.Rno == rno.Value);
        if (reano.HasValue)  query = query.Where(r => r.Reano == reano.Value);
        if (no.HasValue)     query = query.Where(r => r.No == no.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.Reano)
            .ThenByDescending(r => r.Rno)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (total, items);
    }

    /// <inheritdoc />
    public async Task<List<Rl>> GetLinesByRestampAsync(string restamp, CancellationToken ct = default)
    {
        return await _db.Rl
            .AsNoTracking()
            .Where(l => l.Restamp == restamp)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Obtém o nome da conta bancária para uma chave de conta
    /// </summary>
    public async Task<string> GetBankAccountNameAsync(decimal noconta, CancellationToken ct = default)
    {
        var bl = await _db.Bl
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Noconta == noconta, ct);

        if (bl is null)
            return string.Empty;

        return $"{bl.Banco}|{bl.Conta}".Trim();
    }

    /// <summary>
    /// Obtém o ID da série de facturação (ftstamp) para uma chave de série de recibo
    /// </summary>
    public async Task<string> GetInvoiceSeriesIdAsync(string ccstamp, CancellationToken ct = default)
    {
        var ft = await _db.Ft
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Ftstamp == ccstamp, ct);

        return ft?.Ndoc.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByKeyAsync(decimal ndoc, decimal rno, decimal reano, CancellationToken ct = default)
    {
        return await _db.Re
            .AnyAsync(r => r.Ndoc == ndoc && r.Rno == rno && r.Reano == reano, ct);
    }

    // -------------------------------------------------------------------------
    // User fields — tabela re (cabeçalho)
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
        string restamp,
        IReadOnlyList<string> columns,
        CancellationToken ct = default)
    {
        var safe = GetSafeColumns(columns);
        if (safe.Count == 0) return [];

        var colList = string.Join(", ", safe.Select(c => $"[{c}]"));
        var sql = $"SELECT {colList} FROM re WHERE restamp = @restamp";

        var conn = (SqlConnection)_db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@restamp", restamp);
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
        IEnumerable<string> restamps,
        IReadOnlyList<string> columns,
        CancellationToken ct = default)
    {
        var stampList = restamps.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        var safe = GetSafeColumns(columns);
        var result = new Dictionary<string, Dictionary<string, object?>>(stampList.Count);
        if (stampList.Count == 0 || safe.Count == 0) return result;

        var paramNames = stampList.Select((_, i) => $"@p{i}").ToList();
        var colList = string.Join(", ", safe.Select(c => $"[{c}]"));
        var sql = $"SELECT restamp, {colList} FROM re WHERE restamp IN ({string.Join(", ", paramNames)})";

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

    // -------------------------------------------------------------------------
    // User fields — tabela rl (linhas)
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Dictionary<string, object?>> GetUserFieldValuesRlAsync(
        string rlstamp,
        IReadOnlyList<string> columns,
        CancellationToken ct = default)
    {
        var safe = GetSafeColumns(columns);
        if (safe.Count == 0) return [];

        var colList = string.Join(", ", safe.Select(c => $"[{c}]"));
        var sql = $"SELECT {colList} FROM rl WHERE rlstamp = @rlstamp";

        var conn = (SqlConnection)_db.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@rlstamp", rlstamp);
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
    public async Task<Dictionary<string, Dictionary<string, object?>>> GetUserFieldValuesRlBatchAsync(
        IEnumerable<string> rlstamps,
        IReadOnlyList<string> columns,
        CancellationToken ct = default)
    {
        var stampList = rlstamps.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        var safe = GetSafeColumns(columns);
        var result = new Dictionary<string, Dictionary<string, object?>>(stampList.Count);
        if (stampList.Count == 0 || safe.Count == 0) return result;

        var paramNames = stampList.Select((_, i) => $"@p{i}").ToList();
        var colList = string.Join(", ", safe.Select(c => $"[{c}]"));
        var sql = $"SELECT rlstamp, {colList} FROM rl WHERE rlstamp IN ({string.Join(", ", paramNames)})";

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

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static List<string> GetSafeColumns(IReadOnlyList<string> columns) =>
        columns
            .Where(c => !string.IsNullOrWhiteSpace(c) && c.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
