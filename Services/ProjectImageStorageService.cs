using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Portfolyo.Options;

namespace Portfolyo.Services;

public interface IProjectImageStorageService
{
    Task<string?> SaveUploadedImageAsync(IFormFile file, string folderName);
    Task DeleteUploadedImageAsync(string? storedPath);
}

public class ProjectImageStorageService : IProjectImageStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProjectImageStorageService> _logger;
    private readonly CloudinaryOptions _cloudinaryOptions;
    private readonly Cloudinary? _cloudinary;
    private readonly bool _useCloudinary;

    public ProjectImageStorageService(
        IWebHostEnvironment environment,
        IOptions<CloudinaryOptions> cloudinaryOptions,
        ILogger<ProjectImageStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
        _cloudinaryOptions = cloudinaryOptions.Value ?? new CloudinaryOptions();

        _useCloudinary =
            !string.IsNullOrWhiteSpace(_cloudinaryOptions.CloudName) &&
            !string.IsNullOrWhiteSpace(_cloudinaryOptions.ApiKey) &&
            !string.IsNullOrWhiteSpace(_cloudinaryOptions.ApiSecret);

        if (_useCloudinary)
        {
            var account = new Account(
                _cloudinaryOptions.CloudName,
                _cloudinaryOptions.ApiKey,
                _cloudinaryOptions.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
    }

    public async Task<string?> SaveUploadedImageAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return null;
        }

        if (_useCloudinary && _cloudinary != null)
        {
            using var stream = file.OpenReadStream();
            var publicId = Guid.NewGuid().ToString("N");
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"portfolyo/{folderName.Trim('/')}",
                PublicId = publicId,
                Overwrite = false,
                UseFilename = false,
                UniqueFilename = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                _logger.LogWarning(
                    "Cloudinary upload failed for {FolderName}: {Error}",
                    folderName,
                    uploadResult.Error.Message);
                return null;
            }

            return uploadResult.SecureUrl?.ToString();
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await file.CopyToAsync(fileStream);

        return $"/uploads/{folderName}/{fileName}";
    }

    public async Task DeleteUploadedImageAsync(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return;
        }

        var normalized = storedPath.Trim();

        if (_useCloudinary &&
            _cloudinary != null &&
            TryExtractCloudinaryPublicId(normalized, out var publicId))
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Invalidate = true
            };

            var deleteResult = await _cloudinary.DestroyAsync(deleteParams);
            if (deleteResult.Error != null)
            {
                _logger.LogWarning("Cloudinary delete failed for {PublicId}: {Error}", publicId, deleteResult.Error.Message);
            }

            return;
        }

        if (!normalized.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) &&
            !normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = normalized.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relativePath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads"));

        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private bool TryExtractCloudinaryPublicId(string url, out string publicId)
    {
        publicId = string.Empty;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!uri.Host.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var segments = uri.AbsolutePath
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 5)
        {
            return false;
        }

        if (!string.Equals(segments[0], _cloudinaryOptions.CloudName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var uploadIndex = Array.FindIndex(segments, x => x.Equals("upload", StringComparison.OrdinalIgnoreCase));
        if (uploadIndex < 0 || uploadIndex >= segments.Length - 1)
        {
            return false;
        }

        var startIndex = uploadIndex + 1;
        var versionIndex = Array.FindIndex(
            segments,
            startIndex,
            x => x.Length > 1 && x[0] == 'v' && x.Skip(1).All(char.IsDigit));

        if (versionIndex >= 0)
        {
            startIndex = versionIndex + 1;
        }

        if (startIndex >= segments.Length)
        {
            return false;
        }

        var joined = string.Join('/', segments.Skip(startIndex));
        var dotIndex = joined.LastIndexOf('.');
        if (dotIndex > joined.LastIndexOf('/'))
        {
            joined = joined[..dotIndex];
        }

        if (string.IsNullOrWhiteSpace(joined))
        {
            return false;
        }

        publicId = joined;
        return true;
    }
}
