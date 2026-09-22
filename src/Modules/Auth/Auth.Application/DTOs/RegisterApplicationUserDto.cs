using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs;

/// <summary>
/// Register Application User request DTO - for tenant registration with ApplicenseLicense
/// </summary>
public class RegisterApplicationUserRequestDto
{
    [Required(ErrorMessage = "User Name is required")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 256 characters")]
    public string Username { get; set; } = string.Empty;

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(256, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Appliance License Stamp is required")]
    [StringLength(100, ErrorMessage = "Appliance License Stamp cannot exceed 100 characters")]
    public string AppLicenseStamp { get; set; } = string.Empty;
}

/// <summary>
/// Register Application User response DTO
/// </summary>
public class RegisterApplicationUserResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}
