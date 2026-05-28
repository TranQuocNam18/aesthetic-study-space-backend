using System.Text.RegularExpressions;
using AestheticStudySpace.Application.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace AestheticStudySpace.Infrastructure.Integrations;

public class CloudinaryMediaStorageService : IMediaStorageService
{
    private static readonly Regex DataUriRegex = new(
        @"^data:(?<mime>image\/[a-zA-Z0-9\+\-\.]+);base64,(?<data>[A-Za-z0-9\+/=\r\n]+)$",
        RegexOptions.Compiled);

    private readonly Cloudinary _cloudinary;

    public CloudinaryMediaStorageService(IOptions<CloudinarySettings> settings)
    {
        var s = settings.Value;
        if (string.IsNullOrWhiteSpace(s.CloudName) || string.IsNullOrWhiteSpace(s.ApiKey) || string.IsNullOrWhiteSpace(s.ApiSecret))
            throw new InvalidOperationException("Cloudinary settings are not configured.");

        _cloudinary = new Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret));
    }

    public async Task<string> UploadBase64ImageAsync(string base64DataUri, string folder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(base64DataUri))
            throw new ArgumentException("Image is required.", nameof(base64DataUri));

        var match = DataUriRegex.Match(base64DataUri.Trim());
        if (!match.Success)
            throw new ArgumentException("Invalid base64 data URI format. Expected data:image/*;base64,...", nameof(base64DataUri));

        var raw = Convert.FromBase64String(match.Groups["data"].Value);
        await using var stream = new MemoryStream(raw);

        var uploadParams = new ImageUploadParams
        {
            Folder = string.IsNullOrWhiteSpace(folder) ? null : folder.Trim(),
            File = new FileDescription("upload.png", stream),
            Overwrite = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.StatusCode is not System.Net.HttpStatusCode.OK and not System.Net.HttpStatusCode.Created)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error?.Message ?? result.StatusCode.ToString()}");

        return result.SecureUrl?.ToString() ?? throw new InvalidOperationException("Cloudinary did not return a secure URL.");
    }
}

