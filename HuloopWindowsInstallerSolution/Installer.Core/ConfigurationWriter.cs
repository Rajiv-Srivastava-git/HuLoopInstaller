using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Installer.Models;

namespace Installer.Core
{
    /// <summary>
    /// Handles writing configuration values to component configuration files
    /// after installation is complete
  /// </summary>
    public class ConfigurationWriter
    {
        /// <summary>
        /// Applies configuration changes to all components that require manual configuration
        /// </summary>
        public bool ApplyConfigurations(List<InstallerComponent> components, string installPath)
        {
        bool allSuccess = true;

            foreach (var component in components.Where(c => c.ManualConfiguration))
      {
          try
    {
       if (!ApplyComponentConfiguration(component, installPath))
  {
         allSuccess = false;
        }
      }
           catch (Exception ex)
          {
        Console.WriteLine($"Failed to apply configuration for {component.ComponentName}: {ex.Message}");
         allSuccess = false;
     }
            }

            return allSuccess;
        }

     /// <summary>
        /// Applies configuration for a single component
/// </summary>
   private bool ApplyComponentConfiguration(InstallerComponent component, string installPath)
        {
         // Get platform-specific config target
string configTarget = component.GetConfigTargetForCurrentPlatform();
          
    if (string.IsNullOrEmpty(configTarget))
      {
   Console.WriteLine($"No config target specified for {component.ComponentName}");
       return true; // Not an error, just no config to apply
 }

 // Build full path to config file
      // Format: InstallPath\ComponentFolder\ConfigTarget
    // Example: C:\Program Files\HuLoop\Scheduler\x64\appsettings.json
            string componentFolder = GetComponentFolderName(component);
     string configFilePath = Path.Combine(installPath, componentFolder, configTarget.TrimStart('\\', '/'));

   if (!File.Exists(configFilePath))
            {
   Console.WriteLine($"Config file not found: {configFilePath}");
return false;
}

// Apply configuration based on format
 switch (component.ConfigFormat.ToLowerInvariant())
        {
          case "json":
 return ApplyJsonConfiguration(configFilePath, component);
    
case "ini":
      return ApplyIniConfiguration(configFilePath, component);
     
                case "xml":
       return ApplyXmlConfiguration(configFilePath, component);
        
      default:
   Console.WriteLine($"Unsupported config format: {component.ConfigFormat}");
         return false;
 }
     }

      /// <summary>
        /// Gets the component folder name from the download URL or component ID
        /// </summary>
        private string GetComponentFolderName(InstallerComponent component)
      {
         // Extract folder name from download URL
     // Example: https://qa.huloop.ai/latest/HuloopScheduler.zip -> HuloopScheduler
      string downloadUrl = component.GetDownloadUrlForCurrentPlatform();
   string fileName = Path.GetFileNameWithoutExtension(new Uri(downloadUrl).LocalPath);
    
            // Remove platform suffixes if present
    // Example: HuloopScheduler-win-x64 -> HuloopScheduler
            if (fileName.Contains('-'))
   {
    fileName = fileName.Substring(0, fileName.IndexOf('-'));
            }
            
return fileName;
}

  /// <summary>
        /// Applies JSON configuration (Patch or Overwrite mode)
        /// </summary>
        private bool ApplyJsonConfiguration(string configFilePath, InstallerComponent component)
        {
            try
            {
  // Read existing JSON
  string existingJson = File.ReadAllText(configFilePath);
 JsonDocument existingDoc = JsonDocument.Parse(existingJson);

      // Get user-modified values from ConfigurationValues
      var userValues = component.ConfigurationValues;

       if (component.ConfigurationMode.Equals("Overwrite", StringComparison.OrdinalIgnoreCase))
                {
   // Overwrite mode: Replace entire configuration
           var newConfig = ConfigurationHelper.UnflattenConfiguration(userValues);
          string newJson = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions 
    { 
 WriteIndented = true 
});
             File.WriteAllText(configFilePath, newJson);
          }
      else // Patch mode
                {
                  // Patch mode: Merge user values into existing config
             var patchedConfig = PatchJsonConfiguration(existingDoc, userValues);
        string patchedJson = JsonSerializer.Serialize(patchedConfig, new JsonSerializerOptions 
         { 
       WriteIndented = true 
      });
           File.WriteAllText(configFilePath, patchedJson);
    }

                Console.WriteLine($"Successfully updated JSON config: {configFilePath}");
 return true;
   }
     catch (Exception ex)
        {
     Console.WriteLine($"Failed to update JSON config {configFilePath}: {ex.Message}");
    return false;
       }
        }

        /// <summary>
        /// Patches JSON configuration by merging user values
        /// </summary>
        private Dictionary<string, object> PatchJsonConfiguration(JsonDocument existingDoc, Dictionary<string, string> userValues)
     {
 // Convert existing JSON to dictionary
   var existingDict = JsonSerializer.Deserialize<Dictionary<string, object>>(existingDoc.RootElement.GetRawText());
            
      // Apply user values
        foreach (var kvp in userValues)
       {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
      {
        SetNestedValue(existingDict, kvp.Key, kvp.Value);
              }
    }
            
    return existingDict ?? new Dictionary<string, object>();
   }

  /// <summary>
        /// Sets a nested value in a dictionary using a flattened key path
      /// Example: "AppSettings.Triggers[0].ExePath" -> Sets value in nested structure
        /// </summary>
        private void SetNestedValue(Dictionary<string, object> dict, string flatKey, string value)
      {
     var keys = ConfigurationHelper.SplitKey(flatKey);
          
  if (keys.Length == 1)
 {
      dict[keys[0]] = value;
      return;
}

            // Navigate to the nested location
 object current = dict;
       for (int i = 0; i < keys.Length - 1; i++)
  {
             string key = keys[i];
                
       if (ConfigurationHelper.IsArrayIndex(key, out string arrayName, out int index))
         {
     // Handle array access
         if (current is Dictionary<string, object> currentDict)
       {
 if (!currentDict.ContainsKey(arrayName))
       {
     currentDict[arrayName] = new List<object>();
      }
        
    if (currentDict[arrayName] is List<object> list)
     {
     // Ensure list has enough elements
        while (list.Count <= index)
      {
list.Add(new Dictionary<string, object>());
          }
           current = list[index];
    }
      }
            }
      else
         {
  // Handle regular key access
  if (current is Dictionary<string, object> currentDict)
{
        if (!currentDict.ContainsKey(key))
        {
        currentDict[key] = new Dictionary<string, object>();
      }
        current = currentDict[key];
       }
   }
       }

            // Set the final value
  string finalKey = keys[^1];
    if (current is Dictionary<string, object> finalDict)
          {
     finalDict[finalKey] = value;
     }
}

      /// <summary>
        /// Applies INI configuration
        /// </summary>
        private bool ApplyIniConfiguration(string configFilePath, InstallerComponent component)
        {
            try
         {
                var userValues = component.ConfigurationValues;
     var lines = File.ReadAllLines(configFilePath).ToList();

              foreach (var kvp in userValues)
      {
       if (string.IsNullOrWhiteSpace(kvp.Value))
      continue;

 string key = kvp.Key;
       string value = kvp.Value;

    // Find and update the line with this key
                bool found = false;
 for (int i = 0; i < lines.Count; i++)
         {
           string line = lines[i].Trim();
     
      // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#"))
      continue;

       if (line.StartsWith(key + "=") || line.StartsWith(key + " ="))
          {
              lines[i] = $"{key}={value}";
    found = true;
         break;
             }
          }

                 // If key not found, append it
       if (!found)
       {
        lines.Add($"{key}={value}");
            }
           }

          File.WriteAllLines(configFilePath, lines);
  Console.WriteLine($"Successfully updated INI config: {configFilePath}");
        return true;
          }
catch (Exception ex)
 {
            Console.WriteLine($"Failed to update INI config {configFilePath}: {ex.Message}");
  return false;
            }
        }

        /// <summary>
        /// Applies XML configuration (placeholder - implement based on your XML structure)
        /// </summary>
        private bool ApplyXmlConfiguration(string configFilePath, InstallerComponent component)
        {
        Console.WriteLine($"XML configuration not yet implemented for {configFilePath}");
         // TODO: Implement XML configuration updates using System.Xml.Linq
          return true;
        }
    }
}
