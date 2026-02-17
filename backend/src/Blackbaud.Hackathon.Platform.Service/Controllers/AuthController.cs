using Blackbaud.Hackathon.Platform.Shared.BusinessLogic;
using Blackbaud.Hackathon.Platform.Shared.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Blackbaud.Hackathon.Platform.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Initiates Blackbaud OAuth flow by redirecting to BBID authorization endpoint
    /// </summary>
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        var clientId = _configuration["BlackbaudAuth:ClientId"];
        var redirectUri = _configuration["BlackbaudAuth:RedirectUri"];
        var authEndpoint = _configuration["BlackbaudAuth:AuthorizationEndpoint"];

        var state = string.IsNullOrEmpty(returnUrl) ? Guid.NewGuid().ToString() : returnUrl;

        var authUrl = $"{authEndpoint}?" +
            $"client_id={clientId}&" +
            $"response_type=code&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri!)}&" +
            $"state={state}";

        return Ok(new { authUrl });
    }

    /// <summary>
    /// Handles OAuth callback from Blackbaud
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromBody] AuthCallbackRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { error = "Authorization code is required" });
            }

            var authResponse = await _authService.HandleBlackbaudCallback(request.Code);

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback");
            return StatusCode(500, new { error = "Authentication failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Returns the current authenticated user info
    /// </summary>
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity?.IsAuthenticated ?? false)
        {
            return Unauthorized();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var blackbaudId = User.FindFirst("blackbaud_id")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            id = userId,
            email,
            name,
            blackbaudId,
            roles
        });
    }
}
