using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;

namespace ProductApi.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Authenticate and receive a short-lived access token plus a refresh token.
    /// NOTE: this sample validates against a placeholder check only — replace with
    /// a real identity/user store (e.g. ASP.NET Core Identity) before production use.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthResponseDto> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        // Placeholder credential check — swap for a real user store.
        var tokens = _tokenService.GenerateTokens(request.Username);
        return Ok(tokens);
    }

    /// <summary>Exchange a valid refresh token (with the expired access token) for a new token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthResponseDto> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var tokens = _tokenService.RefreshTokens(request.AccessToken, request.RefreshToken);
            return Ok(tokens);
        }
        catch (Exception)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }
    }
}
