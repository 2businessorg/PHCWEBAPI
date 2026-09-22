using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "User Name is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response DTO - returns only essential fields
/// Database credentials are resolved in background from JWT claims
/// </summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime? Expiration { get; set; }
    public bool Allowed { get; set; }
    public string OutputResponse { get; set; } = string.Empty;
}
