using System.IO.Compression;
using System.Net.Http;
using Installer.Models;
using System.Security.Cryptography;

namespace Installer.Core
{
    public class ComponentDownloader
    {
        private readonly HttpClient _http = new HttpClient();

        public ComponentDownloader()
        {
        }

        public async Task<string> DownloadAndExtractAsync(ComponentInfo comp, string targetDir, IProgress<string>? progress = null, CancellationToken ct = default)
        {
            Directory.CreateDirectory(targetDir);
            var zipPath = Path.Combine(targetDir, comp.FileName);
            progress?.Report($"Downloading {comp.FileName}..."); 
            using (var response = await _http.GetAsync(comp.Url, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs, ct);
                }
            }

            progress?.Report("Verifying checksum..."); 
            if (!string.IsNullOrEmpty(comp.Sha256))
            {
                using var sha = SHA256.Create();
                using var fs = File.OpenRead(zipPath);
                var hash = await sha.ComputeHashAsync(fs, ct);
                var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                if (!hex.Equals(comp.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("SHA256 mismatch for " + comp.FileName);
                }
            }

            progress?.Report("Extracting..."); 
            var extractDir = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(comp.FileName));
            if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
            ZipFile.ExtractToDirectory(zipPath, extractDir);
            File.Delete(zipPath);
            progress?.Report("Completed " + comp.FileName);
            return extractDir;
        }
    }
}
