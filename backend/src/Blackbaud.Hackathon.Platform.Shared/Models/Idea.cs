using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blackbaud.Hackathon.Platform.Shared.Models;

public class Idea
{
    [Key]
    public int Id { get; set; }

    public int HackathonId { get; set; }

    public int TeamId { get; set; }

    public int? TrackId { get; set; }

    public int SubmittedBy { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = "draft"; // "draft", "submitted", "under_review", "winner"

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    public string? DemoUrl { get; set; }

    [StringLength(500)]
    public string? RepositoryUrl { get; set; }

    [StringLength(500)]
    public string? VideoUrl { get; set; }

    public string? TechnicalDetails { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(HackathonId))]
    public virtual Hackathon Hackathon { get; set; } = null!;

    [ForeignKey(nameof(TeamId))]
    public virtual Team Team { get; set; } = null!;

    [ForeignKey(nameof(TrackId))]
    public virtual Track? Track { get; set; }

    [ForeignKey(nameof(SubmittedBy))]
    public virtual User SubmittedByUser { get; set; } = null!;

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<IdeaAward> IdeaAwards { get; set; } = new List<IdeaAward>();
}

public class IdeaAward
{
    [Key]
    public int Id { get; set; }

    public int IdeaId { get; set; }

    public int AwardId { get; set; }

    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(IdeaId))]
    public virtual Idea Idea { get; set; } = null!;

    [ForeignKey(nameof(AwardId))]
    public virtual Award Award { get; set; } = null!;
}
