using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Installer.Models
{
    public class InstallerComponent
    {
        public string ComponentId { get; set; } = string.Empty;
        public string ComponentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Single download URL (for backward compatibility)
        public string DownloadUrl { get; set; } = string.Empty;

        // Platform-specific download URLs (NEW)
        public Dictionary<string, PlatformDownloadInfo> PlatformDownloads { get; set; } = new();

        // Single size (for backward compatibility)
        public long ComponentSizeMB { get; set; }

        public bool IsMandatory { get; set; }
        public List<string> IsConnectedWith { get; set; } = new();
        public bool ManualConfiguration { get; set; }

        // Configuration properties
        public string ConfigurationMode { get; set; } = string.Empty;
        public string ConfigFormat { get; set; } = string.Empty;
        public string ConfigTarget { get; set; } = string.Empty;

        // Configuration key-value pairs from JSON
        [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
        public Dictionary<string, object> Configuration { get; set; } = new();

        // Runtime properties (not from JSON)
        public bool IsSelected { get; set; }
        public Dictionary<string, string> ConfigurationValues { get; set; } = new();

        /// <summary>
        /// Gets the appropriate download URL for the current platform
        /// </summary>
        public string GetDownloadUrlForCurrentPlatform()
        {
            // If no platform-specific URLs, return the default URL
            if (PlatformDownloads == null || PlatformDownloads.Count == 0)
                return DownloadUrl;

            string platform = DetectPlatform();

            // Try to get platform-specific URL
            if (PlatformDownloads.TryGetValue(platform, out var downloadInfo))
                return downloadInfo.DownloadUrl;

            // Fallback to default URL
            return DownloadUrl;
        }

        /// <summary>
        /// Gets the appropriate size for the current platform
        /// </summary>
        public long GetSizeForCurrentPlatform()
        {
            // If no platform-specific sizes, return the default size
            if (PlatformDownloads == null || PlatformDownloads.Count == 0)
                return ComponentSizeMB;

            string platform = DetectPlatform();

            // Try to get platform-specific size
            if (PlatformDownloads.TryGetValue(platform, out var downloadInfo))
                return downloadInfo.SizeMB;

            // Fallback to default size
            return ComponentSizeMB;
        }

        /// <summary>
        /// Gets the appropriate ConfigTarget for the current platform
        /// </summary>
        public string GetConfigTargetForCurrentPlatform()
        {
            // If no platform-specific config targets, return the default
            if (PlatformDownloads == null || PlatformDownloads.Count == 0)
                return ConfigTarget;

            string platform = DetectPlatform();

            // Try to get platform-specific config target
            if (PlatformDownloads.TryGetValue(platform, out var downloadInfo))
            {
                // If platform has a specific ConfigTarget, use it
                if (!string.IsNullOrEmpty(downloadInfo.ConfigTarget))
                    return downloadInfo.ConfigTarget;
            }

            // Fallback to default ConfigTarget
            return ConfigTarget;
        }

        /// <summary>
        /// Detects the current platform architecture
        /// </summary>
        private static string DetectPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var arch = RuntimeInformation.ProcessArchitecture;
                return arch switch
                {
                    Architecture.X64 => "win-x64",
                    Architecture.X86 => "win-x86",
                    Architecture.Arm64 => "win-arm64",
                    _ => "win-x64" // Default to x64
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var arch = RuntimeInformation.ProcessArchitecture;
                return arch switch
                {
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm64 => "linux-arm64",
                    _ => "linux-x64"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var arch = RuntimeInformation.ProcessArchitecture;
                return arch switch
                {
                    Architecture.X64 => "osx-x64",
                    Architecture.Arm64 => "osx-arm64",
                    _ => "osx-x64"
                };
            }

            return "win-x64"; // Default fallback
        }

        /// <summary>
        /// Gets a user-friendly description of the current platform
        /// </summary>
        public static string GetCurrentPlatformDescription()
        {
            string platform = DetectPlatform();
            return platform switch
            {
                "win-x64" => "Windows 64-bit (x64)",
                "win-x86" => "Windows 32-bit (x86)",
                "win-arm64" => "Windows ARM 64-bit",
                "linux-x64" => "Linux 64-bit",
                "linux-arm64" => "Linux ARM 64-bit",
                "osx-x64" => "macOS Intel",
                "osx-arm64" => "macOS Apple Silicon",
                _ => platform
            };
        }

        // Legacy properties (deprecated)
        [Obsolete("Use Configuration and ConfigurationValues instead")]
        public string FileToChange { get; set; } = string.Empty;

        [Obsolete("Use Configuration and ConfigurationValues instead")]
        public string KeyToChange { get; set; } = string.Empty;

        [Obsolete("Use Configuration and ConfigurationValues instead")]
        public string KeyToChangeValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Platform-specific download information
    /// </summary>
    public class PlatformDownloadInfo
    {
        public string DownloadUrl { get; set; } = string.Empty;
        public long SizeMB { get; set; }
        public string ConfigTarget { get; set; } = string.Empty;  // NEW: Platform-specific config target path
    }
}
