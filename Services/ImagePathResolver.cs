using System;
using System.IO;

namespace Portfolyo.Services
{
    public static class ImagePathResolver
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".svg" };

        public static string Resolve(string? rawPath, string fallbackPath, string? legacyUploadFolder = null)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return fallbackPath;
            }

            var path = rawPath.Trim().Replace('\\', '/');

            if (IsExternal(path))
            {
                return path;
            }

            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                return "/" + path.Substring(2);
            }

            if (path.StartsWith("/", StringComparison.Ordinal))
            {
                return path;
            }

            if (path.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            {
                return "/" + path;
            }

            if (!path.Contains("/", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(legacyUploadFolder) &&
                HasImageExtension(path))
            {
                return $"/uploads/{legacyUploadFolder.Trim('/')}/{path}";
            }

            return "/" + path.TrimStart('/');
        }

        private static bool HasImageExtension(string value)
        {
            var extension = Path.GetExtension(value);
            return !string.IsNullOrWhiteSpace(extension)
                   && Array.Exists(ImageExtensions, x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsExternal(string value)
        {
            if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                   uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }
    }
}
