using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Shared.BusinessLogic;

public interface ICommentService
{
    Task<List<Comment>> GetCommentsByIdeaAsync(int ideaId);
    Task<Comment?> GetCommentByIdAsync(int commentId);
    Task<Comment> CreateCommentAsync(int ideaId, int userId, string content, int? parentCommentId = null);
    Task<Comment> UpdateCommentAsync(int commentId, string content);
    Task<bool> DeleteCommentAsync(int commentId);
    Task<bool> IsCommentAuthorAsync(int commentId, int userId);
    Task<List<Comment>> GetRepliesAsync(int parentCommentId);
}

public class CommentService : ICommentService
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<CommentService> _logger;

    public CommentService(HackathonDbContext context, ILogger<CommentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Comment>> GetCommentsByIdeaAsync(int ideaId)
    {
        return await _context.Comments
            .Where(c => c.IdeaId == ideaId && !c.IsDeleted && c.ParentCommentId == null)
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(int commentId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
    }

    public async Task<Comment> CreateCommentAsync(int ideaId, int userId, string content, int? parentCommentId = null)
    {
        var comment = new Comment
        {
            IdeaId = ideaId,
            UserId = userId,
            Content = content,
            ParentCommentId = parentCommentId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return comment;
    }

    public async Task<Comment> UpdateCommentAsync(int commentId, string content)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            throw new InvalidOperationException($"Comment {commentId} not found");
        }

        comment.Content = content;
        comment.UpdatedAt = DateTime.UtcNow;
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();

        return comment;
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> IsCommentAuthorAsync(int commentId, int userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        return comment?.UserId == userId;
    }

    public async Task<List<Comment>> GetRepliesAsync(int parentCommentId)
    {
        return await _context.Comments
            .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}
