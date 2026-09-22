namespace Auth.Domain.Interfaces;

/// <summary>
/// Repositório para validação de acesso a módulos.
///
/// A verificação passa por dois níveis:
/// 1. A AppLicense (u_applicensestamp) tem de ter uma linha "API PHC" activa em
///    dbo.u_applicl (acesso geral à API) - ver <see cref="IAppLicenseLineRepository"/>.
/// 2. Essa linha tem de ter o módulo pedido (nomepack) em dbo.u_apilic.
/// </summary>
public interface IAppLicenseModuleRepository
{
    Task<bool> HasAccessToModuleAsync(string u_applicensestamp, string nomepack, CancellationToken cancellationToken = default);
}
