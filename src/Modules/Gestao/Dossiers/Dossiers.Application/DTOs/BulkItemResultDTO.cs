namespace Dossiers.Application.DTOs;

// Import do DTO base compartilhado
using Shared.Kernel.Responses;

/// <summary>
/// Alias para o DTO base de resultado de item bulk (para compatibilidade local)
/// </summary>
public class BulkItemResultDTO : Shared.Kernel.Responses.BulkItemResultDTO
{
}

/// <summary>
/// Alias para o DTO base de erro bulk (para compatibilidade local)
/// </summary>
public class BulkItemErrorDTO : BulkErrorDTO
{
}
