using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;
using Recruitment.Infrastructure.Options;
using Recruitment.Infrastructure.Persistence;

namespace Recruitment.Infrastructure.Repositories;

public sealed class RctCriteriaRepository : IRctCriteriaRepository
{
    private readonly IRecruitmentSqlConnectionFactory _db;
    private readonly RecruitmentSchemaOptions _schema;

    public RctCriteriaRepository(
        IRecruitmentSqlConnectionFactory db,
        IOptions<RecruitmentSchemaOptions> schema)
    {
        _db = db;
        _schema = schema.Value;
    }

    public async Task<IReadOnlyList<RctCriterion>> GetUsableCriteriaAsync(
        string rctStamp,
        CancellationToken ct = default)
    {
        var sql = $"""
            SELECT codigo, rotulo, peso, hints
            FROM [{_schema.RctCriteriaTable}]
            WHERE [{_schema.RctStampColumn}] = @rct
              AND peso > 0
            ORDER BY ordem, codigo
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rct", rctStamp);

        var list = new List<RctCriterion>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var hintsRaw = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            var hints = hintsRaw.Split(
                ['|', ';', ','],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            list.Add(new RctCriterion
            {
                Code = reader.GetString(0),
                Label = reader.GetString(1),
                Weight = reader.GetDecimal(2),
                EvidenceHints = hints
            });
        }

        return list;
    }

    public async Task<int> CountUsableCriteriaAsync(string rctStamp, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT COUNT(1)
            FROM [{_schema.RctCriteriaTable}]
            WHERE [{_schema.RctStampColumn}] = @rct AND peso > 0
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rct", rctStamp);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }
}

public sealed class RctIntervenienteRepository : IRctIntervenienteRepository
{
    private readonly IRecruitmentSqlConnectionFactory _db;
    private readonly RecruitmentSchemaOptions _schema;

    public RctIntervenienteRepository(
        IRecruitmentSqlConnectionFactory db,
        IOptions<RecruitmentSchemaOptions> schema)
    {
        _db = db;
        _schema = schema.Value;
    }

    public async Task<IReadOnlyList<RctInterveniente>> GetIntervenientesAsync(
        string rctStamp,
        CancellationToken ct = default)
    {
        var sql = $"""
            SELECT CAST([{_schema.RctclbUserColumn}] AS nvarchar(50)),
                   [{_schema.RctclbNameColumn}]
            FROM [{_schema.RctclbTable}]
            WHERE [{_schema.RctStampColumn}] = @rct
              AND [{_schema.RctclbUserColumn}] IS NOT NULL
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rct", rctStamp);

        var list = new List<RctInterveniente>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(new RctInterveniente
            {
                UserId = reader.GetString(0),
                DisplayName = reader.IsDBNull(1) ? null : reader.GetString(1)
            });
        }

        return list;
    }

    public async Task<int> CountIntervenientesAsync(string rctStamp, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT COUNT(1)
            FROM [{_schema.RctclbTable}]
            WHERE [{_schema.RctStampColumn}] = @rct
              AND [{_schema.RctclbUserColumn}] IS NOT NULL
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@rct", rctStamp);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }
}
