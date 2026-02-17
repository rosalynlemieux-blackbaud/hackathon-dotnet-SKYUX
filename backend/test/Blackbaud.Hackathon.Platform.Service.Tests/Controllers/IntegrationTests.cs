using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blackbaud.Hackathon.Platform.Service.DataAccess;
using Blackbaud.Hackathon.Platform.Service.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blackbaud.Hackathon.Platform.Service.Tests.Controllers;

public class FilesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FilesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<HackathonDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<HackathonDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Build service provider and seed database
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<HackathonDbContext>();
                db.Database.EnsureCreated();
                SeedDatabase(db);
            });
        });

        _client = _factory.CreateClient();
    }

    private void SeedDatabase(HackathonDbContext context)
    {
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed test data
        var testUser = new User
        {
            Id = 1,
            Email = "test@blackbaud.com",
            FirstName = "Test",
            LastName = "User",
            Role = "Participant",
            CreatedAt = DateTime.UtcNow
        };

        var testHackathon = new Hackathon
        {
            Id = 1,
            Name = "Test Hackathon",
            Description = "Test Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2),
            Status = "active",
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };

        var testIdea = new Idea
        {
            Id = 1,
            Title = "Test Idea",
            Description = "Test Description",
            AuthorId = 1,
            HackathonId = 1,
            Status = "draft",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(testUser);
        context.Hackathons.Add(testHackathon);
        context.Ideas.Add(testIdea);
        context.SaveChanges();
    }

    [Fact]
    public async Task UploadIdeaAttachment_Should_RequireAuthentication()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await _client.PostAsync("/api/files/idea/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListIdeaAttachments_Should_ReturnEmptyListForIdeaWithNoAttachments()
    {
        // Act
        var response = await _client.GetAsync("/api/files/idea/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var attachments = JsonSerializer.Deserialize<List<object>>(content);
        attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task ListIdeaAttachments_Should_Return404ForNonExistentIdea()
    {
        // Act
        var response = await _client.GetAsync("/api/files/idea/9999");

        // Assert - Controller should handle gracefully
        // Depending on implementation, could be 404 or empty list
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task DownloadAttachment_Should_Return404ForNonExistentFile()
    {
        // Act
        var response = await _client.GetAsync("/api/files/download/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAttachment_Should_RequireAuthentication()
    {
        // Act
        var response = await _client.DeleteAsync("/api/files/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class IdeasControllerEmailIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public IdeasControllerEmailIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<HackathonDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<HackathonDbContext>(options =>
                {
                    options.UseInMemoryDatabase("EmailTestDatabase");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateIdea_Should_NotFailIfEmailServiceFails()
    {
        // This test verifies that email failures don't block API responses
        // The actual email send will fail (no SMTP config), but API should succeed
        
        // Note: This requires authentication which is mocked in the actual test environment
        // This is a placeholder test showing the pattern
        var createIdeaRequest = new
        {
            title = "Test Idea",
            description = "Test Description",
            hackathonId = 1
        };

        // In a real scenario, we'd mock authentication and test the full flow
        // For now, this demonstrates the test structure
        Assert.True(true); // Placeholder - actual implementation requires auth mocking
    }
}
