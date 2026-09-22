namespace Dossiers.Application.DTOs;

/// <summary>
/// DTO de entrada para criação em lote de dossiers
/// </summary>
public class CreateDossiersBulkInputDTO
{
    public List<CreateDossierInputDTO> Items { get; set; } = new();
}
