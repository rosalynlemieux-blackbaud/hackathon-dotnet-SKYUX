using Microsoft.AspNetCore.SignalR;
using Blackbaud.Hackathon.Platform.Service.Hubs;

namespace Blackbaud.Hackathon.Platform.Service.BusinessLogic;

/// <summary>
/// Implementation of notification service using SignalR
/// Sends real-time notifications to connected clients
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Notify all users in a hackathon that idea was submitted
    /// </summary>
    public async Task NotifyIdeaSubmitted(int hackathonId, object ideaData)
    {
        try
        {
            await _hubContext.Clients
                .Group($"hackathon_{hackathonId}")
                .SendAsync("IdeaSubmitted", new
                {
                    idea = ideaData,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation($"Notified hackathon {hackathonId} of idea submission");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying idea submission for hackathon {hackathonId}");
        }
    }

    /// <summary>
    /// Notify that comment was added to idea
    /// </summary>
    public async Task NotifyCommentAdded(int hackathonId, int ideaId, object commentData)
    {
        try
        {
            var notification = new
            {
                ideaId = ideaId,
                comment = commentData,
                timestamp = DateTime.UtcNow
            };

            // Notify hackathon group
            await _hubContext.Clients
                .Group($"hackathon_{hackathonId}")
                .SendAsync("CommentAdded", notification);

            // Notify idea watchers
            await _hubContext.Clients
                .Group($"idea_{ideaId}")
                .SendAsync("CommentAdded", notification);

            _logger.LogInformation($"Notified of comment on idea {ideaId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying comment for idea {ideaId}");
        }
    }

    /// <summary>
    /// Notify that rating was submitted for idea
    /// </summary>
    public async Task NotifyRatingSubmitted(int hackathonId, int ideaId, object ratingData)
    {
        try
        {
            var notification = new
            {
                ideaId = ideaId,
                rating = ratingData,
                timestamp = DateTime.UtcNow
            };

            // Notify judges
            await _hubContext.Clients
                .Group($"judging_{hackathonId}")
                .SendAsync("RatingSubmitted", notification);

            // Notify idea watchers
            await _hubContext.Clients
                .Group($"idea_{ideaId}")
                .SendAsync("RatingUpdated", notification);

            _logger.LogInformation($"Notified of rating submission for idea {ideaId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying rating for idea {ideaId}");
        }
    }

    /// <summary>
    /// Notify that idea was deleted
    /// </summary>
    public async Task NotifyIdeaDeleted(int hackathonId, int ideaId)
    {
        try
        {
            await _hubContext.Clients
                .Group($"hackathon_{hackathonId}")
                .SendAsync("IdeaDeleted", new
                {
                    ideaId = ideaId,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation($"Notified of idea {ideaId} deletion");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying idea deletion {ideaId}");
        }
    }

    /// <summary>
    /// Notify that idea status changed
    /// </summary>
    public async Task NotifyIdeaStatusChanged(int hackathonId, int ideaId, string newStatus)
    {
        try
        {
            await _hubContext.Clients
                .Group($"hackathon_{hackathonId}")
                .SendAsync("IdeaStatusChanged", new
                {
                    ideaId = ideaId,
                    status = newStatus,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation($"Notified of status change for idea {ideaId}: {newStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying status change for idea {ideaId}");
        }
    }

    /// <summary>
    /// Notify that judging deadline is approaching
    /// </summary>
    public async Task NotifyJudgingDeadline(int hackathonId, DateTime deadline)
    {
        try
        {
            await _hubContext.Clients
                .Group($"judging_{hackathonId}")
                .SendAsync("JudgingDeadlineApproaching", new
                {
                    deadline = deadline,
                    hoursRemaining = (deadline - DateTime.UtcNow).TotalHours,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation($"Notified judges of deadline for hackathon {hackathonId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying judging deadline for hackathon {hackathonId}");
        }
    }

    /// <summary>
    /// Notify that winner was announced
    /// </summary>
    public async Task NotifyWinnerAnnounced(int hackathonId, object winnerData)
    {
        try
        {
            await _hubContext.Clients
                .Group($"hackathon_{hackathonId}")
                .SendAsync("WinnerAnnounced", new
                {
                    winner = winnerData,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation($"Notified of winner announcement for hackathon {hackathonId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying winner announcement for hackathon {hackathonId}");
        }
    }

    /// <summary>
    /// Notify team member joined
    /// </summary>
    public async Task NotifyTeamMemberJoined(int hackathonId, int teamId, object memberData)
    {
        try
        {
            await _hubContext.Clients
                .Group($"hackathon_{hackathonId}")
                .SendAsync("TeamMemberJoined", new
                {
                    teamId = teamId,
                    member = memberData,
                    timestamp = DateTime.UtcNow
                });

            await _hubContext.Clients
                .Group($"team_{teamId}")
                .SendAsync("TeamMemberJoined", new
                {
                    teamId = teamId,
                    member = memberData,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation($"Notified of member joined team {teamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying team member join for team {teamId}");
        }
    }

    /// <summary>
    /// Notify team member left
    /// </summary>
    public async Task NotifyTeamMemberLeft(int hackathonId, int teamId, string userId)
    {
        try
        {
            await _hubContext.Clients
                .Group($"hackathon_{hackathonId}")
                .SendAsync("TeamMemberLeft", new
                {
                    teamId = teamId,
                    userId = userId,
                    timestamp = DateTime.UtcNow
                });

            await _hubContext.Clients
                .Group($"team_{teamId}")
                .SendAsync("TeamMemberLeft", new
                {
                    teamId = teamId,
                    userId = userId,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation($"Notified of member left team {teamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error notifying team member leave for team {teamId}");
        }
    }
}
