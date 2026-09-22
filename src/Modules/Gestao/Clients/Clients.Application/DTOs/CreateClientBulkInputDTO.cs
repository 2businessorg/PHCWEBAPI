namespace Clients.Application.DTOs;

/// <summary>
/// DTO de entrada para criar múltiplos clientes em bulk
/// </summary>
public class CreateClientBulkInputDTO
{
    /// <summary>
    /// Lista de clientes a criar (máximo 100 itens)
    /// </summary>
    public List<CreateClientInputDTO>? Items { get; set; }
}
