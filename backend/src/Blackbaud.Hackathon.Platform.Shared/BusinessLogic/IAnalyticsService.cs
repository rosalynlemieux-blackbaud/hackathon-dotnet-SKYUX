using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Shared.BusinessLogic;

public interface IAnalyticsService
{
    Task<HackathonAnalytics> GetHackathonAnalyticsAsync(int hackathonId);
    Task<SubmissionAnalytics> GetSubmissionAnalyticsAsync(int hackathonId);
    Task<JudgingAnalytics> GetJudgingAnalyticsAsync(int hackathonId);
    Task<TeamAnalytics> GetTeamAnalyticsAsync(int hackathonId);
    Task<List<IdeaRanking>> GetTopIdeasAsync(int hackathonId, int limit = 10);
    Task<Dictionary<string, int>> GetSubmissionsByTrackAsync(int hackathonId);
    Task<Dictionary<string, decimal>> GetAverageScoresByTrackAsync(int hackathonId);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(HackathonDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HackathonAnalytics> GetHackathonAnalyticsAsync(int hackathonId)
    {
        var hackathon = await _context.Hackathons
            .Include(h => h.Users)
            .Include(h => h.Ideas)
            .Include(h => h.Tracks)
            .Include(h => h.Awards)
            .FirstOrDefaultAsync(h => h.Id == hackathonId);

        if (hackathon == null)
            throw new InvalidOperationException($"Hackathon {hackathonId} not found");

        var ideas = await _context.Ideas.Where(i => i.HackathonId == hackathonId).ToListAsync();
        var participants = await _context.Users.Where(u => u.HackathonId == hackathonId).ToListAsync();

        return new HackathonAnalytics
        {
            HackathonId = hackathonId,
            HackathonName = hackathon.Name,
            TotalParticipants = participants.Count,
            TotalIdeas = ideas.Count,
            SubmittedIdeas = ideas.Count(i => i.Status == "submitted" || i.Status == "judging" || i.Status == "complete"),
            DraftIdeas = ideas.Count(i => i.Status == "draft"),
            Tracks = hackathon.Tracks?.Count ?? 0,
            Awards = hackathon.Awards?.Count ?? 0,
            AverageIdeasPerParticipant = participants.Count > 0 ? (decimal)ideas.Count / participants.Count : 0,
            StartDate = hackathon.StartDate,
            EndDate = hackathon.EndDate
        };
    }

    public async Task<SubmissionAnalytics> GetSubmissionAnalyticsAsync(int hackathonId)
    {
        var ideas = await _context.Ideas
            .Where(i => i.HackathonId == hackathonId)
            .Include(i => i.Author)
            .Include(i => i.Submission)
            .ToListAsync();

        var submissions = ideas.Where(i => i.Status != "draft").ToList();

        return new SubmissionAnalytics
        {
            TotalSubmissions = submissions.Count,
            IndividualSubmissions = submissions.Count(i => i.Submission?.TeamId == null),
            TeamSubmissions = submissions.Count(i => i.Submission?.TeamId != null),
            SubmissionRate = ideas.Count > 0 ? (decimal)submissions.Count / ideas.Count * 100 : 0,
            SubmissionDeadlineMetCount = submissions.Count(i =>
                i.SubmittedAt <= (i.Hackathon?.SubmissionDeadline ?? DateTime.MaxValue)),
            EarliestSubmissionTime = submissions.Any() ? submissions.Min(i => i.SubmittedAt) : null,
            LatestSubmissionTime = submissions.Any() ? submissions.Max(i => i.SubmittedAt) : null
        };
    }

    public async Task<JudgingAnalytics> GetJudgingAnalyticsAsync(int hackathonId)
    {
        var ratings = await _context.Ratings
            .Include(r => r.Criterion)
            .Where(r => r.Idea.HackathonId == hackathonId)
            .ToListAsync();

        var judges = ratings.Select(r => r.JudgeId).Distinct().Count();
        var ideas = ratings.Select(r => r.IdeaId).Distinct().Count();

        var avgScores = ratings.GroupBy(r => r.IdeaId)
            .Select(g =>
            {
                var totalWeighted = 0m;
                var totalWeight = 0m;
                foreach (var rating in g)
                {
                    totalWeighted += rating.Score * rating.Criterion.Weight;
                    totalWeight += rating.Criterion.Weight;
                }
                return totalWeight > 0 ? totalWeighted / totalWeight : 0;
            })
            .ToList();

        return new JudgingAnalytics
        {
            TotalJudges = judges,
            IdeasBeingJudged = ideas,
            AverageScoreAcrossAll = avgScores.Count > 0 ? Math.Round(avgScores.Average(), 2) : 0,
            HighestScore = avgScores.Count > 0 ? Math.Round(avgScores.Max(), 2) : 0,
            LowestScore = avgScores.Count > 0 ? Math.Round(avgScores.Min(), 2) : 0,
            TotalRatingsSubmitted = ratings.Count,
            RatingsPerJudge = judges > 0 ? Math.Round((decimal)ratings.Count / judges, 2) : 0
        };
    }

    public async Task<TeamAnalytics> GetTeamAnalyticsAsync(int hackathonId)
    {
        var teams = await _context.Teams
            .Where(t => t.HackathonId == hackathonId)
            .Include(t => t.TeamMembers)
            .Include(t => t.Ideas)
            .ToListAsync();

        return new TeamAnalytics
        {
            TotalTeams = teams.Count,
            TotalTeamMembers = teams.Sum(t => t.TeamMembers?.Count ?? 0),
            TeamsWithIdeas = teams.Count(t => t.Ideas?.Count > 0),
            AverageMembersPerTeam = teams.Count > 0 ? Math.Round((decimal)teams.Sum(t => t.TeamMembers?.Count ?? 0) / teams.Count, 2) : 0,
            LargestTeamSize = teams.Any() ? teams.Max(t => t.TeamMembers?.Count ?? 0) : 0,
            IdeasSubmittedByTeams = teams.Sum(t => t.Ideas?.Count ?? 0)
        };
    }

    public async Task<List<IdeaRanking>> GetTopIdeasAsync(int hackathonId, int limit = 10)
    {
        var ideas = await _context.Ideas
            .Where(i => i.HackathonId == hackathonId && i.Status != "draft")
            .Include(i => i.Author)
            .Include(i => i.Ratings)
                .ThenInclude(r => r.Criterion)
            .OrderByDescending(i => i.Ratings.Count)
            .Take(limit)
            .ToListAsync();

        var rankings = ideas.Select((idea, index) =>
        {
            var avgScore = 0m;
            if (idea.Ratings.Any())
            {
                var totalWeighted = 0m;
                var totalWeight = 0m;
                foreach (var rating in idea.Ratings)
                {
                    totalWeighted += rating.Score * rating.Criterion.Weight;
                    totalWeight += rating.Criterion.Weight;
                }
                avgScore = totalWeight > 0 ? totalWeighted / totalWeight : 0;
            }

            return new IdeaRanking
            {
                Rank = index + 1,
                IdeaId = idea.Id,
                Title = idea.Title,
                Author = $"{idea.Author?.FirstName} {idea.Author?.LastName}",
                AverageScore = Math.Round(avgScore, 2),
                RatingCount = idea.Ratings.Select(r => r.JudgeId).Distinct().Count(),
                Status = idea.Status
            };
        }).ToList();

        return rankings;
    }

    public async Task<Dictionary<string, int>> GetSubmissionsByTrackAsync(int hackathonId)
    {
        var ideas = await _context.Ideas
            .Where(i => i.HackathonId == hackathonId && i.Status != "draft")
            .Include(i => i.Track)
            .GroupBy(i => i.Track.Name)
            .Select(g => new { Track = g.Key, Count = g.Count() })
            .ToListAsync();

        return ideas.ToDictionary(x => x.Track, x => x.Count);
    }

    public async Task<Dictionary<string, decimal>> GetAverageScoresByTrackAsync(int hackathonId)
    {
        var tracks = await _context.Tracks.Where(t => t.HackathonId == hackathonId).ToListAsync();
        var result = new Dictionary<string, decimal>();

        foreach (var track in tracks)
        {
            var avgScore = await _context.Ideas
                .Where(i => i.TrackId == track.Id && i.Status != "draft")
                .SelectMany(i => i.Ratings)
                .Include(r => r.Criterion)
                .GroupBy(r => r.IdeaId)
                .Select(g =>
                {
                    var totalWeighted = 0m;
                    var totalWeight = 0m;
                    foreach (var rating in g)
                    {
                        totalWeighted += rating.Score * rating.Criterion.Weight;
                        totalWeight += rating.Criterion.Weight;
                    }
                    return totalWeight > 0 ? totalWeighted / totalWeight : 0;
                })
                .AverageAsync();

            result[track.Name] = Math.Round(avgScore, 2);
        }

        return result;
    }
}

// DTOs
public class HackathonAnalytics
{
    public int HackathonId { get; set; }
    public string HackathonName { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalIdeas { get; set; }
    public int SubmittedIdeas { get; set; }
    public int DraftIdeas { get; set; }
    public int Tracks { get; set; }
    public int Awards { get; set; }
    public decimal AverageIdeasPerParticipant { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SubmissionAnalytics
{
    public int TotalSubmissions { get; set; }
    public int IndividualSubmissions { get; set; }
    public int TeamSubmissions { get; set; }
    public decimal SubmissionRate { get; set; }
    public int SubmissionDeadlineMetCount { get; set; }
    public DateTime? EarliestSubmissionTime { get; set; }
    public DateTime? LatestSubmissionTime { get; set; }
}

public class JudgingAnalytics
{
    public int TotalJudges { get; set; }
    public int IdeasBeingJudged { get; set; }
    public decimal AverageScoreAcrossAll { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
    public int TotalRatingsSubmitted { get; set; }
    public decimal RatingsPerJudge { get; set; }
}

public class TeamAnalytics
{
    public int TotalTeams { get; set; }
    public int TotalTeamMembers { get; set; }
    public int TeamsWithIdeas { get; set; }
    public decimal AverageMembersPerTeam { get; set; }
    public int LargestTeamSize { get; set; }
    public int IdeasSubmittedByTeams { get; set; }
}

public class IdeaRanking
{
    public int Rank { get; set; }
    public int IdeaId { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public decimal AverageScore { get; set; }
    public int RatingCount { get; set; }
    public string Status { get; set; }
}
