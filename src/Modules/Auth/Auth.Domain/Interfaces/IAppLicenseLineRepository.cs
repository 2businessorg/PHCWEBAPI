using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

/// <summary>
/// Repositório para as linhas de licenciamento (dbo.u_applicl) de uma AppLicense.
/// </summary>
public interface IAppLicenseLineRepository
{
    /// <summary>
    /// Obtém a linha "API PHC" activa (se existir) associada a esta AppLicense.
    /// A existência desta linha é o que concede acesso geral à API.
    /// </summary>
    Task<AppLicenseLine?> GetApiAccessLineAsync(string appLicenseStamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica se a AppLicense tem acesso geral à API (existe linha "API PHC" não inactiva).
    /// </summary>
    Task<bool> HasApiAccessAsync(string appLicenseStamp, CancellationToken cancellationToken = default);
}
