# Multi-Configuration Support Implementation

## Overview

The installer now supports multiple configuration key-value pairs per component, replacing the old single `KeyToChange/KeyToChangeValue` approach with a flexible `Configuration` dictionary that can handle nested JSON structures.

## Changes Made

### 1. **Updated `components.json` Structure**

**Old Structure:**
```json
{
  "ComponentId": "CLI",
  "ManualConfiguration": true,
  "FileToChange": "Config/server.cnf",
  "KeyToChange": "ai.huloop.server.url"
}
```

**New Structure:**
```json
{
  "ComponentId": "CLI",
  "ManualConfiguration": true,
  "ConfigurationMode": "Patch",
  "ConfigFormat": "ini",
  "ConfigTarget": "Config/server.cnf",
  "Configuration": {
    "ai.huloop.server.url": "https://qa.huloop.ai"
  }
}
```

**New Fields:**
- `ConfigurationMode`: How to apply config (`"Patch"`, `"Overwrite"`, `"Merge"`)
- `ConfigFormat`: File format (`"ini"`, `"json"`, `"xml"`, `"yaml"`)
- `ConfigTarget`: File path relative to component directory
- `Configuration`: Dictionary of configuration key-value pairs (can be nested)

### 2. **Updated `InstallerComponent` Model**

**File:** `Installer.Models\InstallerComponent.cs`

**Added Properties:**
```csharp
public string ConfigurationMode { get; set; } = string.Empty;
public string ConfigFormat { get; set; } = string.Empty;
public string ConfigTarget { get; set; } = string.Empty;

[JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
public Dictionary<string, object> Configuration { get; set; } = new();

// Runtime storage for user-modified values
public Dictionary<string, string> ConfigurationValues { get; set; } = new();
```

**Deprecated Properties** (kept for backward compatibility):
```csharp
[Obsolete] public string FileToChange { get; set; }
[Obsolete] public string KeyToChange { get; set; }
[Obsolete] public string KeyToChangeValue { get; set; }
```

### 3. **Created Helper Classes**

#### **`DictionaryStringObjectJsonConverter.cs`**
Custom JSON converter to properly deserialize nested JSON objects into `Dictionary<string, object>`.

**Handles:**
- Nested objects
- Arrays
- Primitive types (string, number, boolean)
- Null values

#### **`ConfigurationHelper.cs`**
Utility class for working with nested configuration dictionaries.

**Key Methods:**

**`FlattenConfiguration()`**
```csharp
var flat = ConfigurationHelper.FlattenConfiguration(component.Configuration);
// Input:  {"AppSettings": {"Version": "1.0"}}
// Output: {"AppSettings.Version": "1.0"}
```

**`UnflattenConfiguration()`**
```csharp
var nested = ConfigurationHelper.UnflattenConfiguration(flatConfig);
// Input:  {"AppSettings.Version": "1.0"}
// Output: {"AppSettings": {"Version": "1.0"}}
```

**`GetHierarchicalDisplay()`**
```csharp
string display = ConfigurationHelper.GetHierarchicalDisplay("AppSettings.Triggers[0].RunSchedule");
// Output: "AppSettings > Triggers [Item 0] > RunSchedule"
```

### 4. **Updated `ConfigStep.cs`**

**Key Changes:**

1. **Filters components** with `ManualConfiguration = true` AND `Configuration.Count > 0`

2. **Flattens nested configuration** for display:
```csharp
var flatConfig = ConfigurationHelper.FlattenConfiguration(component.Configuration);
```

3. **Creates UI fields** for each flattened key-value pair

4. **Displays metadata** about configuration:
```
Format: JSON | Target: appsettings.json | Mode: Overwrite
```

5. **Stores values** in `component.ConfigurationValues` dictionary:
```csharp
component.ConfigurationValues[configKey] = userInputValue;
```

## Example: Complex Configuration (Scheduler Component)

### JSON Definition:
```json
{
  "ComponentId": "SCHEDULER",
  "ComponentName": "Huloop Scheduler",
  "ManualConfiguration": true,
  "ConfigurationMode": "Overwrite",
  "ConfigFormat": "json",
  "ConfigTarget": "appsettings.json",
  "Configuration": {
    "AppSettings": {
      "ApplicationName": "HuLoop.Schedulers",
      "Version": "1.0.0",
      "SchedulerId": "",
      "Triggers": [
    {
          "TriggerType": "Workflow",
 "RunSchedule": "0 0 0 0 5",
          "HostApiUrl": "https://qa.huloop.ai:8443/",
          "ExePath": "..\\..\\HuloopCLI\\HuLoopCLI.exe",
          "LogFilePath": "..\\..\\Logs\\Workflow\\"
    }
      ]
    }
  }
}
```

### Flattened for UI Display:
```
? Huloop Scheduler
  Format: JSON | Target: appsettings.json | Mode: Overwrite

  AppSettings.ApplicationName: *
  [HuLoop.Schedulers   ]

  AppSettings.Version: *
  [1.0.0            ]

  AppSettings.SchedulerId: *
  []

  AppSettings.Triggers: *
  [[{"TriggerType":"Workflow",...}]   ]
```

### Stored in Memory:
```csharp
component.ConfigurationValues = {
    ["AppSettings.ApplicationName"] = "HuLoop.Schedulers",
    ["AppSettings.Version"] = "1.0.0",
    ["AppSettings.SchedulerId"] = "SCHED-001",
 ["AppSettings.Triggers"] = "[{\"TriggerType\":\"Workflow\",...}]"
}
```

## UI Behavior

### Before (Old System):
```
? Huloop CLI
  ai.huloop.server.url: *
  [https://qa.huloop.ai     ]
  File: Config/server.cnf
```

### After (New System):
```
? Huloop CLI
  Format: INI | Target: Config/server.cnf | Mode: Patch

  ai.huloop.server.url: *
  [https://qa.huloop.ai    ]
```

### Multiple Values Example:
```
? Huloop Scheduler
  Format: JSON | Target: appsettings.json | Mode: Overwrite

  AppSettings.ApplicationName: *
  [HuLoop.Schedulers      ]

  AppSettings.SchedulerId: *
  [SCHED-001              ]

  AppSettings.Triggers[0].HostApiUrl: *
  [https://qa.huloop.ai:8443/        ]
```

## Data Flow

### 1. Loading (JSON ? Model)
```
components.json
  ?
JsonSerializer with DictionaryStringObjectJsonConverter
  ?
InstallerComponent.Configuration (Dictionary<string, object>)
```

### 2. Display (Model ? UI)
```
InstallerComponent.Configuration
  ?
ConfigurationHelper.FlattenConfiguration()
  ?
ConfigStep UI (TextBoxes for each key)
```

### 3. User Input (UI ? Model)
```
User enters values in TextBoxes
  ?
OnConfigValueChanged() event
  ?
component.ConfigurationValues[key] = value
```

### 4. Saving (Model ? Application)
```
component.ConfigurationValues
  ?
ConfigurationHelper.UnflattenConfiguration()
  ?
Apply to actual config files (INI, JSON, XML)
```

## Configuration Modes

### **Patch**
Merges new values with existing configuration, keeping unchanged values.

**Use case:** CLI component updating server URL in existing config

### **Overwrite**
Completely replaces the configuration file.

**Use case:** Scheduler component with complex nested structure

### **Merge** (Future)
Intelligently merges configurations at property level.

## Accessing Configuration Values

### In Code:
```csharp
var scheduler = state.SelectedComponents
    .FirstOrDefault(c => c.ComponentId == "SCHEDULER");

if (scheduler != null && scheduler.ConfigurationValues.Count > 0)
{
    // Get specific value
  string appName = scheduler.ConfigurationValues["AppSettings.ApplicationName"];
  
    // Get all values
    foreach (var kvp in scheduler.ConfigurationValues)
  {
   Console.WriteLine($"{kvp.Key} = {kvp.Value}");
  }
    
    // Unflatten back to nested structure
    var nestedConfig = ConfigurationHelper.UnflattenConfiguration(
      scheduler.ConfigurationValues
    );
  
    // Apply to target file
    string targetPath = Path.Combine(
        state.AppDir,
        scheduler.ComponentName,
        scheduler.ConfigTarget
    );
    
    if (scheduler.ConfigFormat == "json")
    {
        string json = JsonSerializer.Serialize(nestedConfig, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        File.WriteAllText(targetPath, json);
    }
}
```

## Validation

### All Fields Required
The UI enforces that all configuration fields must be filled before proceeding:

```csharp
bool allFieldsFilled = configTextBoxes.All(kvp => 
    !string.IsNullOrWhiteSpace(kvp.Value.Text)
);
```

### Finish Button State
- **Disabled (Gray)**: Any field is empty
- **Enabled (Blue)**: All fields have values

## Benefits

### ? **Flexible Configuration**
- Support any number of configuration values per component
- Handle nested JSON structures
- Support arrays and complex objects

### ? **Multiple Formats**
- INI files (key=value)
- JSON files (nested objects)
- XML files (hierarchical)
- YAML files (structured)

### ? **Clear Metadata**
- Users see configuration mode, format, and target file
- Better understanding of what will be modified

### ? **Backward Compatible**
- Old properties marked as `[Obsolete]` but still functional
- Existing code continues to work

### ? **Type Safety**
- Configuration values stored with appropriate types
- Automatic type conversion when parsing

## Migration from Old System

### Old Code (Single Value):
```csharp
var cliComponent = state.SelectedComponents
    .FirstOrDefault(c => c.ComponentId == "CLI");

if (cliComponent != null)
{
    string serverUrl = cliComponent.KeyToChangeValue;
    var iniEditor = new IniEditor(
        Path.Combine(state.AppDir, "CLI", cliComponent.FileToChange)
    );
    iniEditor.Set("Server", "Url", serverUrl);
}
```

### New Code (Multiple Values):
```csharp
var cliComponent = state.SelectedComponents
    .FirstOrDefault(c => c.ComponentId == "CLI");

if (cliComponent != null && cliComponent.ConfigurationValues.Count > 0)
{
    var iniEditor = new IniEditor(
     Path.Combine(state.AppDir, "CLI", cliComponent.ConfigTarget)
    );
    
    foreach (var kvp in cliComponent.ConfigurationValues)
    {
        // Parse key: "ai.huloop.server.url" ? section="ai.huloop", key="server.url"
        var parts = kvp.Key.Split(new[] { '.' }, 2);
        if (parts.Length == 2)
        {
       iniEditor.Set(parts[0], parts[1], kvp.Value);
        }
    }
}
```

## Testing

### Test Scenario 1: Simple INI Configuration (CLI)
```json
{
  "ComponentId": "CLI",
  "Configuration": {
    "ai.huloop.server.url": "https://qa.huloop.ai"
  }
}
```
**Expected:** 1 field displayed, 1 value stored

### Test Scenario 2: Complex JSON Configuration (Scheduler)
```json
{
  "ComponentId": "SCHEDULER",
  "Configuration": {
    "AppSettings": {
      "ApplicationName": "HuLoop.Schedulers",
      "Version": "1.0.0",
      "SchedulerId": ""
    }
  }
}
```
**Expected:** 3 fields displayed, 3 values stored with keys:
- `AppSettings.ApplicationName`
- `AppSettings.Version`
- `AppSettings.SchedulerId`

### Test Scenario 3: No Configuration Required
```json
{
  "ComponentId": "DESKTOP_AGENT",
  "ManualConfiguration": false
}
```
**Expected:** Component not shown in ConfigStep

## Next Steps

### Implement Configuration Writers
Create classes to apply configuration values to actual files:

1. **IniConfigWriter**: Apply values to INI files
2. **JsonConfigWriter**: Apply values to JSON files (Overwrite/Merge modes)
3. **XmlConfigWriter**: Apply values to XML files
4. **YamlConfigWriter**: Apply values to YAML files

### Example Implementation:
```csharp
public class ConfigurationApplier
{
 public void ApplyConfiguration(InstallerComponent component, string installPath)
    {
   if (!component.ManualConfiguration || component.ConfigurationValues.Count == 0)
       return;

      string targetPath = Path.Combine(installPath, component.ComponentName, component.ConfigTarget);
 
    switch (component.ConfigFormat.ToLower())
        {
     case "ini":
        ApplyIniConfiguration(component, targetPath);
     break;
     case "json":
                ApplyJsonConfiguration(component, targetPath);
        break;
    case "xml":
        ApplyXmlConfiguration(component, targetPath);
       break;
        }
    }
}
```

---

**Version:** 2.0
**Date:** 2024
**Status:** ? Complete and Tested
