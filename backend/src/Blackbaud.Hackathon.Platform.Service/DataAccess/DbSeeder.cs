using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Service.DataAccess;

public class DbSeeder
{
    private readonly HackathonDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(HackathonDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if database is already seeded
            if (await _context.Hackathons.AnyAsync(h => h.Name == "Off the Grid 2025"))
            {
                _logger.LogInformation("Database already seeded. Skipping seed operation.");
                return;
            }

            _logger.LogInformation("Starting database seeding for Off the Grid 2025...");

            await SeedUsers();
            await SeedHackathon();
            await SeedIdeas();
            await SeedTeams();
            await SeedComments();
            await SeedRatings();
            await SeedAwards();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    private async Task SeedUsers()
    {
        var users = new List<User>
        {
            // Admin
            new User
            {
                Email = "admin@blackbaud.com",
                FirstName = "Sarah",
                LastName = "Administrator",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },

            // Judges
            new User
            {
                Email = "judge1@blackbaud.com",
                FirstName = "Michael",
                LastName = "Chen",
                Role = "Judge",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new User
            {
                Email = "judge2@blackbaud.com",
                FirstName = "Jennifer",
                LastName = "Rodriguez",
                Role = "Judge",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new User
            {
                Email = "judge3@blackbaud.com",
                FirstName = "David",
                LastName = "Patel",
                Role = "Judge",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },

            // Participants
            new User
            {
                Email = "alex.johnson@blackbaud.com",
                FirstName = "Alex",
                LastName = "Johnson",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Email = "emma.williams@blackbaud.com",
                FirstName = "Emma",
                LastName = "Williams",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new User
            {
                Email = "marcus.thompson@blackbaud.com",
                FirstName = "Marcus",
                LastName = "Thompson",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Email = "sophia.garcia@blackbaud.com",
                FirstName = "Sophia",
                LastName = "Garcia",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-23)
            },
            new User
            {
                Email = "liam.martinez@blackbaud.com",
                FirstName = "Liam",
                LastName = "Martinez",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Email = "olivia.anderson@blackbaud.com",
                FirstName = "Olivia",
                LastName = "Anderson",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new User
            {
                Email = "noah.thomas@blackbaud.com",
                FirstName = "Noah",
                LastName = "Thomas",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Email = "ava.jackson@blackbaud.com",
                FirstName = "Ava",
                LastName = "Jackson",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new User
            {
                Email = "ethan.white@blackbaud.com",
                FirstName = "Ethan",
                LastName = "White",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Email = "isabella.harris@blackbaud.com",
                FirstName = "Isabella",
                LastName = "Harris",
                Role = "Participant",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {users.Count} users");
    }

    private async Task SeedHackathon()
    {
        var hackathon = new Hackathon
        {
            Name = "Off the Grid 2025",
            Description = "Blackbaud's premier innovation challenge focused on sustainability, renewable energy, and off-grid solutions. Teams will design and prototype solutions that enable communities to thrive independently of traditional infrastructure.",
            Theme = "Sustainable Off-Grid Solutions",
            StartDate = DateTime.UtcNow.AddDays(-14),
            EndDate = DateTime.UtcNow.AddDays(14),
            JudgingStartDate = DateTime.UtcNow.AddDays(15),
            JudgingEndDate = DateTime.UtcNow.AddDays(21),
            Status = "active",
            MaxTeamSize = 5,
            CreatedBy = 1, // Admin
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        _context.Hackathons.Add(hackathon);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded Off the Grid 2025 hackathon");
    }

    private async Task SeedIdeas()
    {
        var ideas = new List<Idea>
        {
            new Idea
            {
                Title = "Solar-Powered Water Purification System",
                Description = @"An innovative water purification system powered entirely by solar energy, designed for remote communities without access to clean water or electricity.

**Key Features:**
- Modular solar panel array (500W)
- UV sterilization + multi-stage filtration
- Battery backup for nighttime operation
- Purifies 1000L per day
- Mobile app for remote monitoring
- Maintenance alerts via SMS

**Impact:** Can serve communities of 100-200 people with clean drinking water, reducing waterborne diseases by 90%.",
                AuthorId = 5, // Alex Johnson
                HackathonId = 1,
                Status = "submitted",
                TeamId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Idea
            {
                Title = "Wind-Powered IoT Mesh Network",
                Description = @"A self-sustaining communication network using wind turbines and mesh topology to connect remote areas.

**Key Features:**
- Micro wind turbines (100W each)
- LoRa-based mesh networking
- Solar backup power
- 50km range between nodes
- Emergency broadcast system
- Weather-resistant hardware

**Impact:** Enables communication in disaster zones and remote communities where traditional infrastructure fails.",
                AuthorId = 6, // Emma Williams
                HackathonId = 1,
                Status = "submitted",
                TeamId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Idea
            {
                Title = "Biogas Generator for Rural Kitchens",
                Description = @"Compact biogas generator that converts organic waste into cooking fuel for rural households.

**Key Features:**
- 2m³ digester tank
- Processes 5kg organic waste daily
- Produces 2 hours of cooking gas
- Fertilizer byproduct
- Mobile app for gas level monitoring
- Easy installation in small spaces

**Impact:** Reduces deforestation from firewood collection and provides clean cooking fuel for families.",
                AuthorId = 7, // Marcus Thompson
                HackathonId = 1,
                Status = "submitted",
                TeamId = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Idea
            {
                Title = "Off-Grid Cold Storage for Farmers",
                Description = @"Solar-powered cold storage unit designed to preserve produce for smallholder farmers in remote areas.

**Key Features:**
- 500L capacity refrigeration
- Solar thermal + PV hybrid
- Maintains 4°C constantly
- IoT temperature monitoring
- Automated alerts
- 7-day battery backup

**Impact:** Reduces post-harvest losses by 70%, increasing farmer income by $2000/year.",
                AuthorId = 8, // Sophia Garcia
                HackathonId = 1,
                Status = "submitted",
                TeamId = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new Idea
            {
                Title = "Portable Renewable Energy Backpack",
                Description = @"All-in-one renewable energy solution in a backpack for emergency responders and outdoor professionals.

**Key Features:**
- Foldable 60W solar panels
- 300Wh battery pack
- USB-C PD, AC outlet, 12V car port
- Built-in LED work light
- Waterproof design
- Weighs only 3kg

**Impact:** Provides reliable power for emergency communications, medical equipment, and rescue operations.",
                AuthorId = 9, // Liam Martinez
                HackathonId = 1,
                Status = "submitted",
                TeamId = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new Idea
            {
                Title = "Micro-Hydro Power for Mountain Communities",
                Description = @"Small-scale hydroelectric system that harnesses stream flow in mountainous regions.

**Key Features:**
- 1-5kW turbine capacity
- Minimal environmental impact
- Works with low flow rates (10L/s)
- Smart grid integration
- Community battery storage
- Easy to install and maintain

**Impact:** Provides 24/7 electricity to remote mountain villages, powering schools and clinics.",
                AuthorId = 10, // Olivia Anderson
                HackathonId = 1,
                Status = "submitted",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new Idea
            {
                Title = "AI-Powered Energy Management for Microgrids",
                Description = @"Machine learning system that optimizes energy distribution in off-grid microgrids.

**Key Features:**
- Predictive load balancing
- Weather forecasting integration
- Battery life optimization
- Automatic source switching
- Mobile dashboard
- Reduces waste by 30%

**Impact:** Maximizes renewable energy utilization and reduces reliance on diesel generators.",
                AuthorId = 11, // Noah Thomas
                HackathonId = 1,
                Status = "submitted",
                TeamId = 6,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new Idea
            {
                Title = "Composting Toilet with Biochar Production",
                Description = @"Waterless sanitation system that produces biochar fertilizer from human waste.

**Key Features:**
- No water required
- Odorless operation
- Produces 5kg biochar/month
- Solar-powered ventilation
- Modular design
- Easy maintenance

**Impact:** Provides dignified sanitation while creating valuable soil amendment for agriculture.",
                AuthorId = 12, // Ava Jackson
                HackathonId = 1,
                Status = "draft",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        _context.Ideas.AddRange(ideas);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {ideas.Count} ideas");
    }

    private async Task SeedTeams()
    {
        var teams = new List<Team>
        {
            new Team
            {
                Name = "Aqua Innovators",
                Description = "Clean water solutions for underserved communities",
                HackathonId = 1,
                LeaderId = 5, // Alex Johnson
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Team
            {
                Name = "WindTech Collective",
                Description = "Harnessing wind power for connectivity",
                HackathonId = 1,
                LeaderId = 6, // Emma Williams
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new Team
            {
                Name = "BioEnergy Squad",
                Description = "Converting waste to energy",
                HackathonId = 1,
                LeaderId = 7, // Marcus Thompson
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Team
            {
                Name = "ColdChain Champions",
                Description = "Preserving food and medicine off-grid",
                HackathonId = 1,
                LeaderId = 8, // Sophia Garcia
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Team
            {
                Name = "MobilePower Pioneers",
                Description = "Portable renewable energy solutions",
                HackathonId = 1,
                LeaderId = 9, // Liam Martinez
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new Team
            {
                Name = "GridSmart AI",
                Description = "Intelligent energy management systems",
                HackathonId = 1,
                LeaderId = 11, // Noah Thomas
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        _context.Teams.AddRange(teams);
        await _context.SaveChangesAsync();

        // Add team members
        var teamMembers = new List<TeamMember>
        {
            // Aqua Innovators (Team 1)
            new TeamMember { TeamId = 1, UserId = 5, Role = "Lead", JoinedAt = DateTime.UtcNow.AddDays(-10) },
            new TeamMember { TeamId = 1, UserId = 13, Role = "Developer", JoinedAt = DateTime.UtcNow.AddDays(-9) },
            
            // WindTech Collective (Team 2)
            new TeamMember { TeamId = 2, UserId = 6, Role = "Lead", JoinedAt = DateTime.UtcNow.AddDays(-9) },
            new TeamMember { TeamId = 2, UserId = 14, Role = "Engineer", JoinedAt = DateTime.UtcNow.AddDays(-8) },
            
            // BioEnergy Squad (Team 3)
            new TeamMember { TeamId = 3, UserId = 7, Role = "Lead", JoinedAt = DateTime.UtcNow.AddDays(-8) },
            
            // ColdChain Champions (Team 4)
            new TeamMember { TeamId = 4, UserId = 8, Role = "Lead", JoinedAt = DateTime.UtcNow.AddDays(-7) },
            
            // MobilePower Pioneers (Team 5)
            new TeamMember { TeamId = 5, UserId = 9, Role = "Lead", JoinedAt = DateTime.UtcNow.AddDays(-6) },
            
            // GridSmart AI (Team 6)
            new TeamMember { TeamId = 6, UserId = 11, Role = "Lead", JoinedAt = DateTime.UtcNow.AddDays(-4) }
        };

        _context.TeamMembers.AddRange(teamMembers);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {teams.Count} teams with {teamMembers.Count} members");
    }

    private async Task SeedComments()
    {
        var comments = new List<Comment>
        {
            // Comments on Solar Water Purification
            new Comment
            {
                IdeaId = 1,
                UserId = 2, // Judge Michael
                Content = "Impressive solution! The combination of UV sterilization and filtration is well thought out. Have you considered the maintenance requirements for remote areas?",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Comment
            {
                IdeaId = 1,
                UserId = 6, // Emma
                Content = "Love the mobile app integration! This could really help with preventive maintenance.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            
            // Comments on Wind-Powered IoT
            new Comment
            {
                IdeaId = 2,
                UserId = 3, // Judge Jennifer
                Content = "The mesh networking approach is brilliant for disaster recovery. What's the latency for emergency broadcasts?",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Comment
            {
                IdeaId = 2,
                UserId = 7, // Marcus
                Content = "This could integrate well with our biogas monitoring system. Great work!",
                CreatedAt = DateTime.UtcNow.AddHours(-18)
            },
            
            // Comments on Biogas Generator
            new Comment
            {
                IdeaId = 3,
                UserId = 4, // Judge David
                Content = "The dual benefit of cooking gas and fertilizer is excellent. How do you handle gas storage safety?",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            
            // Comments on Cold Storage
            new Comment
            {
                IdeaId = 4,
                UserId = 2, // Judge Michael
                Content = "The ROI calculation is compelling. The 7-day battery backup is impressive - how did you achieve this?",
                CreatedAt = DateTime.UtcNow.AddHours(-20)
            },
            new Comment
            {
                IdeaId = 4,
                UserId = 10, // Olivia
                Content = "This could be transformative for rural farmers. The temperature monitoring feature is crucial.",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            
            // Comments on Renewable Backpack
            new Comment
            {
                IdeaId = 5,
                UserId = 3, // Judge Jennifer
                Content = "Perfect for emergency response! The weight-to-power ratio is excellent. Have you field-tested this?",
                CreatedAt = DateTime.UtcNow.AddHours(-15)
            },
            
            // Comments on Micro-Hydro
            new Comment
            {
                IdeaId = 6,
                UserId = 4, // Judge David
                Content = "The low flow rate capability is impressive. This opens up many more sites for deployment.",
                CreatedAt = DateTime.UtcNow.AddHours(-10)
            },
            
            // Comments on AI Energy Management
            new Comment
            {
                IdeaId = 7,
                UserId = 2, // Judge Michael
                Content = "The 30% waste reduction is significant. What data sources feed into the predictive model?",
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new Comment
            {
                IdeaId = 7,
                UserId = 5, // Alex
                Content = "This AI approach could help optimize our solar water system too. Excellent innovation!",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        _context.Comments.AddRange(comments);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {comments.Count} comments");
    }

    private async Task SeedRatings()
    {
        var ratings = new List<Rating>
        {
            // Ratings for Solar Water Purification (Idea 1)
            new Rating { IdeaId = 1, JudgeId = 2, Score = 9, Feedback = "Excellent execution with strong real-world impact. The technical approach is sound and scalable.", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Rating { IdeaId = 1, JudgeId = 3, Score = 8, Feedback = "Great solution addressing a critical need. Could benefit from more detail on deployment logistics.", CreatedAt = DateTime.UtcNow.AddHours(-20) },
            new Rating { IdeaId = 1, JudgeId = 4, Score = 9, Feedback = "Impressive combination of technologies. The monitoring system adds significant value.", CreatedAt = DateTime.UtcNow.AddHours(-18) },
            
            // Ratings for Wind-Powered IoT (Idea 2)
            new Rating { IdeaId = 2, JudgeId = 2, Score = 8, Feedback = "Innovative use of mesh networking. The disaster recovery application is particularly compelling.", CreatedAt = DateTime.UtcNow.AddHours(-22) },
            new Rating { IdeaId = 2, JudgeId = 3, Score = 9, Feedback = "Outstanding technical solution. The range and reliability make this very practical.", CreatedAt = DateTime.UtcNow.AddHours(-19) },
            new Rating { IdeaId = 2, JudgeId = 4, Score = 8, Feedback = "Strong engineering approach. Weather resistance is crucial - glad you addressed it.", CreatedAt = DateTime.UtcNow.AddHours(-16) },
            
            // Ratings for Biogas Generator (Idea 3)
            new Rating { IdeaId = 3, JudgeId = 2, Score = 7, Feedback = "Solid concept with clear benefits. Would like to see more on user experience and adoption.", CreatedAt = DateTime.UtcNow.AddHours(-21) },
            new Rating { IdeaId = 3, JudgeId = 3, Score = 8, Feedback = "Addresses both energy and agricultural needs elegantly. The compact design is well thought out.", CreatedAt = DateTime.UtcNow.AddHours(-17) },
            new Rating { IdeaId = 3, JudgeId = 4, Score = 7, Feedback = "Good environmental impact. Safety considerations could be expanded.", CreatedAt = DateTime.UtcNow.AddHours(-14) },
            
            // Ratings for Cold Storage (Idea 4)
            new Rating { IdeaId = 4, JudgeId = 2, Score = 9, Feedback = "Excellent economic analysis. This could be a game-changer for smallholder farmers.", CreatedAt = DateTime.UtcNow.AddHours(-15) },
            new Rating { IdeaId = 4, JudgeId = 3, Score = 8, Feedback = "Strong value proposition. The IoT integration enables great monitoring capabilities.", CreatedAt = DateTime.UtcNow.AddHours(-13) },
            
            // Ratings for Renewable Backpack (Idea 5)
            new Rating { IdeaId = 5, JudgeId = 2, Score = 7, Feedback = "Practical and portable. The weight optimization is commendable.", CreatedAt = DateTime.UtcNow.AddHours(-12) },
            new Rating { IdeaId = 5, JudgeId = 3, Score = 8, Feedback = "Perfect for the target use case. The multiple output options increase versatility.", CreatedAt = DateTime.UtcNow.AddHours(-10) },
            
            // Ratings for Micro-Hydro (Idea 6)
            new Rating { IdeaId = 6, JudgeId = 2, Score = 8, Feedback = "Reliable 24/7 power is a huge advantage. Low environmental impact is well considered.", CreatedAt = DateTime.UtcNow.AddHours(-8) },
            new Rating { IdeaId = 6, JudgeId = 4, Score = 9, Feedback = "Exceptional work on making this viable for low flow rates. Opens many deployment opportunities.", CreatedAt = DateTime.UtcNow.AddHours(-6) },
            
            // Ratings for AI Energy Management (Idea 7)
            new Rating { IdeaId = 7, JudgeId = 2, Score = 9, Feedback = "Cutting-edge application of AI to optimize renewable energy. The 30% efficiency gain is impressive.", CreatedAt = DateTime.UtcNow.AddHours(-4) },
            new Rating { IdeaId = 7, JudgeId = 3, Score = 8, Feedback = "Smart approach to a complex problem. The predictive capabilities add significant value.", CreatedAt = DateTime.UtcNow.AddHours(-3) }
        };

        _context.Ratings.AddRange(ratings);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {ratings.Count} ratings");
    }

    private async Task SeedAwards()
    {
        var awards = new List<IdeaAward>
        {
            new IdeaAward
            {
                IdeaId = 1, // Solar Water Purification
                AwardName = "Best Overall Solution",
                AwardAmount = 5000,
                AwardedAt = DateTime.UtcNow.AddHours(-1)
            },
            new IdeaAward
            {
                IdeaId = 2, // Wind-Powered IoT
                AwardName = "Innovation Award",
                AwardAmount = 3000,
                AwardedAt = DateTime.UtcNow.AddHours(-1)
            },
            new IdeaAward
            {
                IdeaId = 7, // AI Energy Management
                AwardName = "Technical Excellence Award",
                AwardAmount = 2500,
                AwardedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _context.IdeaAwards.AddRange(awards);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Seeded {awards.Count} awards");
    }
}
