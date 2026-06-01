namespace AestheticStudySpace.Application.Interfaces.Services;

public interface IMediaStorageService
{
    Task<string> UploadBase64ImageAsync(string base64DataUri, string folder, CancellationToken cancellationToken = default);
}

