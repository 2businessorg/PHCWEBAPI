using Dossiers.Domain.Models;

namespace Dossiers.Domain.ExternalServices;

/// <summary>
/// Dossiers-specific PHC Web service interface
/// Implements methods for dossier-specific operations like InsertBo
/// </summary>
public interface IPhcWebServiceDossiers
{
    /// <summary>
    /// Executes the insertBo script on PHC WEB for business object insertion
    /// </summary>
    /// <param name="request">The PHC insert business object request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from PHC Web</returns>
    Task<PhcInsertBoResponse> InsertBoAsync(PhcInsertBoRequest request, CancellationToken cancellationToken = default);
}
