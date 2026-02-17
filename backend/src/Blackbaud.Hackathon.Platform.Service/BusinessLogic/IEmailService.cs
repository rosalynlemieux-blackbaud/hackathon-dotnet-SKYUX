namespace Blackbaud.Hackathon.Platform.Service.BusinessLogic;

/// <summary>
/// Email template models for different notification types
/// </summary>
public enum EmailTemplateType
{
    IdeaSubmitted,
    IdeaCommented,
    IdeaRated,
    JudgingAssigned,
    JudgingDeadlineReminder,
    WinnerAnnounced,
    TeamInvitation
}

public class EmailTemplate
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string PlainTextBody { get; set; } = string.Empty;
}

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send idea submission confirmation email
    /// </summary>
    Task SendIdeaSubmissionEmailAsync(string toEmail, string ideaTitle, string ideaLink);

    /// <summary>
    /// Send notification about new comment
    /// </summary>
    Task SendCommentNotificationEmailAsync(string toEmail, string ideaTitle, string commentAuthor, string commentPreview, string ideaLink);

    /// <summary>
    /// Send notification about new rating
    /// </summary>
    Task SendRatingNotificationEmailAsync(string toEmail, string ideaTitle, int score, string ideaLink);

    /// <summary>
    /// Send judging assignment email
    /// </summary>
    Task SendJudgingAssignmentEmailAsync(string toEmail, string hackathonName, int ideaCount, string judgingLink);

    /// <summary>
    /// Send judging deadline reminder
    /// </summary>
    Task SendJudgingDeadlineReminderEmailAsync(string toEmail, string hackathonName, DateTime deadline, int ideasPending);

    /// <summary>
    /// Send winner announcement email
    /// </summary>
    Task SendWinnerAnnouncementEmailAsync(string toEmail, string ideaTitle, string awardName, string ideaLink);

    /// <summary>
    /// Send team invitation email
    /// </summary>
    Task SendTeamInvitationEmailAsync(string toEmail, string teamName, string inviterName, string acceptLink);

    /// <summary>
    /// Generate email template with placeholders filled
    /// </summary>
    EmailTemplate GenerateTemplate(EmailTemplateType templateType, Dictionary<string, string> data);
}
