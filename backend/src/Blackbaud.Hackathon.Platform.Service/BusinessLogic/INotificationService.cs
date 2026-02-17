namespace Blackbaud.Hackathon.Platform.Service.BusinessLogic;

/// <summary>
/// Service for managing real-time notifications via SignalR
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notify all users in a hackathon that idea was submitted
    /// </summary>
    Task NotifyIdeaSubmitted(int hackathonId, object ideaData);

    /// <summary>
    /// Notify that comment was added to idea
    /// </summary>
    Task NotifyCommentAdded(int hackathonId, int ideaId, object commentData);

    /// <summary>
    /// Notify that rating was submitted for idea
    /// </summary>
    Task NotifyRatingSubmitted(int hackathonId, int ideaId, object ratingData);

    /// <summary>
    /// Notify that idea was deleted
    /// </summary>
    Task NotifyIdeaDeleted(int hackathonId, int ideaId);

    /// <summary>
    /// Notify that idea status changed
    /// </summary>
    Task NotifyIdeaStatusChanged(int hackathonId, int ideaId, string newStatus);

    /// <summary>
    /// Notify that judging deadline is approaching
    /// </summary>
    Task NotifyJudgingDeadline(int hackathonId, DateTime deadline);

    /// <summary>
    /// Notify that winner was announced
    /// </summary>
    Task NotifyWinnerAnnounced(int hackathonId, object winnerData);

    /// <summary>
    /// Notify team member joined
    /// </summary>
    Task NotifyTeamMemberJoined(int hackathonId, int teamId, object memberData);

    /// <summary>
    /// Notify team member left
    /// </summary>
    Task NotifyTeamMemberLeft(int hackathonId, int teamId, string userId);
}
