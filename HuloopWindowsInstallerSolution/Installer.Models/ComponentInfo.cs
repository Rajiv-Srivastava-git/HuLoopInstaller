namespace Installer.Models
{
    public class ComponentInfo
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string SettingsTemplateUrl { get; set; } = string.Empty;
    }
}
