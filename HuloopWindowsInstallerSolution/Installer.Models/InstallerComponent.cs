using System;
using System.Collections.Generic;
using System.Linq;
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
        public long ComponentSizeMB { get; set; } // Size shown to user
        public string DownloadUrl { get; set; } = string.Empty;

        public bool IsMandatory { get; set; }
        public List<string> IsConnectedWith { get; set; } = new();
        public bool ManualConfiguration { get; set; }
        
        // New configuration properties
        public string ConfigurationMode { get; set; } = string.Empty; // "Patch", "Overwrite", etc.
        public string ConfigFormat { get; set; } = string.Empty; // "ini", "json", "xml", etc.
        public string ConfigTarget { get; set; } = string.Empty; // File path relative to component
        
        // Configuration key-value pairs from JSON
        [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
        public Dictionary<string, object> Configuration { get; set; } = new();
        
    // Runtime properties (not from JSON)
    public bool IsSelected { get; set; }
 
      // Store user-modified configuration values at runtime
   public Dictionary<string, string> ConfigurationValues { get; set; } = new();
        
        // Legacy properties (deprecated but kept for backward compatibility)
        [Obsolete("Use Configuration and ConfigurationValues instead")]
        public string FileToChange { get; set; } = string.Empty;
   
        [Obsolete("Use Configuration and ConfigurationValues instead")]
        public string KeyToChange { get; set; } = string.Empty;
        
        [Obsolete("Use Configuration and ConfigurationValues instead")]
        public string KeyToChangeValue { get; set; } = string.Empty;
    }
}
