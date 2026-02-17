using Blackbaud.Hackathon.Platform.Service.BusinessLogic;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blackbaud.Hackathon.Platform.Service.Tests.BusinessLogic;

public class NotificationServiceTests
{
    private readonly Mock<IHubContext<Hubs.NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _notificationService;
    private readonly Mock<IClientProxy> _mockClientProxy;

    public NotificationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<Hubs.NotificationHub>>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _mockClientProxy = new Mock<IClientProxy>();
        
        // Setup HubContext to return mock client proxy
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        
        _notificationService = new NotificationService(_mockHubContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task NotifyIdeaSubmitted_Should_SendNotificationToHackathonGroup()
    {
        // Arrange
        var hackathonId = 1;
        var ideaData = new { id = 1, title = "Test Idea", authorName = "John Doe" };

        // Act
        await _notificationService.NotifyIdeaSubmitted(hackathonId, ideaData);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"hackathon_{hackathonId}"),
            Times.Once
        );
        
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "IdeaSubmitted",
                It.Is<object[]>(o => o.Length == 1 && o[0] == ideaData),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyIdeaCommented_Should_SendNotificationToIdeaWatchers()
    {
        // Arrange
        var ideaId = 1;
        var commentData = new { id = 1, content = "Great idea!", authorName = "Jane Doe" };

        // Act
        await _notificationService.NotifyIdeaCommented(ideaId, commentData);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"idea_{ideaId}"),
            Times.Once
        );
        
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "IdeaCommented",
                It.Is<object[]>(o => o.Length == 1 && o[0] == commentData),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyIdeaRated_Should_SendNotificationToJudgingGroup()
    {
        // Arrange
        var hackathonId = 1;
        var ideaId = 1;
        var ratingData = new { ideaId = 1, score = 8, judgeName = "Judge Smith" };

        // Act
        await _notificationService.NotifyIdeaRated(hackathonId, ideaId, ratingData);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"judging_{hackathonId}"),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyTeamMemberJoined_Should_SendNotificationToTeamGroup()
    {
        // Arrange
        var teamId = 1;
        var memberData = new { userId = 2, userName = "New Member" };

        // Act
        await _notificationService.NotifyTeamMemberJoined(teamId, memberData);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"team_{teamId}"),
            Times.Once
        );
        
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "TeamMemberJoined",
                It.Is<object[]>(o => o.Length == 1 && o[0] == memberData),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyWinnerAnnounced_Should_SendNotificationToHackathonGroup()
    {
        // Arrange
        var hackathonId = 1;
        var winnerData = new { ideaId = 1, ideaTitle = "Winning Idea", awardName = "Best Overall" };

        // Act
        await _notificationService.NotifyWinnerAnnounced(hackathonId, winnerData);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"hackathon_{hackathonId}"),
            Times.Once
        );
        
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "WinnerAnnounced",
                It.Is<object[]>(o => o.Length == 1 && o[0] == winnerData),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task BroadcastToHackathon_Should_SendCustomEventToGroup()
    {
        // Arrange
        var hackathonId = 1;
        var eventName = "CustomEvent";
        var eventData = new { message = "Custom message" };

        // Act
        await _notificationService.BroadcastToHackathon(hackathonId, eventName, eventData);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"hackathon_{hackathonId}"),
            Times.Once
        );
        
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                eventName,
                It.Is<object[]>(o => o.Length == 1 && o[0] == eventData),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyJudgeAssigned_Should_SendNotificationToJudgingGroup()
    {
        // Arrange
        var hackathonId = 1;
        var judgeId = 5;

        // Act
        await _notificationService.NotifyJudgeAssigned(hackathonId, judgeId);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"judging_{hackathonId}"),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyDeadlineApproaching_Should_SendNotificationToHackathonGroup()
    {
        // Arrange
        var hackathonId = 1;
        var deadline = DateTime.UtcNow.AddHours(24);

        // Act
        await _notificationService.NotifyDeadlineApproaching(hackathonId, deadline);

        // Assert
        _mockHubContext.Verify(
            h => h.Clients.Group($"hackathon_{hackathonId}"),
            Times.Once
        );
        
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "DeadlineApproaching",
                It.IsAny<object[]>(),
                default
            ),
            Times.Once
        );
    }
}
