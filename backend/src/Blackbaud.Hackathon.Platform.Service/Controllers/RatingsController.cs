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
public class RatingsController : ControllerBase
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<RatingsController> _logger;
    private readonly INotificationService _notificationService;

    public RatingsController(
        HackathonDbContext context, 
        ILogger<RatingsController> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets all ratings for an idea
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRatings([FromQuery] int? ideaId, [FromQuery] int? judgeId)
    {
        var query = _context.Ratings
            .Include(r => r.Judge)
            .Include(r => r.Criterion)
            .AsQueryable();

        if (ideaId.HasValue)
        {
            query = query.Where(r => r.IdeaId == ideaId.Value);
        }

        if (judgeId.HasValue)
        {
            query = query.Where(r => r.JudgeId == judgeId.Value);
        }

        var ratings = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(ratings);
    }

    /// <summary>
    /// Gets a specific rating
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRating(int id)
    {
        var rating = await _context.Ratings
            .Include(r => r.Judge)
            .Include(r => r.Criterion)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rating == null)
        {
            return NotFound();
        }

        return Ok(rating);
    }

    /// <summary>
    /// Submits or updates a rating for an idea
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "JudgeOnly")]
    public async Task<IActionResult> CreateOrUpdateRating([FromBody] CreateRatingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var judgeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Check if idea exists
        var idea = await _context.Ideas.FindAsync(request.IdeaId);
        if (idea == null)
        {
            return BadRequest("Idea not found");
        }

        // Check if criterion exists
        var criterion = await _context.JudgingCriteria.FindAsync(request.CriterionId);
        if (criterion == null)
        {
            return BadRequest("Judging criterion not found");
        }

        // Check if rating already exists
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.IdeaId == request.IdeaId && 
                                     r.JudgeId == judgeId && 
                                     r.CriterionId == request.CriterionId);

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Score = request.Score;
            existingRating.Feedback = request.Feedback;
            existingRating.UpdatedAt = DateTime.UtcNow;
            _context.Ratings.Update(existingRating);
            await _context.SaveChangesAsync();

            // Notify about rating update
            var judge = await _context.Users.FindAsync(judgeId);
            await _notificationService.NotifyRatingSubmitted(idea.HackathonId, request.IdeaId, new
            {
                id = existingRating.Id,
                score = existingRating.Score,
                feedback = existingRating.Feedback,
                judgeEmail = judge?.Email,
                criterionId = request.CriterionId,
                updatedAt = existingRating.UpdatedAt
            });

            return Ok(existingRating);
        }

        // Create new rating
        var rating = new Rating
        {
            IdeaId = request.IdeaId,
            JudgeId = judgeId,
            CriterionId = request.CriterionId,
            Score = request.Score,
            Feedback = request.Feedback,
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Notify about new rating
        var judgeUser = await _context.Users.FindAsync(judgeId);
        await _notificationService.NotifyRatingSubmitted(idea.HackathonId, request.IdeaId, new
        {
            id = rating.Id,
            score = rating.Score,
            feedback = rating.Feedback,
            judgeEmail = judgeUser?.Email,
            criterionId = request.CriterionId,
            createdAt = rating.CreatedAt
        });

        return CreatedAtAction(nameof(GetRating), new { id = rating.Id }, rating);
    }

    /// <summary>
    /// Gets average rating for an idea (weighted by criterion)
    /// </summary>
    [HttpGet("idea/{ideaId}/average")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAverageRating(int ideaId)
    {
        var ratings = await _context.Ratings
            .Include(r => r.Criterion)
            .Where(r => r.IdeaId == ideaId)
            .ToListAsync();

        if (!ratings.Any())
        {
            return Ok(new { averageScore = 0, ratingCount = 0 });
        }

        var totalWeightedScore = 0.0m;
        var totalWeight = 0.0m;

        foreach (var rating in ratings)
        {
            totalWeightedScore += rating.Score * rating.Criterion.Weight;
            totalWeight += rating.Criterion.Weight;
        }

        var averageScore = totalWeight > 0 ? totalWeightedScore / totalWeight : 0;

        return Ok(new
        {
            averageScore = Math.Round(averageScore, 2),
            ratingCount = ratings.Count,
            byJudge = ratings.GroupBy(r => r.JudgeId)
                .Select(g => new
                {
                    judgeId = g.Key,
                    count = g.Count()
                })
        });
    }

    /// <summary>
    /// Deletes a rating (only by the judge who created it)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "JudgeOnly")]
    public async Task<IActionResult> DeleteRating(int id)
    {
        var judgeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var rating = await _context.Ratings.FindAsync(id);

        if (rating == null)
        {
            return NotFound();
        }

        // Only the judge who created it or an admin can delete
        if (rating.JudgeId != judgeId)
        {
            return Forbid();
        }

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateRatingRequest
{
    public int IdeaId { get; set; }
    public int CriterionId { get; set; }
    public int Score { get; set; }
    public string? Feedback { get; set; }
}
