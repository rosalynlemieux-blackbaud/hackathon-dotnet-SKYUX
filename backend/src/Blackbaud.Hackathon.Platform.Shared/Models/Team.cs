using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blackbaud.Hackathon.Platform.Shared.Models;

public class Team
{
    [Key]
    public int Id { get; set; }

    public int HackathonId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public int LeaderId { get; set; }

    public bool IsLookingForMembers { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(HackathonId))]
    public virtual Hackathon Hackathon { get; set; } = null!;

    [ForeignKey(nameof(LeaderId))]
    public virtual User Leader { get; set; } = null!;

    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    public virtual ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public class TeamMember
{
    [Key]
    public int Id { get; set; }

    public int TeamId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TeamId))]
    public virtual Team Team { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
