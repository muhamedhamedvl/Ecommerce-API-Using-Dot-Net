using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using WebApiEcomm.Core.Services;

namespace WebApiEcomm.InfraStructure.Repositores.Service
{
    public class ImageManagementService : IImageManagementService
    {
        private static readonly Regex InvalidFileChars = new(@"[^a-zA-Z0-9._-]+", RegexOptions.Compiled);
        private const long MaxFileBytes = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly IWebHostEnvironment _environment;

        public ImageManagementService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<string>> AddImageAsync(IFormFileCollection files, string src)
        {
            if (files is null || files.Count == 0)
                return new List<string>();

            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot");
            }

            var safeSegment = SanitizePathSegment(string.IsNullOrWhiteSpace(src) ? "misc" : src);
            var root = Path.Combine(webRoot, "Images", safeSegment);
            Directory.CreateDirectory(root);

            var results = new List<string>();
            foreach (var item in files)
            {
                if (item.Length <= 0 || item.Length > MaxFileBytes)
                    continue;

                var ext = Path.GetExtension(item.FileName);
                if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                    continue;

                var baseName = Path.GetFileNameWithoutExtension(item.FileName);
                var sanitized = InvalidFileChars.Replace(baseName ?? "file", "_");
                if (string.IsNullOrWhiteSpace(sanitized))
                    sanitized = "file";

                var uniqueName = $"{sanitized}_{Guid.NewGuid():N}{ext}";
                var physicalPath = Path.Combine(root, uniqueName);

                await using (FileStream stream = new(physicalPath, FileMode.Create))
                {
                    await item.CopyToAsync(stream);
                }

                results.Add($"/Images/{safeSegment}/{uniqueName}");
            }

            return results;
        }

        public Task<string> DeleteImageAsync(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
                return Task.FromResult("Image not found.");

            try
            {
                var relative = src.TrimStart('/');
                var webRoot = _environment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRoot))
                    webRoot = Path.Combine(_environment.ContentRootPath ?? AppContext.BaseDirectory, "wwwroot");

                var full = Path.GetFullPath(Path.Combine(webRoot, relative));
                webRoot = Path.GetFullPath(webRoot);
                if (!full.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
                    return Task.FromResult("Image not found.");

                File.Delete(full);
                return Task.FromResult("Image deleted successfully.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting image: {ex.Message}", ex);
            }
        }

        private static string SanitizePathSegment(string input)
        {
            var s = InvalidFileChars.Replace(input, "_");
            if (string.IsNullOrWhiteSpace(s))
                return "misc";
            return s[..Math.Min(s.Length, 64)];
        }
    }
}
