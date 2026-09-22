using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;
using Recruitment.Infrastructure.Options;
using Recruitment.Infrastructure.Persistence;

namespace Recruitment.Infrastructure.Repositories;

public sealed class SrtScoreRepository : ISrtScoreRepository
{
    private readonly IRecruitmentSqlConnectionFactory _db;
    private readonly RecruitmentSchemaOptions _schema;

    public SrtScoreRepository(
        IRecruitmentSqlConnectionFactory db,
        IOptions<RecruitmentSchemaOptions> schema)
    {
        _db = db;
        _schema = schema.Value;
    }

    public async Task<SrtCandidateRow?> GetByStampAsync(string srtStamp, CancellationToken ct = default)
    {
        var rows = await QueryAsync($"{_schema.SrtStampColumn} = @key", srtStamp, ct);
        return rows.FirstOrDefault();
    }

    public async Task<IReadOnlyList<SrtCandidateRow>> GetByRctAsync(
        string rctStamp,
        CancellationToken ct = default)
    {
        return await QueryAsync($"{_schema.SrtRctStampColumn} = @key", rctStamp, ct);
    }

    public async Task SaveScoreAsync(
        string srtStamp,
        decimal score,
        string justificationJson,
        string? modelo,
        string? promptVer,
        DateTime stampUtc,
        string? auditJson,
        CancellationToken ct = default)
    {
        // BR-02/03/04: UPDATE only IA columns â€” never condp / selection state.
        var sql = $"""
            UPDATE [{_schema.SrtTable}]
            SET [{_schema.SrtScoreIaColumn}] = @score,
                [{_schema.SrtJustIaColumn}] = @just,
                [{_schema.SrtModeloIaColumn}] = @modelo,
                [{_schema.SrtPromptVerIaColumn}] = @prompt,
                [{_schema.SrtStampIaColumn}] = @stamp,
                [{_schema.SrtAuditIaColumn}] = @audit
            WHERE [{_schema.SrtStampColumn}] = @srt
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@score", score);
        cmd.Parameters.AddWithValue("@just", justificationJson);
        cmd.Parameters.AddWithValue("@modelo", (object?)modelo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@prompt", (object?)promptVer ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stamp", stampUtc);
        cmd.Parameters.AddWithValue("@audit", (object?)auditJson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@srt", srtStamp);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task ClearScoreOnOcrErrorAsync(string srtStamp, CancellationToken ct = default)
    {
        var sql = $"""
            UPDATE [{_schema.SrtTable}]
            SET [{_schema.SrtScoreIaColumn}] = NULL,
                [{_schema.SrtJustIaColumn}] = NULL,
                [{_schema.SrtModeloIaColumn}] = NULL,
                [{_schema.SrtPromptVerIaColumn}] = NULL,
                [{_schema.SrtStampIaColumn}] = NULL,
                [{_schema.SrtAuditIaColumn}] = NULL
            WHERE [{_schema.SrtStampColumn}] = @srt
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@srt", srtStamp);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task<IReadOnlyList<SrtCandidateRow>> QueryAsync(
        string where,
        string key,
        CancellationToken ct)
    {
        var selectionCols = _schema.SrtSelectionStateColumns
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var snapshotExpr = selectionCols.Length == 0
            ? "NULL"
            : string.Join(" + '|' + ", selectionCols.Select(c => $"ISNULL(CAST(s.[{c}] AS nvarchar(100)),'')"));

        var sql = $"""
            SELECT
                s.[{_schema.SrtStampColumn}],
                s.[{_schema.SrtCveStampColumn}],
                s.[{_schema.SrtRctStampColumn}],
                c.[{_schema.CveNameColumn}],
                s.[{_schema.SrtScoreIaColumn}],
                s.[{_schema.SrtJustIaColumn}],
                c.[{_schema.CveEstadoIaColumn}],
                s.[{_schema.SrtCondpColumn}],
                s.[{_schema.SrtModeloIaColumn}],
                s.[{_schema.SrtPromptVerIaColumn}],
                s.[{_schema.SrtStampIaColumn}],
                {snapshotExpr} AS selection_snapshot
            FROM [{_schema.SrtTable}] s
            LEFT JOIN [{_schema.CveTable}] c
              ON c.[{_schema.CveStampColumn}] = s.[{_schema.SrtCveStampColumn}]
            WHERE s.{where}
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@key", key);

        var list = new List<SrtCandidateRow>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(new SrtCandidateRow
            {
                SrtStamp = reader.GetString(0),
                CveStamp = reader.GetString(1),
                RctStamp = reader.GetString(2),
                CandidateName = reader.IsDBNull(3) ? null : reader.GetString(3),
                ScoreIa = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                JustificationJson = reader.IsDBNull(5) ? null : reader.GetString(5),
                EstadoIa = reader.IsDBNull(6) ? null : reader.GetString(6),
                Condp = reader.IsDBNull(7) ? null : Convert.ToString(reader.GetValue(7)),
                ModeloIa = reader.IsDBNull(8) ? null : reader.GetString(8),
                PromptVerIa = reader.IsDBNull(9) ? null : reader.GetString(9),
                StampIa = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                SelectionStateSnapshot = reader.IsDBNull(11) ? null : reader.GetString(11)
            });
        }

        return list;
    }
}

public sealed class RecruitmentOutboxRepository : IRecruitmentOutboxRepository
{
    private readonly IRecruitmentSqlConnectionFactory _db;
    private readonly RecruitmentSchemaOptions _schema;

    public RecruitmentOutboxRepository(
        IRecruitmentSqlConnectionFactory db,
        IOptions<RecruitmentSchemaOptions> schema)
    {
        _db = db;
        _schema = schema.Value;
    }

    public async Task<long?> TryEnqueueAsync(RecruitmentOutboxItem item, CancellationToken ct = default)
    {
        var sql = $"""
            IF EXISTS (
                SELECT 1 FROM [{_schema.OutboxTable}]
                WHERE srtstamp = @srt AND estado IN ('pending','processing'))
                SELECT CAST(NULL AS bigint);
            ELSE
            BEGIN
                INSERT INTO [{_schema.OutboxTable}]
                    (cvestamp, rctstamp, srtstamp, anexostamp, estado, created_at_utc, lab_go_ref)
                VALUES (@cve, @rct, @srt, @anexo, @estado, @created, @lab);
                SELECT CAST(SCOPE_IDENTITY() AS bigint);
            END
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cve", item.CveStamp);
        cmd.Parameters.AddWithValue("@rct", item.RctStamp);
        cmd.Parameters.AddWithValue("@srt", item.SrtStamp);
        cmd.Parameters.AddWithValue("@anexo", item.AnexoStamp);
        cmd.Parameters.AddWithValue("@estado", item.Estado);
        cmd.Parameters.AddWithValue("@created", item.CreatedAtUtc);
        cmd.Parameters.AddWithValue("@lab", (object?)item.LabGoRef ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync(ct);
        if (result is null or DBNull)
            return null;
        return Convert.ToInt64(result);
    }

    public async Task<RecruitmentOutboxItem?> GetAsync(long id, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT id, cvestamp, rctstamp, srtstamp, anexostamp, estado,
                   created_at_utc, started_at_utc, heartbeat_at_utc, completed_at_utc,
                   error_msg, lab_go_ref
            FROM [{_schema.OutboxTable}]
            WHERE id = @id
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return Map(reader);
    }

    public Task MarkProcessingAsync(long id, CancellationToken ct = default) =>
        ExecAsync(
            $"UPDATE [{_schema.OutboxTable}] SET estado='processing', started_at_utc=SYSUTCDATETIME(), heartbeat_at_utc=SYSUTCDATETIME() WHERE id=@id",
            id, ct);

    public Task HeartbeatAsync(long id, CancellationToken ct = default) =>
        ExecAsync(
            $"UPDATE [{_schema.OutboxTable}] SET heartbeat_at_utc=SYSUTCDATETIME() WHERE id=@id",
            id, ct);

    public Task MarkDoneAsync(long id, CancellationToken ct = default) =>
        ExecAsync(
            $"UPDATE [{_schema.OutboxTable}] SET estado='done', completed_at_utc=SYSUTCDATETIME(), heartbeat_at_utc=SYSUTCDATETIME() WHERE id=@id",
            id, ct);

    public async Task MarkErrorAsync(long id, string errorMessage, CancellationToken ct = default)
    {
        var sql = $"""
            UPDATE [{_schema.OutboxTable}]
            SET estado='error', error_msg=@err, completed_at_utc=SYSUTCDATETIME(), heartbeat_at_utc=SYSUTCDATETIME()
            WHERE id=@id
            """;
        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@err", errorMessage.Length > 500 ? errorMessage[..500] : errorMessage);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<RecruitmentOutboxItem>> GetOrphansAsync(
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        var sql = $"""
            SELECT id, cvestamp, rctstamp, srtstamp, anexostamp, estado,
                   created_at_utc, started_at_utc, heartbeat_at_utc, completed_at_utc,
                   error_msg, lab_go_ref
            FROM [{_schema.OutboxTable}]
            WHERE estado IN ('pending','processing')
              AND ISNULL(heartbeat_at_utc, created_at_utc) < DATEADD(minute, -@ttlMinutes, SYSUTCDATETIME())
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ttlMinutes", Math.Max(1, (int)ttl.TotalMinutes));

        var list = new List<RecruitmentOutboxItem>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    private async Task ExecAsync(string sql, long id, CancellationToken ct)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RecruitmentOutboxItem Map(SqlDataReader reader) => new()
    {
        Id = reader.GetInt64(0),
        CveStamp = reader.GetString(1),
        RctStamp = reader.GetString(2),
        SrtStamp = reader.GetString(3),
        AnexoStamp = reader.GetString(4),
        Estado = reader.GetString(5),
        CreatedAtUtc = reader.GetDateTime(6),
        StartedAtUtc = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
        HeartbeatAtUtc = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
        CompletedAtUtc = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
        ErrorMessage = reader.IsDBNull(10) ? null : reader.GetString(10),
        LabGoRef = reader.IsDBNull(11) ? null : reader.GetString(11)
    };
}
