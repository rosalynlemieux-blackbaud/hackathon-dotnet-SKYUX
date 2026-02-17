using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blackbaud.Hackathon.Platform.Shared.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string BlackbaudId { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(255)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public virtual ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

public class UserRole
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int HackathonId { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty; // "participant", "judge", "admin"

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(HackathonId))]
    public virtual Hackathon Hackathon { get; set; } = null!;
}
