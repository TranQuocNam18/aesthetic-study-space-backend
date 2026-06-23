namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IMediaStorageService
{
    Task<string> UploadBase64ImageAsync(string base64DataUri, string folder, CancellationToken cancellationToken = default);
    Task<string> UploadImageAsync(byte[] bytes, string folder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a raw file (audio, video, etc.) to Cloudinary and returns the secure URL.
    /// Suitable for AmbientSound (mp3/ogg/wav) and Effect (video) assets.
    /// </summary>
    Task<string> UploadRawFileAsync(byte[] bytes, string originalFileName, string folder, CancellationToken cancellationToken = default);
}

