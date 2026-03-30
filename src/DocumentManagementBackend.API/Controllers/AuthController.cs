using DocumentManagementBackend.Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace DocumentManagementBackend.API.Controllers;

/// <summary>Authentication endpoints</summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Login and obtain a JWT token</summary>
    /// <remarks>
    /// Use the returned token in the Authorization header for all subsequent requests:
    /// 
    ///     Authorization: Bearer {token}
    /// 
    /// Sample request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "YourPassword123!"
    ///     }
    /// </remarks>
    /// <param name="command">Login credentials</param>
    /// <returns>JWT token with user information</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}