using System.IO;
using AestheticStudySpace.Application.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace AestheticStudySpace.Infrastructure.Integrations;

public class CloudinaryMediaStorageService : IMediaStorageService
{
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

        var trimmed = base64DataUri.Trim();
        if (!trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid base64 data URI format. Expected data:image/*;base64,...", nameof(base64DataUri));

        int commaIndex = trimmed.IndexOf(',');
        if (commaIndex == -1)
            throw new ArgumentException("Invalid base64 data URI format. Expected data:image/*;base64,...", nameof(base64DataUri));

        var header = trimmed.Substring(0, commaIndex);
        if (!header.Contains(";base64"))
            throw new ArgumentException("Invalid base64 data URI format. Expected data:image/*;base64,...", nameof(base64DataUri));

        string base64Data = trimmed.Substring(commaIndex + 1);

        byte[] raw;
        try
        {
            raw = Convert.FromBase64String(base64Data);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid base64 string format.", nameof(base64DataUri), ex);
        }

        // Compress / resize image if it is too large or to optimize for web delivery
        raw = CompressImageIfNeeded(raw);

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

    private byte[] CompressImageIfNeeded(byte[] rawBytes)
    {
        // If image is less than 500KB, no need to compress
        if (rawBytes.Length < 512 * 1024)
            return rawBytes;

        try
        {
            IImageFormat format;
            using var image = Image.Load(rawBytes, out format);

            // Resize image if any dimension exceeds 1920 pixels
            int maxDimension = 1920;
            if (image.Width > maxDimension || image.Height > maxDimension)
            {
                var resizeOptions = new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(maxDimension, maxDimension),
                    Mode = ResizeMode.Max
                };
                image.Mutate(x => x.Resize(resizeOptions));
            }

            using var outputStream = new MemoryStream();

            if (format is PngFormat)
            {
                // Save PNG format (lossless compression)
                image.Save(outputStream, format);
            }
            else
            {
                // Save JPEG format with quality 75 (greatly reduces size)
                image.Save(outputStream, new JpegEncoder { Quality = 75 });
            }

            var compressedBytes = outputStream.ToArray();
            return compressedBytes.Length < rawBytes.Length ? compressedBytes : rawBytes;
        }
        catch
        {
            // If anything fails (e.g. invalid format or metadata parsing issue), fallback to original raw bytes
            return rawBytes;
        }
    }

    public async Task<string> UploadImageAsync(byte[] bytes, string folder, CancellationToken cancellationToken = default)
    {
        if (bytes == null || bytes.Length == 0)
            throw new ArgumentException("Image data is required.", nameof(bytes));

        // Compress / resize image if it is too large or to optimize for web delivery
        var raw = CompressImageIfNeeded(bytes);

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

    public async Task<string> UploadRawFileAsync(byte[] bytes, string originalFileName, string folder, CancellationToken cancellationToken = default)
    {
        if (bytes == null || bytes.Length == 0)
            throw new ArgumentException("File data is required.", nameof(bytes));

        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));

        await using var stream = new MemoryStream(bytes);

        var uploadParams = new RawUploadParams
        {
            Folder = string.IsNullOrWhiteSpace(folder) ? null : folder.Trim(),
            File = new FileDescription(originalFileName, stream),
            Overwrite = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.StatusCode is not System.Net.HttpStatusCode.OK and not System.Net.HttpStatusCode.Created)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error?.Message ?? result.StatusCode.ToString()}");

        return result.SecureUrl?.ToString() ?? throw new InvalidOperationException("Cloudinary did not return a secure URL.");
    }
}
