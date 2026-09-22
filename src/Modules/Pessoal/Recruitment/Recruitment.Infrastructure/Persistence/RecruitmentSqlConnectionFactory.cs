using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Recruitment.Infrastructure.Options;

namespace Recruitment.Infrastructure.Persistence;

public interface IRecruitmentSqlConnectionFactory
{
    SqlConnection Create();
}

public sealed class RecruitmentSqlConnectionFactory : IRecruitmentSqlConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly RecruitmentSchemaOptions _schema;

    public RecruitmentSqlConnectionFactory(
        IConfiguration configuration,
        IOptions<RecruitmentSchemaOptions> schema)
    {
        _configuration = configuration;
        _schema = schema.Value;
    }

    public SqlConnection Create()
    {
        var cs = _configuration.GetConnectionString(_schema.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{_schema.ConnectionStringName}' em falta.");
        return new SqlConnection(cs);
    }
}
