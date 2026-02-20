using System.Text.Json;
using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using HackathonModel = Blackbaud.Hackathon.Platform.Shared.Models.Hackathon;

namespace Blackbaud.Hackathon.Platform.Service.DataAccess;

public class DbSeeder
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<DbSeeder> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public DbSeeder(
        HackathonDbContext context,
        ILogger<DbSeeder> logger,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var configuredSeedPath = _configuration["Database:SeedFilePath"];
        var seedPath = string.IsNullOrWhiteSpace(configuredSeedPath)
            ? Path.Combine(_hostEnvironment.ContentRootPath, "database", "seed-data.json")
            : (Path.IsPathRooted(configuredSeedPath)
                ? configuredSeedPath
                : Path.Combine(_hostEnvironment.ContentRootPath, configuredSeedPath));

        _logger.LogInformation("JSON seeding started. Looking for seed file at {SeedPath}", seedPath);

        if (!File.Exists(seedPath))
        {
            _logger.LogWarning("Seed file not found at {SeedPath}. Skipping seed.", seedPath);
            return;
        }

        var seedText = await File.ReadAllTextAsync(seedPath, cancellationToken);
        using var doc = JsonDocument.Parse(seedText);
        var root = doc.RootElement;

        var targetHackathonNames = new List<string>();
        if (root.TryGetProperty("hackathons", out var targetHackathons) && targetHackathons.ValueKind == JsonValueKind.Array)
        {
            foreach (var hackathonItem in targetHackathons.EnumerateArray())
            {
                var name = GetString(hackathonItem, "name");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    targetHackathonNames.Add(name);
                }
            }
        }

        if (targetHackathonNames.Count > 0)
        {
            var alreadySeeded = await _context.Ideas
                .Include(i => i.Hackathon)
                .AnyAsync(i => targetHackathonNames.Contains(i.Hackathon.Name), cancellationToken);

            if (alreadySeeded)
            {
                _logger.LogInformation("Seed skipped: target hackathon data already contains ideas.");
                return;
            }
        }

        var hackathonIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var userIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var trackIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var awardIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var criterionIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var teamIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var ideaIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (root.TryGetProperty("hackathons", out var hackathonsElement) && hackathonsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in hackathonsElement.EnumerateArray())
            {
                var sourceId = GetString(item, "id");
                if (string.IsNullOrWhiteSpace(sourceId))
                {
                    continue;
                }

                var registrationOpen = GetDateTime(item, "registration_open") ?? DateTime.UtcNow;
                var submissionDeadline = GetDateTime(item, "submission_deadline") ?? registrationOpen.AddDays(7);
                var judgingStart = GetDateTime(item, "judging_start") ?? submissionDeadline;
                var judgingEnd = GetDateTime(item, "judging_end") ?? judgingStart.AddDays(2);
                var hackathonName = GetString(item, "name") ?? "Hackathon";

                var existingHackathon = await _context.Hackathons
                    .FirstOrDefaultAsync(h => h.Name == hackathonName, cancellationToken);

                if (existingHackathon is not null)
                {
                    hackathonIdMap[sourceId] = existingHackathon.Id;
                    continue;
                }

                var hackathon = new HackathonModel
                {
                    Name = hackathonName,
                    Description = GetString(item, "description") ?? string.Empty,
                    Status = ResolveStatus(GetString(item, "status")),
                    RegistrationStart = registrationOpen,
                    RegistrationEnd = submissionDeadline,
                    StartDate = registrationOpen,
                    EndDate = submissionDeadline,
                    JudgingStart = judgingStart,
                    JudgingEnd = judgingEnd,
                    WinnersAnnouncement = judgingEnd,
                    Rules = GetString(item, "rules"),
                    Faq = item.TryGetProperty("faq", out var faq) ? faq.GetRawText() : null,
                    MaxTeamSize = 6,
                    AllowLateSubmissions = false,
                    IsPublic = true,
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow,
                    UpdatedAt = GetDateTime(item, "updated_at")
                };

                _context.Hackathons.Add(hackathon);
                await _context.SaveChangesAsync(cancellationToken);
                hackathonIdMap[sourceId] = hackathon.Id;
            }
        }

        if (hackathonIdMap.Count == 0)
        {
            _logger.LogWarning("Seed file contains no usable hackathons. Skipping seed.");
            return;
        }

        var defaultHackathonId = hackathonIdMap.Values.First();

        if (root.TryGetProperty("profiles", out var profilesElement) && profilesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in profilesElement.EnumerateArray())
            {
                var sourceUserId = GetString(item, "user_id");
                if (string.IsNullOrWhiteSpace(sourceUserId) || userIdMap.ContainsKey(sourceUserId))
                {
                    continue;
                }

                var email = (GetString(item, "email") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(email))
                {
                    email = $"{sourceUserId}@seed.local";
                }

                var blackbaudId = GetString(item, "blackbaud_id");
                if (string.IsNullOrWhiteSpace(blackbaudId))
                {
                    blackbaudId = $"seed-{sourceUserId}";
                }

                var firstName = GetString(item, "first_name");
                var lastName = GetString(item, "last_name");

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    SplitName(GetString(item, "name"), out var parsedFirst, out var parsedLast);
                    firstName ??= parsedFirst;
                    lastName ??= parsedLast;
                }

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(
                        u => u.BlackbaudId == blackbaudId || u.Email == email,
                        cancellationToken);

                if (existingUser is not null)
                {
                    userIdMap[sourceUserId] = existingUser.Id;
                    continue;
                }

                var user = new User
                {
                    BlackbaudId = blackbaudId,
                    Email = email,
                    FirstName = string.IsNullOrWhiteSpace(firstName) ? "Unknown" : firstName,
                    LastName = string.IsNullOrWhiteSpace(lastName) ? "User" : lastName,
                    AvatarUrl = GetString(item, "avatar"),
                    IsActive = !GetBool(item, "banned"),
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow,
                    UpdatedAt = GetDateTime(item, "updated_at")
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
                userIdMap[sourceUserId] = user.Id;
            }
        }

        if (root.TryGetProperty("tracks", out var tracksElement) && tracksElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in tracksElement.EnumerateArray())
            {
                var sourceId = GetString(item, "id");
                var sourceHackathonId = GetString(item, "hackathon_id");

                if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(sourceHackathonId) || !hackathonIdMap.TryGetValue(sourceHackathonId, out var mappedHackathonId))
                {
                    continue;
                }

                var track = new Track
                {
                    HackathonId = mappedHackathonId,
                    Name = GetString(item, "name") ?? "Track",
                    Description = GetString(item, "description"),
                    Color = null,
                    DisplayOrder = GetInt(item, "sort_order") ?? 0,
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow
                };

                _context.Tracks.Add(track);
                await _context.SaveChangesAsync(cancellationToken);
                trackIdMap[sourceId] = track.Id;
            }
        }

        if (root.TryGetProperty("awards", out var awardsElement) && awardsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in awardsElement.EnumerateArray())
            {
                var sourceId = GetString(item, "id");
                var sourceHackathonId = GetString(item, "hackathon_id");

                if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(sourceHackathonId) || !hackathonIdMap.TryGetValue(sourceHackathonId, out var mappedHackathonId))
                {
                    continue;
                }

                var award = new Award
                {
                    HackathonId = mappedHackathonId,
                    Name = GetString(item, "name") ?? "Award",
                    Description = GetString(item, "description"),
                    Icon = GetString(item, "icon"),
                    DisplayOrder = GetInt(item, "sort_order") ?? 0,
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow
                };

                _context.Awards.Add(award);
                await _context.SaveChangesAsync(cancellationToken);
                awardIdMap[sourceId] = award.Id;
            }
        }

        if (root.TryGetProperty("judging_criteria", out var criteriaElement) && criteriaElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in criteriaElement.EnumerateArray())
            {
                var sourceId = GetString(item, "id");
                var sourceHackathonId = GetString(item, "hackathon_id");

                if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(sourceHackathonId) || !hackathonIdMap.TryGetValue(sourceHackathonId, out var mappedHackathonId))
                {
                    continue;
                }

                var criterion = new JudgingCriterion
                {
                    HackathonId = mappedHackathonId,
                    Name = GetString(item, "name") ?? "Criterion",
                    Description = GetString(item, "description"),
                    Weight = GetDecimal(item, "weight") ?? 1m,
                    MaxScore = GetInt(item, "max_score") ?? 10,
                    DisplayOrder = GetInt(item, "sort_order") ?? 0,
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow
                };

                _context.JudgingCriteria.Add(criterion);
                await _context.SaveChangesAsync(cancellationToken);
                criterionIdMap[sourceId] = criterion.Id;
            }
        }

        if (root.TryGetProperty("milestones", out var milestonesElement) && milestonesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in milestonesElement.EnumerateArray())
            {
                var sourceHackathonId = GetString(item, "hackathon_id");
                if (string.IsNullOrWhiteSpace(sourceHackathonId) || !hackathonIdMap.TryGetValue(sourceHackathonId, out var mappedHackathonId))
                {
                    continue;
                }

                var milestone = new Milestone
                {
                    HackathonId = mappedHackathonId,
                    Name = GetString(item, "title") ?? "Milestone",
                    Description = GetString(item, "description"),
                    DueDate = GetDateTime(item, "date") ?? DateTime.UtcNow,
                    IsComplete = GetBool(item, "is_completed"),
                    DisplayOrder = GetInt(item, "sort_order") ?? 0,
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow
                };

                _context.Milestones.Add(milestone);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        if (root.TryGetProperty("user_roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in rolesElement.EnumerateArray())
            {
                var sourceUserId = GetString(item, "user_id");
                var role = GetString(item, "role");

                if (string.IsNullOrWhiteSpace(sourceUserId) || string.IsNullOrWhiteSpace(role) || !userIdMap.TryGetValue(sourceUserId, out var mappedUserId))
                {
                    continue;
                }

                var userRole = new UserRole
                {
                    UserId = mappedUserId,
                    HackathonId = defaultHackathonId,
                    Role = role,
                    AssignedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow
                };

                var roleExists = await _context.UserRoles.AnyAsync(
                    r => r.UserId == userRole.UserId && r.HackathonId == userRole.HackathonId && r.Role == userRole.Role,
                    cancellationToken);

                if (!roleExists)
                {
                    _context.UserRoles.Add(userRole);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        if (root.TryGetProperty("teams", out var teamsElement) && teamsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in teamsElement.EnumerateArray())
            {
                var sourceId = GetString(item, "id");
                var sourceLeaderId = GetString(item, "leader_id");

                if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(sourceLeaderId) || !userIdMap.TryGetValue(sourceLeaderId, out var mappedLeaderId))
                {
                    continue;
                }

                var team = new Team
                {
                    HackathonId = defaultHackathonId,
                    Name = GetString(item, "name") ?? "Team",
                    Description = GetString(item, "description"),
                    ImageUrl = GetString(item, "image_url"),
                    LeaderId = mappedLeaderId,
                    IsLookingForMembers = false,
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow,
                    UpdatedAt = GetDateTime(item, "updated_at")
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync(cancellationToken);
                teamIdMap[sourceId] = team.Id;
            }
        }

        if (root.TryGetProperty("team_members", out var membersElement) && membersElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in membersElement.EnumerateArray())
            {
                var sourceTeamId = GetString(item, "team_id");
                var sourceUserId = GetString(item, "user_id");

                if (string.IsNullOrWhiteSpace(sourceTeamId) || string.IsNullOrWhiteSpace(sourceUserId)
                    || !teamIdMap.TryGetValue(sourceTeamId, out var mappedTeamId)
                    || !userIdMap.TryGetValue(sourceUserId, out var mappedUserId))
                {
                    continue;
                }

                var teamMember = new TeamMember
                {
                    TeamId = mappedTeamId,
                    UserId = mappedUserId,
                    JoinedAt = GetDateTime(item, "joined_at") ?? DateTime.UtcNow
                };

                _context.TeamMembers.Add(teamMember);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        var fallbackTeamId = teamIdMap.Values.FirstOrDefault();

        if (root.TryGetProperty("ideas", out var ideasElement) && ideasElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in ideasElement.EnumerateArray())
            {
                var sourceId = GetString(item, "id");
                var sourceHackathonId = GetString(item, "hackathon_id");
                var sourceAuthorId = GetString(item, "author_id");

                if (string.IsNullOrWhiteSpace(sourceId)
                    || string.IsNullOrWhiteSpace(sourceHackathonId)
                    || string.IsNullOrWhiteSpace(sourceAuthorId)
                    || !hackathonIdMap.TryGetValue(sourceHackathonId, out var mappedHackathonId)
                    || !userIdMap.TryGetValue(sourceAuthorId, out var mappedAuthorId))
                {
                    continue;
                }

                var sourceTeamId = GetString(item, "team_id");
                if (!string.IsNullOrWhiteSpace(sourceTeamId) && !teamIdMap.TryGetValue(sourceTeamId, out _))
                {
                    sourceTeamId = null;
                }

                var mappedTeamId = !string.IsNullOrWhiteSpace(sourceTeamId)
                    ? teamIdMap[sourceTeamId]
                    : fallbackTeamId;

                if (mappedTeamId == 0)
                {
                    continue;
                }

                var sourceTrackId = GetString(item, "track_id");
                var mappedTrackId = !string.IsNullOrWhiteSpace(sourceTrackId) && trackIdMap.TryGetValue(sourceTrackId, out var trackId)
                    ? trackId
                    : (int?)null;

                var idea = new Idea
                {
                    HackathonId = mappedHackathonId,
                    TeamId = mappedTeamId,
                    TrackId = mappedTrackId,
                    SubmittedBy = mappedAuthorId,
                    Title = GetString(item, "title") ?? "Idea",
                    Description = GetString(item, "description") ?? string.Empty,
                    Status = ResolveIdeaStatus(GetString(item, "status")),
                    DemoUrl = GetString(item, "demo_url"),
                    RepositoryUrl = GetString(item, "repo_url"),
                    VideoUrl = GetString(item, "video_url"),
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow,
                    UpdatedAt = GetDateTime(item, "updated_at"),
                    SubmittedAt = GetDateTime(item, "submitted_at")
                };

                _context.Ideas.Add(idea);
                await _context.SaveChangesAsync(cancellationToken);
                ideaIdMap[sourceId] = idea.Id;

                if (item.TryGetProperty("award_ids", out var awardIds) && awardIds.ValueKind == JsonValueKind.Array)
                {
                    foreach (var awardIdElement in awardIds.EnumerateArray())
                    {
                        var sourceAwardId = awardIdElement.GetString();
                        if (string.IsNullOrWhiteSpace(sourceAwardId) || !awardIdMap.TryGetValue(sourceAwardId, out var mappedAwardId))
                        {
                            continue;
                        }

                        _context.IdeaAwards.Add(new IdeaAward
                        {
                            IdeaId = idea.Id,
                            AwardId = mappedAwardId,
                            AwardedAt = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        if (root.TryGetProperty("idea_ratings", out var ratingsElement) && ratingsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in ratingsElement.EnumerateArray())
            {
                var sourceIdeaId = GetString(item, "idea_id");
                var sourceJudgeId = GetString(item, "judge_id");

                if (string.IsNullOrWhiteSpace(sourceIdeaId)
                    || string.IsNullOrWhiteSpace(sourceJudgeId)
                    || !ideaIdMap.TryGetValue(sourceIdeaId, out var mappedIdeaId)
                    || !userIdMap.TryGetValue(sourceJudgeId, out var mappedJudgeId)
                    || !item.TryGetProperty("scores", out var scoresElement)
                    || scoresElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var feedback = GetString(item, "overall_feedback");
                var createdAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow;
                var updatedAt = GetDateTime(item, "updated_at");

                foreach (var score in scoresElement.EnumerateArray())
                {
                    var sourceCriterionId = GetString(score, "criterionId");
                    if (string.IsNullOrWhiteSpace(sourceCriterionId) || !criterionIdMap.TryGetValue(sourceCriterionId, out var mappedCriterionId))
                    {
                        continue;
                    }

                    _context.Ratings.Add(new Rating
                    {
                        IdeaId = mappedIdeaId,
                        JudgeId = mappedJudgeId,
                        CriterionId = mappedCriterionId,
                        Score = GetInt(score, "score") ?? 0,
                        Feedback = feedback,
                        CreatedAt = createdAt,
                        UpdatedAt = updatedAt
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        if (root.TryGetProperty("comments", out var commentsElement) && commentsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in commentsElement.EnumerateArray())
            {
                var sourceIdeaId = GetString(item, "idea_id");
                var sourceUserId = GetString(item, "user_id");
                var content = GetString(item, "content") ?? GetString(item, "message");

                if (string.IsNullOrWhiteSpace(sourceIdeaId)
                    || string.IsNullOrWhiteSpace(sourceUserId)
                    || string.IsNullOrWhiteSpace(content)
                    || !ideaIdMap.TryGetValue(sourceIdeaId, out var mappedIdeaId)
                    || !userIdMap.TryGetValue(sourceUserId, out var mappedUserId))
                {
                    continue;
                }

                _context.Comments.Add(new Comment
                {
                    IdeaId = mappedIdeaId,
                    UserId = mappedUserId,
                    Content = content,
                    CreatedAt = GetDateTime(item, "created_at") ?? DateTime.UtcNow,
                    UpdatedAt = GetDateTime(item, "updated_at")
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Seed completed from JSON file {SeedPath}. Imported Hackathons={Hackathons}, Users={Users}, Teams={Teams}, Ideas={Ideas}.",
            seedPath,
            _context.Hackathons.Count(),
            _context.Users.Count(),
            _context.Teams.Count(),
            _context.Ideas.Count());
    }

    private static string? GetString(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
    }

    private static int? GetInt(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue))
        {
            return intValue;
        }

        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out intValue))
        {
            return intValue;
        }

        return null;
    }

    private static decimal? GetDecimal(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var decimalValue))
        {
            return decimalValue;
        }

        if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), out decimalValue))
        {
            return decimalValue;
        }

        return null;
    }

    private static bool GetBool(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return false;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
            _ => false
        };
    }

    private static DateTime? GetDateTime(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(value.GetString(), out var dto))
        {
            return dto.UtcDateTime;
        }

        return null;
    }

    private static string ResolveStatus(string? sourceStatus)
    {
        if (string.IsNullOrWhiteSpace(sourceStatus))
        {
            return "active";
        }

        var value = sourceStatus.Trim().ToLowerInvariant();
        return value switch
        {
            "upcoming" => "upcoming",
            "active" => "active",
            "judging" => "judging",
            "completed" => "completed",
            _ => "active"
        };
    }

    private static string ResolveIdeaStatus(string? sourceStatus)
    {
        if (string.IsNullOrWhiteSpace(sourceStatus))
        {
            return "draft";
        }

        var value = sourceStatus.Trim().ToLowerInvariant();
        return value switch
        {
            "draft" => "draft",
            "submitted" => "submitted",
            "under_review" => "under_review",
            "winner" => "winner",
            _ => "draft"
        };
    }

    private static void SplitName(string? fullName, out string firstName, out string lastName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            firstName = "Unknown";
            lastName = "User";
            return;
        }

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            firstName = parts[0];
            lastName = "User";
            return;
        }

        firstName = parts[0];
        lastName = string.Join(' ', parts.Skip(1));
    }
}
