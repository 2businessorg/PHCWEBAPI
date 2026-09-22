using Auth.Application.DTOs;
using Auth.Application.Features.Login;
using Auth.Application.Features.Register;
using Auth.Application.Features.RegisterApplicationUser;
using Auth.Application.Features.CreateRole;
using Auth.Application.Features.AddUserToRole;
using Auth.Application.Features.GetAllRoles;
using Auth.Application.Features.GetUserRoles;
using Shared.Kernel.Authorization;
using Shared.Kernel.Responses;
using Shared.Kernel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;

namespace Auth.Presentation.Controllers;

/// <summary>
/// Authentication controller
/// Provides endpoints for login, registration, and user management
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public AuthenticateController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    /// <summary>
    /// Register a new application user linked to an existing AppLicense (tenant).
    /// </summary>
    [HttpPost]
    [Route("registerAppUser")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("login-endpoint")] // 🔴 CRITICAL: 3 attempts/min (anti-brute force)
    public async Task<IActionResult> RegisterApplicationUser(
        [FromBody] RegisterApplicationUserRequestDto model,
        CancellationToken ct = default)
    {
        var correlationId = HttpContext.GetCorrelationId();

        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0002", "Invalid request - Model validation failed", correlationId),
                data: new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
            ));
        }

        var command = new RegisterApplicationUserCommand(model.Username, model.Email, model.Password, model.AppLicenseStamp);
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0015", "Validation error", correlationId),
                data: result
            ));
        }

        return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login-endpoint")] // 🔴 CRITICAL: 3 attempts/min (anti-brute force)
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var command = new LoginCommand(model.Username, model.Password);
        var result = await _mediator.Send(command, ct);

        return Ok(result);
    }

    /// <summary>
    /// Bootstrap endpoint to create the first administrator user.
    /// Only available in Development environment.
    /// </summary>
    [HttpPost]
    [Route("bootstrap-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> BootstrapAdmin(
        [FromBody] RegisterRequestDto model,
        CancellationToken ct = default)
    {
        var correlationId = HttpContext.GetCorrelationId();

        if (!string.Equals(_environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0002", "Invalid request - Model validation failed", correlationId),
                data: new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
            ));
        }

        var registerResult = await _mediator.Send(
            new RegisterCommand(model.Username, model.Email, model.Password),
            ct);

        if (!registerResult.Success &&
            !registerResult.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0015", "Validation error", correlationId),
                data: registerResult
            ));
        }

        await _mediator.Send(new CreateRoleCommand(AppRoles.Administrator), ct);
        var addRoleResult = await _mediator.Send(
            new AddUserToRoleCommand(model.Username, AppRoles.Administrator),
            ct);

        if (!addRoleResult.Success)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0015", "Validation error", correlationId),
                data: addRoleResult
            ));
        }

        return Ok(ResponseDTO.Success(
            data: new
            {
                user = model.Username,
                role = AppRoles.Administrator,
                message = "Admin user created successfully"
            },
            correlationId: correlationId));
    }

    /// <summary>
    /// Creates a new administrator user.
    /// Requires an authenticated Administrator.
    /// </summary>
    [HttpPost]
    [Route("users/admin")]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CreateAdminUser(
        [FromBody] RegisterRequestDto model,
        CancellationToken ct = default)
    {
        var correlationId = HttpContext.GetCorrelationId();

        if (!ModelState.IsValid)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0002", "Invalid request - Model validation failed", correlationId),
                data: new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) }
            ));
        }

        var registerResult = await _mediator.Send(
            new RegisterCommand(model.Username, model.Email, model.Password),
            ct);

        if (!registerResult.Success)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0015", "Validation error", correlationId),
                data: registerResult
            ));
        }

        var addRoleResult = await _mediator.Send(
            new AddUserToRoleCommand(model.Username, AppRoles.Administrator),
            ct);

        if (!addRoleResult.Success)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0015", "Validation error", correlationId),
                data: addRoleResult
            ));
        }

        return Ok(ResponseDTO.Success(
            data: new
            {
                user = model.Username,
                role = AppRoles.Administrator,
                message = "Admin user created successfully"
            },
            correlationId: correlationId));
    }

    /// <summary>
    /// Adds a role to an existing user.
    /// Requires an authenticated Administrator.
    /// </summary>
    [HttpPost]
    [Route("users/{username}/roles/{role}")]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> AddUserToRole(
        string username,
        string role,
        CancellationToken ct = default)
    {
        var correlationId = HttpContext.GetCorrelationId();

        var addRoleResult = await _mediator.Send(
            new AddUserToRoleCommand(username, role),
            ct);

        if (!addRoleResult.Success)
        {
            return BadRequest(ResponseDTO.Error(
                new ResponseCodeDTO("0015", "Failed to add role", correlationId),
                data: addRoleResult
            ));
        }

        return Ok(ResponseDTO.Success(
            data: new
            {
                user = username,
                role = role,
                message = $"Role '{role}' added to user '{username}' successfully"
            },
            correlationId: correlationId));
    }

}
