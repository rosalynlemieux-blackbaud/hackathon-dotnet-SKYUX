using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blackbaud.Hackathon.Platform.Shared.Models;

public class Rating
{
    [Key]
    public int Id { get; set; }

    public int IdeaId { get; set; }

    public int JudgeId { get; set; }

    public int CriterionId { get; set; }

    public int Score { get; set; }

    public string? Feedback { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(IdeaId))]
    public virtual Idea Idea { get; set; } = null!;

    [ForeignKey(nameof(JudgeId))]
    public virtual User Judge { get; set; } = null!;

    [ForeignKey(nameof(CriterionId))]
    public virtual JudgingCriterion Criterion { get; set; } = null!;
}

public class Comment
{
    [Key]
    public int Id { get; set; }

    public int IdeaId { get; set; }

    public int UserId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(IdeaId))]
    public virtual Idea Idea { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(ParentCommentId))]
    public virtual Comment? ParentComment { get; set; }

    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
