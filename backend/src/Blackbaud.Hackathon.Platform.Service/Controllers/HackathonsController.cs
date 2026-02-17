using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HackathonsController : ControllerBase
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<HackathonsController> _logger;

    public HackathonsController(HackathonDbContext context, ILogger<HackathonsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all hackathons
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetHackathons()
    {
        var hackathons = await _context.Hackathons
            .Include(h => h.Tracks)
            .Include(h => h.Awards)
            .Include(h => h.JudgingCriteria)
            .Include(h => h.Milestones)
            .OrderByDescending(h => h.StartDate)
            .ToListAsync();

        return Ok(hackathons);
    }

    /// <summary>
    /// Gets a specific hackathon by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHackathon(int id)
    {
        var hackathon = await _context.Hackathons
            .Include(h => h.Tracks)
            .Include(h => h.Awards)
            .Include(h => h.JudgingCriteria)
            .Include(h => h.Milestones)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hackathon == null)
        {
            return NotFound();
        }

        return Ok(hackathon);
    }

    /// <summary>
    /// Gets the current active hackathon
    /// </summary>
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentHackathon()
    {
        var now = DateTime.UtcNow;
        var hackathon = await _context.Hackathons
            .Include(h => h.Tracks)
            .Include(h => h.Awards)
            .Include(h => h.JudgingCriteria)
            .Include(h => h.Milestones)
            .Where(h => h.StartDate <= now && h.EndDate >= now)
            .OrderByDescending(h => h.StartDate)
            .FirstOrDefaultAsync();

        if (hackathon == null)
        {
            // Return the next upcoming hackathon if no active one
            hackathon = await _context.Hackathons
                .Include(h => h.Tracks)
                .Include(h => h.Awards)
                .Include(h => h.JudgingCriteria)
                .Include(h => h.Milestones)
                .Where(h => h.StartDate > now)
                .OrderBy(h => h.StartDate)
                .FirstOrDefaultAsync();
        }

        if (hackathon == null)
        {
            return NotFound(new { message = "No active or upcoming hackathon found" });
        }

        return Ok(hackathon);
    }

    /// <summary>
    /// Creates a new hackathon (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateHackathon([FromBody] Shared.Models.Hackathon hackathon)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        hackathon.CreatedAt = DateTime.UtcNow;
        _context.Hackathons.Add(hackathon);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHackathon), new { id = hackathon.Id }, hackathon);
    }

    /// <summary>
    /// Updates an existing hackathon (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateHackathon(int id, [FromBody] Shared.Models.Hackathon hackathon)
    {
        if (id != hackathon.Id)
        {
            return BadRequest();
        }

        hackathon.UpdatedAt = DateTime.UtcNow;
        _context.Entry(hackathon).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Hackathons.AnyAsync(h => h.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a hackathon (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteHackathon(int id)
    {
        var hackathon = await _context.Hackathons.FindAsync(id);
        if (hackathon == null)
        {
            return NotFound();
        }

        _context.Hackathons.Remove(hackathon);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
