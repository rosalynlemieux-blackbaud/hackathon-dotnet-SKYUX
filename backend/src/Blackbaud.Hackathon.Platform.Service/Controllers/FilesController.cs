using Blackbaud.Hackathon.Platform.Shared.DataAccess;
using Blackbaud.Hackathon.Platform.Shared.Models;
using Blackbaud.Hackathon.Platform.Service.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blackbaud.Hackathon.Platform.Service.Controllers;

/// <summary>
/// File upload and download endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ParticipantOnly")]
public class FilesController : ControllerBase
{
    private readonly HackathonDbContext _context;
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        HackathonDbContext context,
        IFileService fileService,
        ILogger<FilesController> logger)
    {
        _context = context;
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Upload attachment for idea
    /// </summary>
    [HttpPost("idea/{ideaId}")]
    public async Task<IActionResult> UploadIdeaAttachment(int ideaId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        // Verify idea exists and user has access
        var idea = await _context.Ideas.FindAsync(ideaId);
        if (idea == null)
            return NotFound("Idea not found");

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (idea.SubmittedBy != userId)
            return Forbid("Only idea author can upload attachments");

        var result = await _fileService.UploadIdeaAttachmentAsync(ideaId, file);
        if (!result.Success)
            return BadRequest(result.Error);

        // Save attachment metadata to database
        var attachment = new Attachment
        {
            IdeaId = ideaId,
            FileName = result.FileName,
            FilePath = result.FilePath,
            FileType = file.ContentType,
            FileSizeBytes = result.FileSizeBytes,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow,
            DisplayOrder = (await _context.Attachments.CountAsync(a => a.IdeaId == ideaId)) + 1
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"File attachment added to idea {ideaId}: {result.FileName}");

        return Ok(new { id = attachment.Id, fileName = result.FileName, fileSize = result.FileSizeBytes });
    }

    /// <summary>
    /// Get attachments for idea
    /// </summary>
    [HttpGet("idea/{ideaId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetIdeaAttachments(int ideaId)
    {
        var attachments = await _context.Attachments
            .Where(a => a.IdeaId == ideaId)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new
            {
                id = a.Id,
                fileName = a.FileName,
                fileSize = a.FileSizeBytes,
                uploadedAt = a.UploadedAt,
                uploadedBy = a.UploadedByUser.FirstName + " " + a.UploadedByUser.LastName
            })
            .ToListAsync();

        return Ok(attachments);
    }

    /// <summary>
    /// Download file
    /// </summary>
    [HttpGet("download/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment == null)
            return NotFound("File not found");

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, attachment.FileType, attachment.FileName);
    }

    /// <summary>
    /// Delete attachment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Idea)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment == null)
            return NotFound("Attachment not found");

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (attachment.UploadedBy != userId && attachment.Idea?.SubmittedBy != userId)
            return Forbid("Only uploader or idea author can delete attachment");

        // Delete physical file
        await _fileService.DeleteFileAsync(attachment.FilePath);

        // Delete database record
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Attachment deleted: {id}");

        return NoContent();
    }
}
