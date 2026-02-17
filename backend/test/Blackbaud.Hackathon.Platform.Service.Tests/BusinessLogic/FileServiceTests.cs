using Blackbaud.Hackathon.Platform.Service.BusinessLogic;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blackbaud.Hackathon.Platform.Service.Tests.BusinessLogic;

public class FileServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly FileService _fileService;
    private readonly string _testUploadDirectory;

    public FileServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<FileService>>();
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        
        // Setup test directory
        _testUploadDirectory = Path.Combine(Path.GetTempPath(), "test_uploads");
        Directory.CreateDirectory(_testUploadDirectory);
        
        SetupConfiguration();
        
        _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(_testUploadDirectory);
        
        _fileService = new FileService(
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockWebHostEnvironment.Object
        );
    }

    private void SetupConfiguration()
    {
        var fileUploadSection = new Mock<IConfigurationSection>();
        fileUploadSection.Setup(x => x["MaxFileSizeMb"]).Returns("10");
        fileUploadSection.Setup(x => x["UploadDirectory"]).Returns("uploads");
        
        var allowedExtensions = new Mock<IConfigurationSection>();
        allowedExtensions.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection>
        {
            CreateConfigSection(".jpg"),
            CreateConfigSection(".jpeg"),
            CreateConfigSection(".png"),
            CreateConfigSection(".gif"),
            CreateConfigSection(".pdf"),
            CreateConfigSection(".doc"),
            CreateConfigSection(".docx")
        });
        
        fileUploadSection.Setup(x => x.GetSection("AllowedExtensions")).Returns(allowedExtensions.Object);
        _mockConfiguration.Setup(x => x.GetSection("FileUpload")).Returns(fileUploadSection.Object);
    }

    private IConfigurationSection CreateConfigSection(string value)
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(x => x.Value).Returns(value);
        return section.Object;
    }

    [Fact]
    public async Task ValidateFile_Should_RejectNullFile()
    {
        // Act
        var (isValid, error) = await _fileService.ValidateFile(null);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("No file");
    }

    [Fact]
    public async Task ValidateFile_Should_RejectEmptyFile()
    {
        // Arrange
        var mockFile = CreateMockFile("test.jpg", 0);

        // Act
        var (isValid, error) = await _fileService.ValidateFile(mockFile.Object);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("empty");
    }

    [Fact]
    public async Task ValidateFile_Should_RejectOversizedFile()
    {
        // Arrange
        var oversizedFile = CreateMockFile("large.jpg", 11 * 1024 * 1024); // 11MB

        // Act
        var (isValid, error) = await _fileService.ValidateFile(oversizedFile.Object);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("exceeds maximum");
    }

    [Theory]
    [InlineData(".exe")]
    [InlineData(".bat")]
    [InlineData(".sh")]
    [InlineData(".dll")]
    [InlineData(".zip")]
    public async Task ValidateFile_Should_RejectDisallowedExtensions(string extension)
    {
        // Arrange
        var mockFile = CreateMockFile($"file{extension}", 1024);

        // Act
        var (isValid, error) = await _fileService.ValidateFile(mockFile.Object);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("not allowed");
    }

    [Theory]
    [InlineData("image.jpg", "image/jpeg")]
    [InlineData("image.jpeg", "image/jpeg")]
    [InlineData("image.png", "image/png")]
    [InlineData("image.gif", "image/gif")]
    [InlineData("document.pdf", "application/pdf")]
    public async Task ValidateFile_Should_AcceptValidFiles(string fileName, string contentType)
    {
        // Arrange
        var mockFile = CreateMockFile(fileName, 1024, contentType);

        // Act
        var (isValid, error) = await _fileService.ValidateFile(mockFile.Object);

        // Assert
        isValid.Should().BeTrue();
        error.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UploadIdeaAttachmentAsync_Should_RejectInvalidFile()
    {
        // Arrange
        var mockFile = CreateMockFile("test.exe", 1024);

        // Act
        var result = await _fileService.UploadIdeaAttachmentAsync(1, mockFile.Object);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UploadIdeaAttachmentAsync_Should_CreateDirectoryIfNotExists()
    {
        // Arrange
        var mockFile = CreateMockFileWithStream("test.jpg", 1024, "image/jpeg");
        var ideaId = 999;
        var expectedDirectory = Path.Combine(_testUploadDirectory, "uploads", $"idea_{ideaId}");

        // Ensure directory doesn't exist
        if (Directory.Exists(expectedDirectory))
        {
            Directory.Delete(expectedDirectory, true);
        }

        // Act
        var result = await _fileService.UploadIdeaAttachmentAsync(ideaId, mockFile.Object);

        // Assert
        result.Success.Should().BeTrue();
        Directory.Exists(expectedDirectory).Should().BeTrue();

        // Cleanup
        if (Directory.Exists(expectedDirectory))
        {
            Directory.Delete(expectedDirectory, true);
        }
    }

    [Fact]
    public async Task UploadIdeaAttachmentAsync_Should_GenerateUniqueFileName()
    {
        // Arrange
        var mockFile1 = CreateMockFileWithStream("test.jpg", 1024, "image/jpeg");
        var mockFile2 = CreateMockFileWithStream("test.jpg", 1024, "image/jpeg");
        var ideaId = 1;

        // Act
        var result1 = await _fileService.UploadIdeaAttachmentAsync(ideaId, mockFile1.Object);
        var result2 = await _fileService.UploadIdeaAttachmentAsync(ideaId, mockFile2.Object);

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        result1.FileName.Should().NotBe(result2.FileName);

        // Cleanup
        var directory = Path.Combine(_testUploadDirectory, "uploads", $"idea_{ideaId}");
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task UploadCommentAttachmentAsync_Should_UploadToCorrectDirectory()
    {
        // Arrange
        var mockFile = CreateMockFileWithStream("comment.jpg", 1024, "image/jpeg");
        var commentId = 42;

        // Act
        var result = await _fileService.UploadCommentAttachmentAsync(commentId, mockFile.Object);

        // Assert
        result.Success.Should().BeTrue();
        result.FilePath.Should().Contain($"comment_{commentId}");

        // Cleanup
        var directory = Path.Combine(_testUploadDirectory, "uploads", $"comment_{commentId}");
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task DeleteFileAsync_Should_ReturnFalseForNonExistentFile()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testUploadDirectory, "nonexistent.jpg");

        // Act
        var result = await _fileService.DeleteFileAsync(nonExistentPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_Should_DeleteExistingFile()
    {
        // Arrange
        var testFilePath = Path.Combine(_testUploadDirectory, "test_delete.jpg");
        await File.WriteAllTextAsync(testFilePath, "test content");
        File.Exists(testFilePath).Should().BeTrue();

        // Act
        var result = await _fileService.DeleteFileAsync(testFilePath);

        // Assert
        result.Should().BeTrue();
        File.Exists(testFilePath).Should().BeFalse();
    }

    [Theory]
    [InlineData("image.jpg", 1024)]
    [InlineData("document.pdf", 5 * 1024 * 1024)] // 5MB
    [InlineData("large.png", 9 * 1024 * 1024)] // 9MB (just under limit)
    public async Task UploadIdeaAttachmentAsync_Should_HandleVariousFileSizes(string fileName, long fileSize)
    {
        // Arrange
        var mockFile = CreateMockFileWithStream(fileName, fileSize, "image/jpeg");
        var ideaId = 1;

        // Act
        var result = await _fileService.UploadIdeaAttachmentAsync(ideaId, mockFile.Object);

        // Assert
        result.Success.Should().BeTrue();
        result.FileSizeBytes.Should().Be(fileSize);

        // Cleanup
        var directory = Path.Combine(_testUploadDirectory, "uploads", $"idea_{ideaId}");
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }

    private Mock<IFormFile> CreateMockFile(string fileName, long length, string contentType = "application/octet-stream")
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        return mockFile;
    }

    private Mock<IFormFile> CreateMockFileWithStream(string fileName, long length, string contentType)
    {
        var mockFile = CreateMockFile(fileName, length, contentType);
        
        // Create actual stream for file operations
        var stream = new MemoryStream(new byte[length]);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));
        
        return mockFile;
    }
}
