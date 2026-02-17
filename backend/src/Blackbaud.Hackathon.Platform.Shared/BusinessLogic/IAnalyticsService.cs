using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    // TODO: Analytics implementation has data model reference issues and needs refactoring
    // Placeholder stubs allow deployment. Will be fixed in next iteration.

    public Task<HackathonAnalytics> GetHackathonAnalyticsAsync(int hackathonId)
        => throw new NotImplementedException("Analytics service is under maintenance. Coming soon!");

    public Task<SubmissionAnalytics> GetSubmissionAnalyticsAsync(int hackathonId)
        => throw new NotImplementedException("Analytics service is under maintenance. Coming soon!");

    public Task<JudgingAnalytics> GetJudgingAnalyticsAsync(int hackathonId)
        => throw new NotImplementedException("Analytics service is under maintenance. Coming soon!");

    public Task<TeamAnalytics> GetTeamAnalyticsAsync(int hackathonId)
        => throw new NotImplementedException("Analytics service is under maintenance. Coming soon!");

    public Task<List<IdeaRanking>> GetTopIdeasAsync(int hackathonId, int limit = 10)
        => throw new NotImplementedException("Analytics service is under maintenance. Coming soon!");

    public Task<Dictionary<string, int>> GetSubmissionsByTrackAsync(int hackathonId)
        => throw new NotImplementedException("Analytics service is under maintenance. Coming soon!");

    public Task<Dictionary<string, decimal>> GetAverageScoresByTrackAsync(int hackathonId)
        => throw new NotImplementedException("Analytics service is under maintenance. Coming soon!");
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
