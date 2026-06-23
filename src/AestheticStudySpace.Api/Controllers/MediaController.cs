using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AestheticStudySpace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaStorageService _mediaStorageService;

    public MediaController(IMediaStorageService mediaStorageService)
    {
        _mediaStorageService = mediaStorageService;
    }

    /// <summary>
    /// Uploads an image file, automatically compressing and resizing it if necessary, and returns the Cloudinary URL.
    /// This resolves the Cloudinary 10MB file size limit by performing optimization on the server.
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file uploaded."));

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        try
        {
            var url = await _mediaStorageService.UploadImageAsync(bytes, "ass/uploads", cancellationToken);
            return Ok(ApiResponse<string>.Ok(url, "Image uploaded and optimized successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Upload failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Uploads an audio or raw file (mp3, ogg, wav, etc.) to Cloudinary and returns the secure URL.
    /// Use this to upload AmbientSound or Effect assets before submitting a component or theme.
    /// Accepted content types: audio/mpeg, audio/ogg, audio/wav, audio/webm, video/mp4, video/webm.
    /// Max file size: 20 MB.
    /// </summary>
    [HttpPost("upload-audio")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> UploadAudio(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file uploaded."));

        const long maxBytes = 20 * 1024 * 1024; // 20 MB
        if (file.Length > maxBytes)
            return BadRequest(ApiResponse<string>.Fail("File size exceeds the 20 MB limit."));

        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "audio/mpeg", "audio/ogg", "audio/wav", "audio/webm",
            "video/mp4", "video/webm"
        };

        if (!allowedContentTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse<string>.Fail($"Unsupported file type: {file.ContentType}. Allowed: mp3, ogg, wav, webm, mp4."));

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        try
        {
            var url = await _mediaStorageService.UploadRawFileAsync(bytes, file.FileName, "ass/audio", cancellationToken);
            return Ok(ApiResponse<string>.Ok(url, "Audio file uploaded successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"Upload failed: {ex.Message}"));
        }
    }
}
