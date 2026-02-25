using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Blackbaud.Hackathon.Platform.Service.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IdeasController : ControllerBase
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<IdeasController> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public IdeasController(
        HackathonDbContext context, 
        ILogger<IdeasController> logger,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    /// <summary>
    /// Gets all ideas for a hackathon
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetIdeas([FromQuery] int? hackathonId, [FromQuery] string? status)
    {
        var query = _context.Ideas
            .Include(i => i.Team)
            .Include(i => i.Track)
            .Include(i => i.SubmittedByUser)
            .Include(i => i.IdeaAwards)
                .ThenInclude(ia => ia.Award)
            .AsQueryable();

        if (hackathonId.HasValue)
        {
            query = query.Where(i => i.HackathonId == hackathonId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        var ideas = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(ideas);
    }

    /// <summary>
    /// Gets ideas for a specific team
    /// </summary>
    [HttpGet("team/{teamId}")]
    public async Task<IActionResult> GetIdeasByTeam(int teamId)
    {
        var ideas = await _context.Ideas
            .Include(i => i.Team)
            .Include(i => i.Track)
            .Include(i => i.SubmittedByUser)
            .Include(i => i.IdeaAwards)
                .ThenInclude(ia => ia.Award)
            .Where(i => i.TeamId == teamId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(ideas);
    }

    /// <summary>
    /// Searches ideas by title/description
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchIdeas([FromQuery] string? q, [FromQuery] int? hackathonId)
    {
        var queryText = q?.Trim();
        if (string.IsNullOrWhiteSpace(queryText))
        {
            return Ok(new List<Idea>());
        }

        var query = _context.Ideas
            .Include(i => i.Team)
            .Include(i => i.Track)
            .Include(i => i.SubmittedByUser)
            .Include(i => i.IdeaAwards)
                .ThenInclude(ia => ia.Award)
            .AsQueryable();

        if (hackathonId.HasValue)
        {
            query = query.Where(i => i.HackathonId == hackathonId.Value);
        }

        query = query.Where(i =>
            EF.Functions.Like(i.Title, $"%{queryText}%") ||
            EF.Functions.Like(i.Description, $"%{queryText}%"));

        var ideas = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(ideas);
    }

    /// <summary>
    /// Gets a specific idea
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetIdea(int id)
    {
        var idea = await _context.Ideas
            .Include(i => i.Team)
                .ThenInclude(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
            .Include(i => i.Track)
            .Include(i => i.SubmittedByUser)
            .Include(i => i.IdeaAwards)
                .ThenInclude(ia => ia.Award)
            .Include(i => i.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
            .Include(i => i.Ratings)
                .ThenInclude(r => r.Criterion)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (idea == null)
        {
            return NotFound();
        }

        return Ok(idea);
    }

    /// <summary>
    /// Creates a new idea
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> CreateIdea([FromBody] Idea idea)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        idea.SubmittedBy = userId;
        idea.CreatedAt = DateTime.UtcNow;
        idea.Status = "draft";

        _context.Ideas.Add(idea);
        await _context.SaveChangesAsync();

        // Notify about new idea creation
        await _notificationService.NotifyIdeaSubmitted(idea.HackathonId, new
        {
            id = idea.Id,
            title = idea.Title,
            description = idea.Description,
            status = idea.Status,
            createdAt = idea.CreatedAt
        });

        return CreatedAtAction(nameof(GetIdea), new { id = idea.Id }, idea);
    }

    /// <summary>
    /// Updates an existing idea
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> UpdateIdea(int id, [FromBody] Idea idea)
    {
        if (id != idea.Id)
        {
            return BadRequest();
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var existingIdea = await _context.Ideas.FindAsync(id);

        if (existingIdea == null)
        {
            return NotFound();
        }

        // Only allow the submitter or team leader to update
        var team = await _context.Teams.FindAsync(existingIdea.TeamId);
        if (existingIdea.SubmittedBy != userId && team?.LeaderId != userId)
        {
            return Forbid();
        }

        idea.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingIdea).CurrentValues.SetValues(idea);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Ideas.AnyAsync(i => i.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Submit an idea for judging
    /// </summary>
    [HttpPost("{id}/submit")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> SubmitIdea(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var idea = await _context.Ideas
            .Include(i => i.Team)
            .Include(i => i.SubmittedByUser)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (idea == null)
        {
            return NotFound();
        }

        // Only allow the submitter or team leader to submit
        if (idea.SubmittedBy != userId && idea.Team.LeaderId != userId)
        {
            return Forbid();
        }

        idea.Status = "submitted";
        idea.SubmittedAt = DateTime.UtcNow;
        idea.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Notify about status change
        await _notificationService.NotifyIdeaStatusChanged(idea.HackathonId, idea.Id, "submitted");
        await _notificationService.NotifyIdeaSubmitted(idea.HackathonId, new
        {
            id = idea.Id,
            title = idea.Title,
            status = idea.Status,
            submittedAt = idea.SubmittedAt
        });

        // Send submission confirmation email
        if (idea.SubmittedByUser?.Email != null)
        {
            var ideaLink = $"https://hackathon.example.com/ideas/{idea.Id}";
            await _emailService.SendIdeaSubmissionEmailAsync(
                idea.SubmittedByUser.Email,
                idea.Title,
                ideaLink
            );
        }

        return Ok(idea);
    }

    /// <summary>
    /// Deletes an idea
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> DeleteIdea(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var idea = await _context.Ideas
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (idea == null)
        {
            return NotFound();
        }

        // Only allow the submitter or team leader to delete
        if (idea.SubmittedBy != userId && idea.Team.LeaderId != userId)
        {
            return Forbid();
        }

        var hackathonId = idea.HackathonId;
        _context.Ideas.Remove(idea);
        await _context.SaveChangesAsync();

        // Notify about deletion
        await _notificationService.NotifyIdeaDeleted(hackathonId, id);

        return NoContent();
    }
}
