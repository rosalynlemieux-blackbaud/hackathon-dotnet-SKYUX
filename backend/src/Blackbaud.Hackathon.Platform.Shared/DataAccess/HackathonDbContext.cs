using Blackbaud.Hackathon.Platform.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Shared.DataAccess;

public class HackathonDbContext : DbContext
{
    public HackathonDbContext(DbContextOptions<HackathonDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Hackathon> Hackathons { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Award> Awards { get; set; }
    public DbSet<JudgingCriterion> JudgingCriteria { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Idea> Ideas { get; set; }
    public DbSet<IdeaAward> IdeaAwards { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.BlackbaudId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // UserRole configurations
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.HackathonId, e.Role }).IsUnique();
        });

        // Hackathon configurations
        modelBuilder.Entity<Models.Hackathon>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
        });

        // Track configurations
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasIndex(e => e.HackathonId);
        });

        // Award configurations
        modelBuilder.Entity<Award>(entity =>
        {
            entity.HasIndex(e => e.HackathonId);
        });

        // JudgingCriterion configurations
        modelBuilder.Entity<JudgingCriterion>(entity =>
        {
            entity.HasIndex(e => e.HackathonId);
        });

        // Milestone configurations
        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.HasIndex(e => e.HackathonId);
        });

        // Team configurations
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasIndex(e => e.HackathonId);
        });

        // TeamMember configurations
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasIndex(e => new { e.TeamId, e.UserId }).IsUnique();
        });

        // Idea configurations
        modelBuilder.Entity<Idea>(entity =>
        {
            entity.HasIndex(e => e.HackathonId);
            entity.HasIndex(e => e.TeamId);
            entity.HasIndex(e => e.Status);
        });

        // IdeaAward configurations
        modelBuilder.Entity<IdeaAward>(entity =>
        {
            entity.HasIndex(e => new { e.IdeaId, e.AwardId }).IsUnique();
        });

        // Rating configurations
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasIndex(e => new { e.IdeaId, e.JudgeId, e.CriterionId }).IsUnique();
        });

        // Comment configurations
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(e => e.IdeaId);
            entity.HasIndex(e => e.ParentCommentId);
        });

        // Configure relationships with cascade delete behavior
        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Idea)
            .WithMany(i => i.Ratings)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Idea)
            .WithMany(i => i.Comments)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
