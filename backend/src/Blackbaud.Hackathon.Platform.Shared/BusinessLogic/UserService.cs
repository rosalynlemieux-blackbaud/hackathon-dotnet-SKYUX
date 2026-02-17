using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Shared.BusinessLogic;

public class UserService : IUserService
{
    private readonly HackathonDbContext _context;

    public UserService(HackathonDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByBlackbaudId(string blackbaudId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.BlackbaudId == blackbaudId);
    }

    public async Task<User?> GetUserById(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUser(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<List<string>> GetUserRoles(int userId, int hackathonId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.HackathonId == hackathonId)
            .Select(ur => ur.Role)
            .Distinct()
            .ToListAsync();
    }
}
