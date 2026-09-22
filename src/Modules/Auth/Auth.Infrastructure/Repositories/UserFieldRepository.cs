using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserFieldRepository.
/// Reads from dbo.u_userfields in the primary (config) database.
/// </summary>
public class UserFieldRepository : IUserFieldRepository
{
    private readonly PrimaryDbContext _context;
    private readonly ILogger<UserFieldRepository> _logger;

    public UserFieldRepository(PrimaryDbContext context, ILogger<UserFieldRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserField>> GetActiveFieldsAsync(
        string appLicenseStamp,
        string moduleName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var stamp = appLicenseStamp.Trim();
        var module = moduleName.Trim();
        var table = tableName.Trim();

        var fields = await _context.UserFields
            .AsNoTracking()
            .Where(f =>
                f.AppLicenseStamp.Trim() == stamp &&
                f.ModuleName.Trim() == module &&
                f.TableName.Trim() == table &&
                !f.Inactive)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "UserField lookup: stamp={Stamp}, module={Module}, table={Table}, found={Count}",
            stamp, module, table, fields.Count);

        return fields;
    }
}
