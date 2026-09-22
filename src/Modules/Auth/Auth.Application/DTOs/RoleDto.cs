using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs;

/// <summary>
/// Add role to user request
/// </summary>
public class AddRoleToUserRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Create role request
/// </summary>
public class CreateRoleRequestDto
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>
/// Role response DTO
/// </summary>
public class RoleResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
