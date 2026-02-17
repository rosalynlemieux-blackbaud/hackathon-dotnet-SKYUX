namespace Blackbaud.Hackathon.Platform.Service.BusinessLogic;

/// <summary>
/// File service implementation for handling file uploads
/// </summary>
public class FileService : IFileService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly FileUploadSettings _settings;

    public FileService(
        IConfiguration configuration,
        ILogger<FileService> logger,
        IWebHostEnvironment env)
    {
        _configuration = configuration;
        _logger = logger;
        _env = env;
        _settings = configuration.GetSection("FileUpload").Get<FileUploadSettings>() ?? new FileUploadSettings();
    }

    /// <summary>
    /// Upload a file for an idea attachment
    /// </summary>
    public async Task<FileUploadResult> UploadIdeaAttachmentAsync(int ideaId, IFormFile file)
    {
        try
        {
            var validation = ValidateFile(file);
            if (!validation.isValid)
            {
                return new FileUploadResult { Success = false, Error = validation.error };
            }

            var uploadDir = Path.Combine(_env.WebRootPath, _settings.UploadDirectory, $"idea_{ideaId}");
            Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine(_settings.UploadDirectory, $"idea_{ideaId}", uniqueFileName)
                .Replace("\\", "/");

            _logger.LogInformation($"File uploaded for idea {ideaId}: {uniqueFileName}");

            return new FileUploadResult
            {
                Success = true,
                FileName = file.FileName,
                FilePath = relativePath,
                FileSizeBytes = file.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file for idea {ideaId}");
            return new FileUploadResult { Success = false, Error = "Failed to upload file" };
        }
    }

    /// <summary>
    /// Upload a comment attachment
    /// </summary>
    public async Task<FileUploadResult> UploadCommentAttachmentAsync(int commentId, IFormFile file)
    {
        try
        {
            var validation = ValidateFile(file);
            if (!validation.isValid)
            {
                return new FileUploadResult { Success = false, Error = validation.error };
            }

            var uploadDir = Path.Combine(_env.WebRootPath, _settings.UploadDirectory, $"comment_{commentId}");
            Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine(_settings.UploadDirectory, $"comment_{commentId}", uniqueFileName)
                .Replace("\\", "/");

            _logger.LogInformation($"File uploaded for comment {commentId}: {uniqueFileName}");

            return new FileUploadResult
            {
                Success = true,
                FileName = file.FileName,
                FilePath = relativePath,
                FileSizeBytes = file.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file for comment {commentId}");
            return new FileUploadResult { Success = false, Error = "Failed to upload file" };
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                _logger.LogInformation($"File deleted: {filePath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file: {filePath}");
            return false;
        }
    }

    /// <summary>
    /// Get file URL for serving
    /// </summary>
    public string GetFileUrl(string filePath)
    {
        return $"/files/{Uri.EscapeDataString(filePath)}";
    }

    /// <summary>
    /// Validate file is allowed
    /// </summary>
    public (bool isValid, string? error) ValidateFile(IFormFile file)
    {
        // Check file size
        var maxSizeBytes = long.Parse(_settings.MaxFileSizeMb) * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            return (false, $"File size exceeds {_settings.MaxFileSizeMb}MB limit");
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!_settings.AllowedExtensions.Contains(fileExtension))
        {
            return (false, $"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", _settings.AllowedExtensions)}");
        }

        // Check MIME type matches extension to prevent spoofing
        var validMimeTypes = GetValidMimeTypes(fileExtension);
        if (!validMimeTypes.Contains(file.ContentType))
        {
            return (false, "File content doesn't match file extension");
        }

        return (true, null);
    }

    private string[] GetValidMimeTypes(string extension) => extension.ToLower() switch
    {
        ".pdf" => new[] { "application/pdf" },
        ".jpg" or ".jpeg" => new[] { "image/jpeg" },
        ".png" => new[] { "image/png" },
        ".gif" => new[] { "image/gif" },
        ".doc" => new[] { "application/msword" },
        ".docx" => new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        _ => new[] { "application/octet-stream" }
    };
}
