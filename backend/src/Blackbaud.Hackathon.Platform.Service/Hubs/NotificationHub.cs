using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Blackbaud.Hackathon.Platform.Service.Hubs;

/// <summary>
/// WebSocket hub for real-time notifications and live updates
/// Handles idea comments, ratings, submissions, and judge presence
/// </summary>
public class NotificationHub : Hub
{
    private static readonly ConcurrentDictionary<string, UserConnection> UserConnections = 
        new ConcurrentDictionary<string, UserConnection>();

    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? "Anonymous";
        var connectionId = Context.ConnectionId;

        var userConnection = new UserConnection
        {
            UserId = userId,
            ConnectionId = connectionId,
            ConnectedAt = DateTime.UtcNow
        };

        UserConnections.TryAdd(connectionId, userConnection);

        _logger.LogInformation($"User {userId} connected with ID {connectionId}");

        // Add user to group for broadcasting (e.g., for a hackathon)
        await Groups.AddToGroupAsync(connectionId, "all_users");

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (UserConnections.TryRemove(Context.ConnectionId, out var userConnection))
        {
            _logger.LogInformation($"User {userConnection.UserId} disconnected");

            // Notify others that judge left (if applicable)
            if (userConnection.HackathonId > 0 && userConnection.IsJudge)
            {
                await Clients
                    .Group($"hackathon_{userConnection.HackathonId}")
                    .SendAsync("JudgeStatusChanged", new
                    {
                        userId = userConnection.UserId,
                        isOnline = false,
                        timestamp = DateTime.UtcNow
                    });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a hackathon-specific group for targeted notifications
    /// </summary>
    public async Task JoinHackathon(int hackathonId)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.FindFirst("sub")?.Value ?? "Anonymous";

        if (UserConnections.TryGetValue(connectionId, out var userConnection))
        {
            userConnection.HackathonId = hackathonId;
        }

        await Groups.AddToGroupAsync(connectionId, $"hackathon_{hackathonId}");
        _logger.LogInformation($"User {userId} joined hackathon {hackathonId}");
    }

    /// <summary>
    /// Leave a hackathon group
    /// </summary>
    public async Task LeaveHackathon(int hackathonId)
    {
        var connectionId = Context.ConnectionId;
        await Groups.RemoveFromGroupAsync(connectionId, $"hackathon_{hackathonId}");
    }

    /// <summary>
    /// Register user as judge joining judging session
    /// </summary>
    public async Task JoinJudging(int hackathonId)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.FindFirst("sub")?.Value ?? "Anonymous";

        if (UserConnections.TryGetValue(connectionId, out var userConnection))
        {
            userConnection.IsJudge = true;
            userConnection.HackathonId = hackathonId;
        }

        await Groups.AddToGroupAsync(connectionId, $"judging_{hackathonId}");

        // Notify other judges that someone came online
        await Clients
            .Group($"judging_{hackathonId}")
            .SendAsync("JudgeOnline", new
            {
                userId = userId,
                timestamp = DateTime.UtcNow
            });

        _logger.LogInformation($"Judge {userId} joined judging session for hackathon {hackathonId}");
    }

    /// <summary>
    /// Notify when someone leaves judging
    /// </summary>
    public async Task LeaveJudging(int hackathonId)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.FindFirst("sub")?.Value ?? "Anonymous";

        await Clients
            .Group($"judging_{hackathonId}")
            .SendAsync("JudgeOffline", new
            {
                userId = userId,
                timestamp = DateTime.UtcNow
            });

        await Groups.RemoveFromGroupAsync(connectionId, $"judging_{hackathonId}");
    }

    /// <summary>
    /// Broadcast idea submission notification
    /// </summary>
    public async Task NotifyIdeaSubmitted(int hackathonId, object ideaData)
    {
        await Clients
            .Group($"hackathon_{hackathonId}")
            .SendAsync("IdeaSubmitted", new
            {
                idea = ideaData,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcast new comment on an idea
    /// </summary>
    public async Task NotifyCommentAdded(int hackathonId, int ideaId, object commentData)
    {
        // Notify hackathon group
        await Clients
            .Group($"hackathon_{hackathonId}")
            .SendAsync("CommentAdded", new
            {
                ideaId = ideaId,
                comment = commentData,
                timestamp = DateTime.UtcNow
            });

        // Also notify on idea-specific group
        await Clients
            .Group($"idea_{ideaId}")
            .SendAsync("CommentAdded", new
            {
                ideaId = ideaId,
                comment = commentData,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcast rating submission
    /// </summary>
    public async Task NotifyRatingSubmitted(int hackathonId, int ideaId, object ratingData)
    {
        // Notify judges
        await Clients
            .Group($"judging_{hackathonId}")
            .SendAsync("RatingSubmitted", new
            {
                ideaId = ideaId,
                rating = ratingData,
                timestamp = DateTime.UtcNow
            });

        // Notify idea followers
        await Clients
            .Group($"idea_{ideaId}")
            .SendAsync("RatingUpdated", new
            {
                ideaId = ideaId,
                rating = ratingData,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Watch specific idea for updates
    /// </summary>
    public async Task WatchIdea(int ideaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"idea_{ideaId}");
    }

    /// <summary>
    /// Stop watching specific idea
    /// </summary>
    public async Task UnwatchIdea(int ideaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"idea_{ideaId}");
    }

    /// <summary>
    /// Notify idea deleted
    /// </summary>
    public async Task NotifyIdeaDeleted(int hackathonId, int ideaId)
    {
        await Clients
            .Group($"hackathon_{hackathonId}")
            .SendAsync("IdeaDeleted", new
            {
                ideaId = ideaId,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notify idea status changed (draft → submitted → accepted)
    /// </summary>
    public async Task NotifyIdeaStatusChanged(int hackathonId, int ideaId, string newStatus)
    {
        await Clients
            .Group($"hackathon_{hackathonId}")
            .SendAsync("IdeaStatusChanged", new
            {
                ideaId = ideaId,
                status = newStatus,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notify judging deadline approaching
    /// </summary>
    public async Task NotifyJudgingDeadline(int hackathonId, DateTime deadline)
    {
        await Clients
            .Group($"judging_{hackathonId}")
            .SendAsync("JudgingDeadlineApproaching", new
            {
                deadline = deadline,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Notify winner announcement
    /// </summary>
    public async Task NotifyWinnerAnnounced(int hackathonId, object winnerData)
    {
        await Clients
            .Group($"hackathon_{hackathonId}")
            .SendAsync("WinnerAnnounced", new
            {
                winner = winnerData,
                timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Get online judge count for hackathon
    /// </summary>
    public async Task GetOnlineJudges(int hackathonId)
    {
        var onlineJudges = UserConnections.Values
            .Where(u => u.HackathonId == hackathonId && u.IsJudge)
            .Select(u => new { userId = u.UserId, connectedAt = u.ConnectedAt })
            .ToList();

        await Clients.Caller.SendAsync("OnlineJudgesList", new
        {
            hackathonId = hackathonId,
            judges = onlineJudges,
            count = onlineJudges.Count
        });
    }
}

/// <summary>
/// Tracks user connection metadata
/// </summary>
public class UserConnection
{
    public string UserId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public int HackathonId { get; set; }
    public bool IsJudge { get; set; }
    public DateTime ConnectedAt { get; set; }
}
