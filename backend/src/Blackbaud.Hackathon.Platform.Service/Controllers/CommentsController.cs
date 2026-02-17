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
public class CommentsController : ControllerBase
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<CommentsController> _logger;
    private readonly INotificationService _notificationService;

    public CommentsController(
        HackathonDbContext context, 
        ILogger<CommentsController> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets all comments for an idea
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery] int ideaId)
    {
        var comments = await _context.Comments
            .Where(c => c.IdeaId == ideaId && !c.IsDeleted)
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments);
    }

    /// <summary>
    /// Gets a specific comment
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComment(int id)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (comment == null)
        {
            return NotFound();
        }

        return Ok(comment);
    }

    /// <summary>
    /// Creates a new comment on an idea
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if idea exists
        var idea = await _context.Ideas.FindAsync(request.IdeaId);
        if (idea == null)
        {
            return BadRequest("Idea not found");
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var comment = new Comment
        {
            IdeaId = request.IdeaId,
            UserId = userId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Load user info for notification
        var user = await _context.Users.FindAsync(userId);

        // Notify about comment
        await _notificationService.NotifyCommentAdded(idea.HackathonId, idea.Id, new
        {
            id = comment.Id,
            content = comment.Content,
            authorId = userId,
            authorName = user != null ? $"{user.FirstName} {user.LastName}" : "Anonymous",
            createdAt = comment.CreatedAt,
            parentCommentId = comment.ParentCommentId
        });

        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    }

    /// <summary>
    /// Updates a comment
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var comment = await _context.Comments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        // Only the comment author can update
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        if (comment.IsDeleted)
        {
            return BadRequest("Cannot update a deleted comment");
        }

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();

        return Ok(comment);
    }

    /// <summary>
    /// Soft deletes a comment
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var comment = await _context.Comments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        // Only the comment author can delete
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateCommentRequest
{
    public int IdeaId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
