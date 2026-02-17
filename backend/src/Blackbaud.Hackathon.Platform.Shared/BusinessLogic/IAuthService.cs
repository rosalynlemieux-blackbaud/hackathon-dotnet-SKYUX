using Blackbaud.Hackathon.Platform.Shared.Models;
using Blackbaud.Hackathon.Platform.Shared.Models.DTOs;

namespace Blackbaud.Hackathon.Platform.Shared.BusinessLogic;

public interface IAuthService
{
    Task<AuthResponse> HandleBlackbaudCallback(string code);
    Task<User> GetOrCreateUser(BlackbaudUserInfo userInfo);
    string GenerateJwtToken(User user, List<string> roles);
}

public interface IUserService
{
    Task<User?> GetUserByBlackbaudId(string blackbaudId);
    Task<User?> GetUserById(int id);
    Task<User> CreateUser(User user);
    Task<User> UpdateUser(User user);
    Task<List<string>> GetUserRoles(int userId, int hackathonId);
}
