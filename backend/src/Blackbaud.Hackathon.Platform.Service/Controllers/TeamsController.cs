using Blackbaud.Hackathon.Platform.Shared.BusinessLogic;
using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(HackathonDbContext context, ILogger<TeamsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all teams for a hackathon
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTeams([FromQuery] int? hackathonId)
    {
        var query = _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
            .Include(t => t.Ideas)
            .AsQueryable();

        if (hackathonId.HasValue)
        {
            query = query.Where(t => t.HackathonId == hackathonId.Value);
        }

        var teams = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(teams);
    }

    /// <summary>
    /// Gets a specific team
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeam(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
            .Include(t => t.Ideas)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    /// <summary>
    /// Creates a new team
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> CreateTeam([FromBody] Team team)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        team.LeaderId = userId;
        team.CreatedAt = DateTime.UtcNow;

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Add leader as team member
        var teamMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    /// <summary>
    /// Updates a team
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] Team team)
    {
        if (id != team.Id)
        {
            return BadRequest();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var existingTeam = await _context.Teams.FindAsync(id);

        if (existingTeam == null)
        {
            return NotFound();
        }

        // Only team leader can update
        if (existingTeam.LeaderId != userId)
        {
            return Forbid();
        }

        team.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingTeam).CurrentValues.SetValues(team);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Teams.AnyAsync(t => t.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Adds a member to a team
    /// </summary>
    [HttpPost("{id}/members")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> AddTeamMember(int id, [FromBody] AddTeamMemberRequest request)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Allow leader to add anyone, or a user to join themselves
        var isLeader = team.LeaderId == userId;
        var isSelfJoin = request.UserId == userId;
        if (!isLeader && !isSelfJoin)
        {
            return Forbid();
        }

        // Check if user is already a member
        var existingMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == id && tm.UserId == request.UserId);

        if (existingMember != null)
        {
            return BadRequest("User is already a member of this team");
        }

        var teamMember = new TeamMember
        {
            TeamId = id,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow
        };

        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        return Ok(teamMember);
    }

    /// <summary>
    /// Removes a member from a team
    /// </summary>
    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> RemoveTeamMember(int id, int userId)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Allow leader to remove anyone, or a user to remove themselves (leave team)
        var isLeader = team.LeaderId == currentUserId;
        var isSelfLeave = userId == currentUserId;
        if (!isLeader && !isSelfLeave)
        {
            return Forbid();
        }

        if (team.LeaderId == userId && !isLeader)
        {
            return BadRequest("Team leader cannot leave without transferring leadership.");
        }

        var teamMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == id && tm.UserId == userId);

        if (teamMember == null)
        {
            return NotFound();
        }

        _context.TeamMembers.Remove(teamMember);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a team
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var team = await _context.Teams.FindAsync(id);

        if (team == null)
        {
            return NotFound();
        }

        // Only team leader can delete
        if (team.LeaderId != userId)
        {
            return Forbid();
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class AddTeamMemberRequest
{
    public int UserId { get; set; }
}
