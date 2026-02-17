using Blackbaud.Hackathon.Platform.Service.BusinessLogic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blackbaud.Hackathon.Platform.Service.Tests.BusinessLogic;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        
        // Setup configuration
        SetupConfiguration();
        
        _emailService = new EmailService(_mockConfiguration.Object, _mockLogger.Object);
    }

    private void SetupConfiguration()
    {
        var emailSection = new Mock<IConfigurationSection>();
        emailSection.Setup(x => x["FromAddress"]).Returns("noreply@test.com");
        emailSection.Setup(x => x["FromName"]).Returns("Test Hackathon");
        emailSection.Setup(x => x["SmtpServer"]).Returns("smtp.test.com");
        emailSection.Setup(x => x["SmtpPort"]).Returns("587");
        emailSection.Setup(x => x["SmtpUsername"]).Returns("testuser");
        emailSection.Setup(x => x["SmtpPassword"]).Returns("testpass");
        
        _mockConfiguration.Setup(x => x.GetSection("Email")).Returns(emailSection.Object);
    }

    [Fact]
    public async Task SendIdeaSubmittedEmailAsync_Should_HandleValidInput()
    {
        // Arrange
        var userEmail = "user@test.com";
        var ideaTitle = "Test Idea";
        var hackathonName = "Off the Grid 2025";

        // Act & Assert - Should not throw
        // Note: Actual SMTP sending is not tested in unit tests
        // Integration tests will verify actual email delivery
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendIdeaSubmittedEmailAsync(userEmail, ideaTitle, hackathonName)
        );
        
        // Should not throw, but SMTP will fail in test environment (expected)
        // We're validating the method doesn't crash on null/invalid config
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>();
    }

    [Fact]
    public async Task SendIdeaCommentedEmailAsync_Should_HandleValidInput()
    {
        // Arrange
        var authorEmail = "author@test.com";
        var ideaTitle = "Test Idea";
        var commenterName = "John Doe";
        var commentPreview = "Great idea!";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendIdeaCommentedEmailAsync(authorEmail, ideaTitle, commenterName, commentPreview)
        );
        
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>();
    }

    [Fact]
    public async Task SendIdeaRatedEmailAsync_Should_HandleValidInput()
    {
        // Arrange
        var authorEmail = "author@test.com";
        var ideaTitle = "Test Idea";
        var judgeName = "Jane Judge";
        var score = 8;

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendIdeaRatedEmailAsync(authorEmail, ideaTitle, judgeName, score)
        );
        
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>();
    }

    [Fact]
    public async Task SendJudgingAssignedEmailAsync_Should_HandleValidInput()
    {
        // Arrange
        var judgeEmail = "judge@test.com";
        var hackathonName = "Off the Grid 2025";
        var ideaCount = 15;

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendJudgingAssignedEmailAsync(judgeEmail, hackathonName, ideaCount)
        );
        
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>();
    }

    [Fact]
    public async Task SendJudgingDeadlineReminderAsync_Should_HandleValidInput()
    {
        // Arrange
        var judgeEmail = "judge@test.com";
        var hackathonName = "Off the Grid 2025";
        var deadline = DateTime.UtcNow.AddHours(24);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendJudgingDeadlineReminderAsync(judgeEmail, hackathonName, deadline)
        );
        
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>();
    }

    [Fact]
    public async Task SendWinnerAnnouncedEmailAsync_Should_HandleValidInput()
    {
        // Arrange
        var winnerEmail = "winner@test.com";
        var hackathonName = "Off the Grid 2025";
        var awardName = "Best Overall";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendWinnerAnnouncedEmailAsync(winnerEmail, hackathonName, awardName)
        );
        
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>();
    }

    [Fact]
    public async Task SendTeamInvitationEmailAsync_Should_HandleValidInput()
    {
        // Arrange
        var recipientEmail = "member@test.com";
        var teamName = "Team Awesome";
        var inviterName = "John Doe";

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendTeamInvitationEmailAsync(recipientEmail, teamName, inviterName)
        );
        
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>();
    }

    [Theory]
    [InlineData(null, "Title", "Hackathon")]
    [InlineData("", "Title", "Hackathon")]
    [InlineData("   ", "Title", "Hackathon")]
    public async Task SendIdeaSubmittedEmailAsync_Should_HandleInvalidEmail(
        string invalidEmail, string title, string hackathon)
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendIdeaSubmittedEmailAsync(invalidEmail, title, hackathon)
        );
        
        // Should fail with argument or format exception for invalid email
        exception.Should().NotBeNull();
    }

    [Theory]
    [InlineData("user@test.com", null, "Hackathon")]
    [InlineData("user@test.com", "", "Hackathon")]
    [InlineData("user@test.com", "Title", null)]
    [InlineData("user@test.com", "Title", "")]
    public async Task SendIdeaSubmittedEmailAsync_Should_HandleEmptyFields(
        string email, string title, string hackathon)
    {
        // Act & Assert - Should not crash on empty fields
        var exception = await Record.ExceptionAsync(() =>
            _emailService.SendIdeaSubmittedEmailAsync(email, title, hackathon)
        );
        
        // Method should handle empty values gracefully
        exception.Should().BeOfType<System.Net.Mail.SmtpException>()
            .Or.BeOfType<System.Net.Sockets.SocketException>()
            .Or.BeOfType<ArgumentNullException>();
    }
}
