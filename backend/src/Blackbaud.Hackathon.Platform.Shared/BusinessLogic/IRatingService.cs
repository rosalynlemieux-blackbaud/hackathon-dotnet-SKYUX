using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Blackbaud.Hackathon.Platform.Shared.BusinessLogic;

public interface IRatingService
{
    Task<List<Rating>> GetRatingsByIdeaAsync(int ideaId);
    Task<List<Rating>> GetRatingsByJudgeAsync(int judgeId);
    Task<Rating?> GetRatingByIdAsync(int ratingId);
    Task<Rating> CreateOrUpdateRatingAsync(int ideaId, int judgeId, int criterionId, int score, string? feedback);
    Task<(decimal AverageScore, int RatingCount)> GetAverageRatingAsync(int ideaId);
    Task<Dictionary<int, (decimal Score, int Count)>> GetAverageRatingsByCriterionAsync(int ideaId);
    Task<bool> DeleteRatingAsync(int ratingId);
}

public class RatingService : IRatingService
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<RatingService> _logger;

    public RatingService(HackathonDbContext context, ILogger<RatingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Rating>> GetRatingsByIdeaAsync(int ideaId)
    {
        return await _context.Ratings
            .Where(r => r.IdeaId == ideaId)
            .Include(r => r.Judge)
            .Include(r => r.Criterion)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Rating>> GetRatingsByJudgeAsync(int judgeId)
    {
        return await _context.Ratings
            .Where(r => r.JudgeId == judgeId)
            .Include(r => r.Idea)
            .Include(r => r.Criterion)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Rating?> GetRatingByIdAsync(int ratingId)
    {
        return await _context.Ratings
            .Include(r => r.Judge)
            .Include(r => r.Idea)
            .Include(r => r.Criterion)
            .FirstOrDefaultAsync(r => r.Id == ratingId);
    }

    public async Task<Rating> CreateOrUpdateRatingAsync(int ideaId, int judgeId, int criterionId, int score, string? feedback)
    {
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.IdeaId == ideaId && r.JudgeId == judgeId && r.CriterionId == criterionId);

        if (existingRating != null)
        {
            existingRating.Score = score;
            existingRating.Feedback = feedback;
            existingRating.UpdatedAt = DateTime.UtcNow;
            _context.Ratings.Update(existingRating);
            await _context.SaveChangesAsync();
            return existingRating;
        }

        var rating = new Rating
        {
            IdeaId = ideaId,
            JudgeId = judgeId,
            CriterionId = criterionId,
            Score = score,
            Feedback = feedback,
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<(decimal AverageScore, int RatingCount)> GetAverageRatingAsync(int ideaId)
    {
        var ratings = await _context.Ratings
            .Where(r => r.IdeaId == ideaId)
            .Include(r => r.Criterion)
            .ToListAsync();

        if (!ratings.Any())
        {
            return (0, 0);
        }

        var totalWeightedScore = 0m;
        var totalWeight = 0m;

        foreach (var rating in ratings)
        {
            totalWeightedScore += rating.Score * rating.Criterion.Weight;
            totalWeight += rating.Criterion.Weight;
        }

        var averageScore = totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
        return (Math.Round(averageScore, 2), ratings.Select(r => r.JudgeId).Distinct().Count());
    }

    public async Task<Dictionary<int, (decimal Score, int Count)>> GetAverageRatingsByCriterionAsync(int ideaId)
    {
        var ratings = await _context.Ratings
            .Where(r => r.IdeaId == ideaId)
            .Include(r => r.Criterion)
            .ToListAsync();

        var byCriterion = new Dictionary<int, (decimal Score, int Count)>();

        foreach (var criterionGroup in ratings.GroupBy(r => r.CriterionId))
        {
            var criterionRatings = criterionGroup.ToList();
            var averageScore = criterionRatings.Count > 0 
                ? Math.Round((decimal)criterionRatings.Average(r => r.Score), 2)
                : 0;
            byCriterion[criterionGroup.Key] = (averageScore, criterionRatings.Count);
        }

        return byCriterion;
    }

    public async Task<bool> DeleteRatingAsync(int ratingId)
    {
        var rating = await _context.Ratings.FindAsync(ratingId);
        if (rating != null)
        {
            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
