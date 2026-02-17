using Blackbaud.Hackathon.Platform.Shared.BusinessLogic;
using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blackbaud.Hackathon.Platform.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("hackathon/{hackathonId}")]
    public async Task<IActionResult> GetHackathonAnalytics(int hackathonId)
    {
        var analytics = await _analyticsService.GetHackathonAnalyticsAsync(hackathonId);
        return Ok(analytics);
    }

    [HttpGet("submissions/{hackathonId}")]
    public async Task<IActionResult> GetSubmissionAnalytics(int hackathonId)
    {
        var analytics = await _analyticsService.GetSubmissionAnalyticsAsync(hackathonId);
        return Ok(analytics);
    }

    [HttpGet("judging/{hackathonId}")]
    public async Task<IActionResult> GetJudgingAnalytics(int hackathonId)
    {
        var analytics = await _analyticsService.GetJudgingAnalyticsAsync(hackathonId);
        return Ok(analytics);
    }

    [HttpGet("teams/{hackathonId}")]
    public async Task<IActionResult> GetTeamAnalytics(int hackathonId)
    {
        var analytics = await _analyticsService.GetTeamAnalyticsAsync(hackathonId);
        return Ok(analytics);
    }

    [HttpGet("top-ideas/{hackathonId}")]
    public async Task<IActionResult> GetTopIdeas(int hackathonId, [FromQuery] int limit = 10)
    {
        var topIdeas = await _analyticsService.GetTopIdeasAsync(hackathonId, limit);
        return Ok(topIdeas);
    }

    [HttpGet("submissions-by-track/{hackathonId}")]
    public async Task<IActionResult> GetSubmissionsByTrack(int hackathonId)
    {
        var data = await _analyticsService.GetSubmissionsByTrackAsync(hackathonId);
        return Ok(data);
    }

    [HttpGet("average-scores-by-track/{hackathonId}")]
    public async Task<IActionResult> GetAverageScoresByTrack(int hackathonId)
    {
        var data = await _analyticsService.GetAverageScoresByTrackAsync(hackathonId);
        return Ok(data);
    }

    [HttpGet("dashboard-summary/{hackathonId}")]
    public async Task<IActionResult> GetDashboardSummary(int hackathonId)
    {
        var hackathonAnalytics = await _analyticsService.GetHackathonAnalyticsAsync(hackathonId);
        var submissionAnalytics = await _analyticsService.GetSubmissionAnalyticsAsync(hackathonId);
        var judgingAnalytics = await _analyticsService.GetJudgingAnalyticsAsync(hackathonId);
        var teamAnalytics = await _analyticsService.GetTeamAnalyticsAsync(hackathonId);
        var topIdeas = await _analyticsService.GetTopIdeasAsync(hackathonId, 5);

        return Ok(new
        {
            hackathon = hackathonAnalytics,
            submissions = submissionAnalytics,
            judging = judgingAnalytics,
            teams = teamAnalytics,
            topIdeas
        });
    }
}
