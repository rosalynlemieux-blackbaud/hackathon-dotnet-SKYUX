namespace Blackbaud.Hackathon.Platform.Service.BusinessLogic;

public class FileUploadSettings
{
    public string MaxFileSizeMb { get; set; } = "10";
    public string UploadDirectory { get; set; } = "wwwroot/uploads";
    public string[] AllowedExtensions { get; set; } = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".doc", ".docx" };
}

public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? Error { get; set; }
    public long FileSizeBytes { get; set; }
}

/// <summary>
/// Service for handling file uploads
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Upload a file for an idea attachment
    /// </summary>
    Task<FileUploadResult> UploadIdeaAttachmentAsync(int ideaId, IFormFile file);

    /// <summary>
    /// Upload a comment attachment
    /// </summary>
    Task<FileUploadResult> UploadCommentAttachmentAsync(int commentId, IFormFile file);

    /// <summary>
    /// Delete a file
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// Get file URL for serving
    /// </summary>
    string GetFileUrl(string filePath);

    /// <summary>
    /// Validate file is allowed
    /// </summary>
    (bool isValid, string? error) ValidateFile(IFormFile file);
}
