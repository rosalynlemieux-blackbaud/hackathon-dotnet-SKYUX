using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Shared.BusinessLogic;

public interface ITeamService
{
    Task<List<Team>> GetTeamsByHackathonAsync(int hackathonId);
    Task<Team?> GetTeamByIdAsync(int teamId);
    Task<Team> CreateTeamAsync(Team team, int leaderId);
    Task<Team> UpdateTeamAsync(Team team);
    Task<bool> IsTeamLeaderAsync(int teamId, int userId);
    Task<List<TeamMember>> GetTeamMembersAsync(int teamId);
    Task AddTeamMemberAsync(int teamId, int userId);
    Task RemoveTeamMemberAsync(int teamId, int userId);
    Task<bool> DeleteTeamAsync(int teamId);
}

public class TeamService : ITeamService
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<TeamService> _logger;

    public TeamService(HackathonDbContext context, ILogger<TeamService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Team>> GetTeamsByHackathonAsync(int hackathonId)
    {
        return await _context.Teams
            .Where(t => t.HackathonId == hackathonId)
            .Include(t => t.Leader)
            .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int teamId)
    {
        return await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
            .Include(t => t.Ideas)
            .FirstOrDefaultAsync(t => t.Id == teamId);
    }

    public async Task<Team> CreateTeamAsync(Team team, int leaderId)
    {
        team.LeaderId = leaderId;
        team.CreatedAt = DateTime.UtcNow;
        
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Add leader as first member
        var teamMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = leaderId,
            JoinedAt = DateTime.UtcNow
        };
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        return team;
    }

    public async Task<Team> UpdateTeamAsync(Team team)
    {
        team.UpdatedAt = DateTime.UtcNow;
        _context.Teams.Update(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task<bool> IsTeamLeaderAsync(int teamId, int userId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        return team?.LeaderId == userId;
    }

    public async Task<List<TeamMember>> GetTeamMembersAsync(int teamId)
    {
        return await _context.TeamMembers
            .Where(tm => tm.TeamId == teamId)
            .Include(tm => tm.User)
            .ToListAsync();
    }

    public async Task AddTeamMemberAsync(int teamId, int userId)
    {
        var existingMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (existingMember == null)
        {
            var teamMember = new TeamMember
            {
                TeamId = teamId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveTeamMemberAsync(int teamId, int userId)
    {
        var teamMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (teamMember != null)
        {
            _context.TeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> DeleteTeamAsync(int teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team != null)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
