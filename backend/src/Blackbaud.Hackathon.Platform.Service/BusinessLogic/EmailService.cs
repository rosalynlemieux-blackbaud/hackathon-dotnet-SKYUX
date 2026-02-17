using System.Net;
using System.Net.Mail;
using System.Text;

namespace Blackbaud.Hackathon.Platform.Service.BusinessLogic;

/// <summary>
/// Email service implementation using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var emailSettings = configuration.GetSection("Email");
        _fromEmail = emailSettings["FromAddress"] ?? "noreply@blackbaud-hackathon.com";
        _fromName = emailSettings["FromName"] ?? "Blackbaud Hackathon";

        var smtpServer = emailSettings["SmtpServer"] ?? "localhost";
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var smtpUsername = emailSettings["SmtpUsername"];
        var smtpPassword = emailSettings["SmtpPassword"];

        _smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
        {
            _smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
        }
    }

    /// <summary>
    /// Send idea submission confirmation email
    /// </summary>
    public async Task SendIdeaSubmissionEmailAsync(string toEmail, string ideaTitle, string ideaLink)
    {
        var data = new Dictionary<string, string>
        {
            { "{{ideaTitle}}", ideaTitle },
            { "{{ideaLink}}", ideaLink }
        };

        var template = GenerateTemplate(EmailTemplateType.IdeaSubmitted, data);
        await SendEmailAsync(toEmail, template.Subject, template.HtmlBody, template.PlainTextBody);
    }

    /// <summary>
    /// Send notification about new comment
    /// </summary>
    public async Task SendCommentNotificationEmailAsync(string toEmail, string ideaTitle, string commentAuthor, string commentPreview, string ideaLink)
    {
        var data = new Dictionary<string, string>
        {
            { "{{ideaTitle}}", ideaTitle },
            { "{{commentAuthor}}", commentAuthor },
            { "{{commentPreview}}", commentPreview },
            { "{{ideaLink}}", ideaLink }
        };

        var template = GenerateTemplate(EmailTemplateType.IdeaCommented, data);
        await SendEmailAsync(toEmail, template.Subject, template.HtmlBody, template.PlainTextBody);
    }

    /// <summary>
    /// Send notification about new rating
    /// </summary>
    public async Task SendRatingNotificationEmailAsync(string toEmail, string ideaTitle, int score, string ideaLink)
    {
        var data = new Dictionary<string, string>
        {
            { "{{ideaTitle}}", ideaTitle },
            { "{{score}}", score.ToString() },
            { "{{ideaLink}}", ideaLink }
        };

        var template = GenerateTemplate(EmailTemplateType.IdeaRated, data);
        await SendEmailAsync(toEmail, template.Subject, template.HtmlBody, template.PlainTextBody);
    }

    /// <summary>
    /// Send judging assignment email
    /// </summary>
    public async Task SendJudgingAssignmentEmailAsync(string toEmail, string hackathonName, int ideaCount, string judgingLink)
    {
        var data = new Dictionary<string, string>
        {
            { "{{hackathonName}}", hackathonName },
            { "{{ideaCount}}", ideaCount.ToString() },
            { "{{judgingLink}}", judgingLink }
        };

        var template = GenerateTemplate(EmailTemplateType.JudgingAssigned, data);
        await SendEmailAsync(toEmail, template.Subject, template.HtmlBody, template.PlainTextBody);
    }

    /// <summary>
    /// Send judging deadline reminder
    /// </summary>
    public async Task SendJudgingDeadlineReminderEmailAsync(string toEmail, string hackathonName, DateTime deadline, int ideasPending)
    {
        var data = new Dictionary<string, string>
        {
            { "{{hackathonName}}", hackathonName },
            { "{{deadline}}", deadline.ToString("MMMM d, yyyy h:mm tt") },
            { "{{ideasPending}}", ideasPending.ToString() },
            { "{{hoursRemaining}}", ((deadline - DateTime.UtcNow).TotalHours).ToString("F1") }
        };

        var template = GenerateTemplate(EmailTemplateType.JudgingDeadlineReminder, data);
        await SendEmailAsync(toEmail, template.Subject, template.HtmlBody, template.PlainTextBody);
    }

    /// <summary>
    /// Send winner announcement email
    /// </summary>
    public async Task SendWinnerAnnouncementEmailAsync(string toEmail, string ideaTitle, string awardName, string ideaLink)
    {
        var data = new Dictionary<string, string>
        {
            { "{{ideaTitle}}", ideaTitle },
            { "{{awardName}}", awardName },
            { "{{ideaLink}}", ideaLink }
        };

        var template = GenerateTemplate(EmailTemplateType.WinnerAnnounced, data);
        await SendEmailAsync(toEmail, template.Subject, template.HtmlBody, template.PlainTextBody);
    }

    /// <summary>
    /// Send team invitation email
    /// </summary>
    public async Task SendTeamInvitationEmailAsync(string toEmail, string teamName, string inviterName, string acceptLink)
    {
        var data = new Dictionary<string, string>
        {
            { "{{teamName}}", teamName },
            { "{{inviterName}}", inviterName },
            { "{{acceptLink}}", acceptLink }
        };

        var template = GenerateTemplate(EmailTemplateType.TeamInvitation, data);
        await SendEmailAsync(toEmail, template.Subject, template.HtmlBody, template.PlainTextBody);
    }

    /// <summary>
    /// Generate email template with placeholders filled
    /// </summary>
    public EmailTemplate GenerateTemplate(EmailTemplateType templateType, Dictionary<string, string> data)
    {
        var template = templateType switch
        {
            EmailTemplateType.IdeaSubmitted => GenerateIdeaSubmittedTemplate(data),
            EmailTemplateType.IdeaCommented => GenerateIdeaCommentedTemplate(data),
            EmailTemplateType.IdeaRated => GenerateIdeaRatedTemplate(data),
            EmailTemplateType.JudgingAssigned => GenerateJudgingAssignedTemplate(data),
            EmailTemplateType.JudgingDeadlineReminder => GenerateJudgingDeadlineTemplate(data),
            EmailTemplateType.WinnerAnnounced => GenerateWinnerAnnouncedTemplate(data),
            EmailTemplateType.TeamInvitation => GenerateTeamInvitationTemplate(data),
            _ => throw new ArgumentException($"Unknown template type: {templateType}")
        };

        return template;
    }

    /// <summary>
    /// Send email via SMTP
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string plainTextBody)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = plainTextBody,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            // Add HTML alternative view
            var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
            message.AlternateViews.Add(htmlView);

            await _smtpClient.SendMailAsync(message);
            _logger.LogInformation($"Email sent to {toEmail}: {subject}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {toEmail}: {subject}");
        }
    }

    #region Template Generators

    private EmailTemplate GenerateIdeaSubmittedTemplate(Dictionary<string, string> data)
    {
        var ideaTitle = GetData(data, "{{ideaTitle}}");
        var ideaLink = GetData(data, "{{ideaLink}}");

        return new EmailTemplate
        {
            Subject = $"✅ Your Idea Was Submitted: {ideaTitle}",
            HtmlBody = $@"
                <h2>Idea Submitted Successfully!</h2>
                <p>Your idea <strong>{ideaTitle}</strong> has been submitted for judging.</p>
                <p><a href='{ideaLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Your Idea</a></p>
                <p>The judges will review and rate your idea. You'll receive email notifications for comments and ratings.</p>
                <p>Find updates on your idea in your dashboard.</p>
            ",
            PlainTextBody = $@"Idea Submitted Successfully!

Your idea '{ideaTitle}' has been submitted for judging.

View your idea: {ideaLink}

The judges will review and rate your idea. You'll receive email notifications for comments and ratings."
        };
    }

    private EmailTemplate GenerateIdeaCommentedTemplate(Dictionary<string, string> data)
    {
        var ideaTitle = GetData(data, "{{ideaTitle}}");
        var commentAuthor = GetData(data, "{{commentAuthor}}");
        var commentPreview = GetData(data, "{{commentPreview}}");
        var ideaLink = GetData(data, "{{ideaLink}}");

        return new EmailTemplate
        {
            Subject = $"💬 New Comment on: {ideaTitle}",
            HtmlBody = $@"
                <h2>New Comment on Your Idea</h2>
                <p><strong>{commentAuthor}</strong> commented on your idea <strong>{ideaTitle}</strong>:</p>
                <div style='background-color: #f5f5f5; padding: 15px; border-left: 4px solid #17a2b8; margin: 15px 0;'>
                    <p>{commentPreview}</p>
                </div>
                <p><a href='{ideaLink}' style='background-color: #17a2b8; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Comment</a></p>
            ",
            PlainTextBody = $@"New Comment on Your Idea

{commentAuthor} commented on your idea '{ideaTitle}':

{commentPreview}

View comment: {ideaLink}"
        };
    }

    private EmailTemplate GenerateIdeaRatedTemplate(Dictionary<string, string> data)
    {
        var ideaTitle = GetData(data, "{{ideaTitle}}");
        var score = GetData(data, "{{score}}");
        var ideaLink = GetData(data, "{{ideaLink}}");

        return new EmailTemplate
        {
            Subject = $"⭐ Your Idea Was Rated: {ideaTitle}",
            HtmlBody = $@"
                <h2>Your Idea Was Rated</h2>
                <p>A judge has rated your idea <strong>{ideaTitle}</strong> with a score of <strong>{score}/10</strong>.</p>
                <p><a href='{ideaLink}' style='background-color: #ffc107; color: #333; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Ratings</a></p>
            ",
            PlainTextBody = $@"Your Idea Was Rated

A judge has rated your idea '{ideaTitle}' with a score of {score}/10.

View ratings: {ideaLink}"
        };
    }

    private EmailTemplate GenerateJudgingAssignedTemplate(Dictionary<string, string> data)
    {
        var hackathonName = GetData(data, "{{hackathonName}}");
        var ideaCount = GetData(data, "{{ideaCount}}");
        var judgingLink = GetData(data, "{{judgingLink}}");

        return new EmailTemplate
        {
            Subject = $"🔍 You're assigned as a judge for {hackathonName}",
            HtmlBody = $@"
                <h2>Judging Assignment</h2>
                <p>You've been assigned as a judge for <strong>{hackathonName}</strong>.</p>
                <p>There are <strong>{ideaCount} ideas</strong> awaiting your judgment.</p>
                <p>Please review and rate each idea based on the provided criteria.</p>
                <p><a href='{judgingLink}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Start Judging</a></p>
            ",
            PlainTextBody = $@"Judging Assignment

You've been assigned as a judge for {hackathonName}.

There are {ideaCount} ideas awaiting your judgment.

Please review and rate each idea based on the provided criteria.

Start judging: {judgingLink}"
        };
    }

    private EmailTemplate GenerateJudgingDeadlineTemplate(Dictionary<string, string> data)
    {
        var hackathonName = GetData(data, "{{hackathonName}}");
        var deadline = GetData(data, "{{deadline}}");
        var ideasPending = GetData(data, "{{ideasPending}}");
        var hoursRemaining = GetData(data, "{{hoursRemaining}}");

        return new EmailTemplate
        {
            Subject = $"⏰ Judging Deadline Reminder: {hoursRemaining} hours left",
            HtmlBody = $@"
                <h2>Judging Deadline Approaching</h2>
                <p>⚠️ The judging deadline for <strong>{hackathonName}</strong> is in <strong>{hoursRemaining} hours</strong>.</p>
                <p>You have <strong>{ideasPending} ideas</strong> still pending judgment.</p>
                <p>Deadline: <strong>{deadline}</strong></p>
                <p>Please complete your ratings before the deadline.</p>
            ",
            PlainTextBody = $@"Judging Deadline Approaching

The judging deadline for {hackathonName} is in {hoursRemaining} hours.

You have {ideasPending} ideas still pending judgment.

Deadline: {deadline}

Please complete your ratings before the deadline."
        };
    }

    private EmailTemplate GenerateWinnerAnnouncedTemplate(Dictionary<string, string> data)
    {
        var ideaTitle = GetData(data, "{{ideaTitle}}");
        var awardName = GetData(data, "{{awardName}}");
        var ideaLink = GetData(data, "{{ideaLink}}");

        return new EmailTemplate
        {
            Subject = $"🏆 Congratulations! Your Idea Won: {awardName}",
            HtmlBody = $@"
                <h2>🏆 Winner Announcement!</h2>
                <p><strong>Congratulations!</strong> Your idea <strong>{ideaTitle}</strong> won the <strong>{awardName}</strong> award!</p>
                <p>View your winning idea and join us in celebrating this achievement.</p>
                <p><a href='{ideaLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>View Award</a></p>
            ",
            PlainTextBody = $@"🏆 Winner Announcement!

Congratulations! Your idea '{ideaTitle}' won the {awardName} award!

View your winning idea: {ideaLink}"
        };
    }

    private EmailTemplate GenerateTeamInvitationTemplate(Dictionary<string, string> data)
    {
        var teamName = GetData(data, "{{teamName}}");
        var inviterName = GetData(data, "{{inviterName}}");
        var acceptLink = GetData(data, "{{acceptLink}}");

        return new EmailTemplate
        {
            Subject = $"👥 {inviterName} invited you to join team: {teamName}",
            HtmlBody = $@"
                <h2>Team Invitation</h2>
                <p><strong>{inviterName}</strong> invited you to join the team <strong>{teamName}</strong></p>
                <p>Click below to accept the invitation and start collaborating!</p>
                <p><a href='{acceptLink}' style='background-color: #6f42c1; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Accept Invitation</a></p>
            ",
            PlainTextBody = $@"Team Invitation

{inviterName} invited you to join the team {teamName}

Click below to accept the invitation and start collaborating!

Accept invitation: {acceptLink}"
        };
    }

    private string GetData(Dictionary<string, string> data, string key) => 
        data.ContainsKey(key) ? data[key] : string.Empty;

    #endregion
}
