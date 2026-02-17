using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(HackathonDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all hackathons
    /// </summary>
    [HttpGet("hackathons")]
    public async Task<IActionResult> GetHackathons()
    {
        var hackathons = await _context.Hackathons
            .Include(h => h.Tracks)
            .Include(h => h.Awards)
            .Include(h => h.JudgingCriteria)
            .OrderByDescending(h => h.StartDate)
            .ToListAsync();

        return Ok(hackathons);
    }

    /// <summary>
    /// Get hackathon details
    /// </summary>
    [HttpGet("hackathons/{id}")]
    public async Task<IActionResult> GetHackathon(int id)
    {
        var hackathon = await _context.Hackathons
            .Include(h => h.Tracks)
            .Include(h => h.Awards)
            .Include(h => h.JudgingCriteria)
            .Include(h => h.Users)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hackathon == null)
            return NotFound();

        return Ok(hackathon);
    }

    /// <summary>
    /// Update hackathon
    /// </summary>
    [HttpPut("hackathons/{id}")]
    public async Task<IActionResult> UpdateHackathon(int id, [FromBody] UpdateHackathonRequest request)
    {
        var hackathon = await _context.Hackathons.FindAsync(id);
        if (hackathon == null)
            return NotFound();

        hackathon.Name = request.Name ?? hackathon.Name;
        hackathon.Description = request.Description ?? hackathon.Description;
        hackathon.StartDate = request.StartDate ?? hackathon.StartDate;
        hackathon.EndDate = request.EndDate ?? hackathon.EndDate;
        hackathon.SubmissionDeadline = request.SubmissionDeadline ?? hackathon.SubmissionDeadline;
        hackathon.JudgingDeadline = request.JudgingDeadline ?? hackathon.JudgingDeadline;
        hackathon.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(hackathon);
    }

    /// <summary>
    /// Get all users for a hackathon
    /// </summary>
    [HttpGet("hackathons/{hackathonId}/users")]
    public async Task<IActionResult> GetHackathonUsers(int hackathonId)
    {
        var users = await _context.Users
            .Where(u => u.HackathonId == hackathonId)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .OrderBy(u => u.LastName)
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Update user role
    /// </summary>
    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(user);
    }

    /// <summary>
    /// Get all judging criteria for hackathon
    /// </summary>
    [HttpGet("hackathons/{hackathonId}/criteria")]
    public async Task<IActionResult> GetJudgingCriteria(int hackathonId)
    {
        var criteria = await _context.JudgingCriteria
            .Where(c => c.HackathonId == hackathonId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(criteria);
    }

    /// <summary>
    /// Add judging criterion
    /// </summary>
    [HttpPost("hackathons/{hackathonId}/criteria")]
    public async Task<IActionResult> AddJudgingCriterion(int hackathonId, [FromBody] CreateCriterionRequest request)
    {
        var criterion = new JudgingCriterion
        {
            HackathonId = hackathonId,
            Name = request.Name,
            Description = request.Description,
            Weight = request.Weight,
            MaxScore = request.MaxScore ?? 10,
            CreatedAt = DateTime.UtcNow
        };

        _context.JudgingCriteria.Add(criterion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetJudgingCriteria), new { hackathonId }, criterion);
    }

    /// <summary>
    /// Update judging criterion
    /// </summary>
    [HttpPut("criteria/{id}")]
    public async Task<IActionResult> UpdateJudgingCriterion(int id, [FromBody] UpdateCriterionRequest request)
    {
        var criterion = await _context.JudgingCriteria.FindAsync(id);
        if (criterion == null)
            return NotFound();

        criterion.Name = request.Name ?? criterion.Name;
        criterion.Description = request.Description ?? criterion.Description;
        criterion.Weight = request.Weight ?? criterion.Weight;
        criterion.MaxScore = request.MaxScore ?? criterion.MaxScore;
        criterion.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(criterion);
    }

    /// <summary>
    /// Delete judging criterion
    /// </summary>
    [HttpDelete("criteria/{id}")]
    public async Task<IActionResult> DeleteJudgingCriterion(int id)
    {
        var criterion = await _context.JudgingCriteria.FindAsync(id);
        if (criterion == null)
            return NotFound();

        // Check if criterion is in use
        var ratingsCount = await _context.Ratings.CountAsync(r => r.CriterionId == id);
        if (ratingsCount > 0)
            return BadRequest("Cannot delete criterion that has ratings");

        _context.JudgingCriteria.Remove(criterion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get all awards for hackathon
    /// </summary>
    [HttpGet("hackathons/{hackathonId}/awards")]
    public async Task<IActionResult> GetAwards(int hackathonId)
    {
        var awards = await _context.Awards
            .Where(a => a.HackathonId == hackathonId)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return Ok(awards);
    }

    /// <summary>
    /// Announce hackathon winners
    /// </summary>
    [HttpPost("hackathons/{hackathonId}/announce-winners")]
    public async Task<IActionResult> AnnounceWinners(int hackathonId, [FromBody] List<int> awardIds)
    {
        var hackathon = await _context.Hackathons.FindAsync(hackathonId);
        if (hackathon == null)
            return NotFound();

        hackathon.Status = "announced";
        hackathon.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Winners announced", hackathonId });
    }

    /// <summary>
    /// Get submission statistics
    /// </summary>
    [HttpGet("hackathons/{hackathonId}/export")]
    public async Task<IActionResult> ExportHackathonData(int hackathonId)
    {
        var ideas = await _context.Ideas
            .Where(i => i.HackathonId == hackathonId)
            .Include(i => i.Author)
            .Include(i => i.Track)
            .Include(i => i.Ratings)
            .ToListAsync();

        var exportData = ideas.Select(i => new
        {
            i.Id,
            i.Title,
            i.Status,
            Author = $"{i.Author?.FirstName} {i.Author?.LastName}",
            Track = i.Track?.Name,
            i.Description,
            RatingCount = i.Ratings.Select(r => r.JudgeId).Distinct().Count(),
            i.CreatedAt,
            i.SubmittedAt
        }).ToList();

        return Ok(exportData);
    }
}

public class UpdateHackathonRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public DateTime? JudgingDeadline { get; set; }
}

public class UpdateUserRoleRequest
{
    public string Role { get; set; }
}

public class CreateCriterionRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Weight { get; set; }
    public int? MaxScore { get; set; }
}

public class UpdateCriterionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Weight { get; set; }
    public int? MaxScore { get; set; }
}
