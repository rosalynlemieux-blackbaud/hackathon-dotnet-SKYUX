namespace Blackbaud.Hackathon.Platform.Shared.Models.DTOs;

public class AuthCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string? State { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UserDTO User { get; set; } = null!;
}

public class UserDTO
{
    public int Id { get; set; }
    public string BlackbaudId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class BlackbaudTokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public string? refresh_token { get; set; }
}

public class BlackbaudUserInfo
{
    public string sub { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string given_name { get; set; } = string.Empty;
    public string family_name { get; set; } = string.Empty;
}
