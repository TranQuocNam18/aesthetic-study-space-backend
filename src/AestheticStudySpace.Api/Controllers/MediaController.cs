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
}
