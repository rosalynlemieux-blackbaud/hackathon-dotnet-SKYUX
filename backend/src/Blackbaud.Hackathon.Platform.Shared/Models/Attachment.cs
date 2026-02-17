namespace Blackbaud.Hackathon.Platform.Shared.Models;

/// <summary>
/// File attachment for ideas and comments
/// </summary>
public class Attachment
{
    public int Id { get; set; }
    
    /// <summary>
    /// Optional: Idea this attachment belongs to
    /// </summary>
    public int? IdeaId { get; set; }
    public Idea? Idea { get; set; }
    
    /// <summary>
    /// Optional: Comment this attachment belongs to
    /// </summary>
    public int? CommentId { get; set; }
    public Comment? Comment { get; set; }
    
    /// <summary>
    /// Original file name as uploaded
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Relative path to saved file (from wwwroot)
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME type of file
    /// </summary>
    public string FileType { get; set; } = "application/octet-stream";
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// User who uploaded the file
    /// </summary>
    public int UploadedBy { get; set; }
    public User? UploadedByUser { get; set; }
    
    /// <summary>
    /// When file was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; }
    
    /// <summary>
    /// Display order in attachments list
    /// </summary>
    public int DisplayOrder { get; set; }
}
