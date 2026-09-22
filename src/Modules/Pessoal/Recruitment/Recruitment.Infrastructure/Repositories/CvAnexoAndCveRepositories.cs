using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;
using Recruitment.Infrastructure.Options;
using Recruitment.Infrastructure.Persistence;

namespace Recruitment.Infrastructure.Repositories;

public sealed class CvAnexoRepository : ICvAnexoRepository
{
    private readonly IRecruitmentSqlConnectionFactory _db;
    private readonly RecruitmentSchemaOptions _schema;

    public CvAnexoRepository(
        IRecruitmentSqlConnectionFactory db,
        IOptions<RecruitmentSchemaOptions> schema)
    {
        _db = db;
        _schema = schema.Value;
    }

    public async Task<CvAnexo?> GetCvAnexoAsync(string cveStamp, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT TOP 1
                [{_schema.AnexosStampColumn}],
                [{_schema.AnexosRecStampColumn}],
                [{_schema.AnexosBytesColumn}],
                [{_schema.AnexosFileNameColumn}],
                [{_schema.AnexosTextoColumn}]
            FROM [{_schema.AnexosTable}]
            WHERE [{_schema.AnexosOriTableColumn}] = @ori
              AND [{_schema.AnexosRecStampColumn}] = @cve
              AND [{_schema.AnexosTipoColumn}] = @tipo
            ORDER BY [{_schema.AnexosStampColumn}] DESC
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ori", _schema.AnexosOriTableCveValue);
        cmd.Parameters.AddWithValue("@cve", cveStamp);
        cmd.Parameters.AddWithValue("@tipo", _schema.AnexosCvTipoValue);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var bytes = reader.IsDBNull(2) ? Array.Empty<byte>() : (byte[])reader.GetValue(2);
        return new CvAnexo
        {
            AnexoStamp = reader.GetString(0),
            CveStamp = reader.GetString(1),
            Bytes = bytes,
            FileName = reader.IsDBNull(3) ? null : reader.GetString(3),
            Texto = reader.IsDBNull(4) ? null : reader.GetString(4)
        };
    }

    public async Task SaveTextoAsync(string anexoStamp, string texto, CancellationToken ct = default)
    {
        var sql = $"""
            UPDATE [{_schema.AnexosTable}]
            SET [{_schema.AnexosTextoColumn}] = @texto
            WHERE [{_schema.AnexosStampColumn}] = @stamp
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@texto", texto);
        cmd.Parameters.AddWithValue("@stamp", anexoStamp);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}

public sealed class CveEstadoIaRepository : ICveEstadoIaRepository
{
    private readonly IRecruitmentSqlConnectionFactory _db;
    private readonly RecruitmentSchemaOptions _schema;

    public CveEstadoIaRepository(
        IRecruitmentSqlConnectionFactory db,
        IOptions<RecruitmentSchemaOptions> schema)
    {
        _db = db;
        _schema = schema.Value;
    }

    public async Task SetEstadoIaAsync(string cveStamp, string estadoIa, CancellationToken ct = default)
    {
        var sql = $"""
            UPDATE [{_schema.CveTable}]
            SET [{_schema.CveEstadoIaColumn}] = @estado
            WHERE [{_schema.CveStampColumn}] = @stamp
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@estado", estadoIa);
        cmd.Parameters.AddWithValue("@stamp", cveStamp);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<string?> GetEstadoIaAsync(string cveStamp, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT [{_schema.CveEstadoIaColumn}]
            FROM [{_schema.CveTable}]
            WHERE [{_schema.CveStampColumn}] = @stamp
            """;

        await using var conn = _db.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@stamp", cveStamp);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is null or DBNull ? null : Convert.ToString(result);
    }
}
