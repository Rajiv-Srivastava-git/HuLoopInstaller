using Installer.Models;

namespace Installer.Core
{
    public class InstallerState
    {
        public string AppDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "huloop");
        public string ApiUrl { get; set; } = "https://qa.huloop.ai/api";
        public string ServerUrl { get; set; } = "https://qa.huloop.ai/server";
        public string UpdateChannel { get; set; } = "stable";
        public string HuloopRoot { get; set; } = string.Empty;
        public string HuloopVersion { get; set; } = "1.0.0";
        public bool EnableAutoUpdate { get; set; } = true;
        public List<InstallerComponent> SelectedComponents { get; set; } = new();
        
        // Store configuration values: Key = ComponentId:KeyToChange, Value = User entered value
        public Dictionary<string, string> ComponentConfigurationValues { get; set; } = new();
    }
}
