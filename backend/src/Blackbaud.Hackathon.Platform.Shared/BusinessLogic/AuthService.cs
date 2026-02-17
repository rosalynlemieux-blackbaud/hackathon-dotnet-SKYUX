using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Blackbaud.Hackathon.Platform.Shared.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Blackbaud.Hackathon.Platform.Shared.BusinessLogic;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly HackathonDbContext _context;
    private readonly IUserService _userService;

    public AuthService(
        IConfiguration configuration,
        HttpClient httpClient,
        HackathonDbContext context,
        IUserService userService)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _context = context;
        _userService = userService;
    }

    public async Task<AuthResponse> HandleBlackbaudCallback(string code)
    {
        // Exchange authorization code for access token
        var tokenResponse = await ExchangeCodeForToken(code);

        // Extract user info from token (BBID includes user info in token response)
        var userInfo = ExtractUserInfoFromToken(tokenResponse.access_token);

        // Get or create user in our database
        var user = await GetOrCreateUser(userInfo);

        // Get user roles for the current hackathon (assume hackathon ID 1 for now)
        var roles = await _userService.GetUserRoles(user.Id, 1);

        // Generate our own JWT token
        var jwtToken = GenerateJwtToken(user, roles);

        return new AuthResponse
        {
            AccessToken = jwtToken,
            ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60,
            User = new UserDTO
            {
                Id = user.Id,
                BlackbaudId = user.BlackbaudId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles
            }
        };
    }

    private async Task<BlackbaudTokenResponse> ExchangeCodeForToken(string code)
    {
        var clientId = _configuration["BlackbaudAuth:ClientId"];
        var clientSecret = _configuration["BlackbaudAuth:ClientSecret"];
        var redirectUri = _configuration["BlackbaudAuth:RedirectUri"];
        var tokenEndpoint = _configuration["BlackbaudAuth:TokenEndpoint"];

        // Create Basic auth header
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri! }
        };

        request.Content = new FormUrlEncodedContent(formData);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<BlackbaudTokenResponse>(content);

        return tokenResponse ?? throw new Exception("Failed to deserialize token response");
    }

    private BlackbaudUserInfo ExtractUserInfoFromToken(string accessToken)
    {
        // Decode JWT token to extract user claims
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);

        return new BlackbaudUserInfo
        {
            sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? string.Empty,
            email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
            given_name = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ?? string.Empty,
            family_name = token.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value ?? string.Empty
        };
    }

    public async Task<User> GetOrCreateUser(BlackbaudUserInfo userInfo)
    {
        var existingUser = await _userService.GetUserByBlackbaudId(userInfo.sub);

        if (existingUser != null)
        {
            return existingUser;
        }

        // Create new user
        var newUser = new User
        {
            BlackbaudId = userInfo.sub,
            Email = userInfo.email,
            FirstName = userInfo.given_name,
            LastName = userInfo.family_name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _userService.CreateUser(newUser);
    }

    public string GenerateJwtToken(User user, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("blackbaud_id", user.BlackbaudId)
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"] ?? "60")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
