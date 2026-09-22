using Advances.Application.DTOs;

namespace Advances.Application.Mappings;

/// <summary>
/// Constrói o payload enviado ao PHC WEB para criação de adiantamento (script insertRdAPI).
/// </summary>
public static class AdvancePhcMapper
{
    /// <summary>
    /// Converte o DTO de input num objecto anónimo para o script insertRdAPI do PHC WEB.
    /// </summary>
    public static object ToPhcPayload(
        CreateAdvanceInputDTO dto,
        string? createdBy = null,
        Dictionary<string, object?>? resolvedAddFields = null)
    {
        return new
        {
            docTypeId = dto.Ndoc,
            clientId = dto.No,
            bankAccountId = dto.Contado,
            amount = dto.Amount,
            date = dto.Date,
            descricao = dto.Descricao,
            createdBy = createdBy ?? "PHCAPI",
            addFields = resolvedAddFields ?? dto.AddFields
        };
    }
}
