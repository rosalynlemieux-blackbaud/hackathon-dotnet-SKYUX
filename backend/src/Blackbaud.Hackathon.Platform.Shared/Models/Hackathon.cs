using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blackbaud.Hackathon.Platform.Shared.Models;

public class Hackathon
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = "upcoming"; // "upcoming", "active", "judging", "completed"

    public DateTime RegistrationStart { get; set; }

    public DateTime RegistrationEnd { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime JudgingStart { get; set; }

    public DateTime JudgingEnd { get; set; }

    public DateTime WinnersAnnouncement { get; set; }

    public string? Rules { get; set; }

    public string? Faq { get; set; }

    public int MaxTeamSize { get; set; } = 5;

    public bool AllowLateSubmissions { get; set; } = false;

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
    public virtual ICollection<Award> Awards { get; set; } = new List<Award>();
    public virtual ICollection<JudgingCriterion> JudgingCriteria { get; set; } = new List<JudgingCriterion>();
    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Track
{
    [Key]
    public int Id { get; set; }

    public int HackathonId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(HackathonId))]
    public virtual Hackathon Hackathon { get; set; } = null!;

    public virtual ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public class Award
{
    [Key]
    public int Id { get; set; }

    public int HackathonId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(HackathonId))]
    public virtual Hackathon Hackathon { get; set; } = null!;

    public virtual ICollection<IdeaAward> IdeaAwards { get; set; } = new List<IdeaAward>();
}

public class JudgingCriterion
{
    [Key]
    public int Id { get; set; }

    public int HackathonId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Weight { get; set; } = 1.0m;

    public int MaxScore { get; set; } = 5;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(HackathonId))]
    public virtual Hackathon Hackathon { get; set; } = null!;

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}

public class Milestone
{
    [Key]
    public int Id { get; set; }

    public int HackathonId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public bool IsComplete { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(HackathonId))]
    public virtual Hackathon Hackathon { get; set; } = null!;
}
