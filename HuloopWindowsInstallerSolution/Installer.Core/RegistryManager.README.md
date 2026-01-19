# HuLoop Installer - Registry Manager Documentation

## Overview

The `RegistryManager` class provides functionality to save, retrieve, and manage HuLoop installation information in the Windows Registry. This is essential for:
- Tracking installed components
- Storing configuration values
- Enabling uninstallation
- Supporting upgrades and updates
- Providing system-wide installation information

## Registry Structure

The HuLoop installer stores data under:
```
HKEY_LOCAL_MACHINE\SOFTWARE\HuLoop\
??? InstallInfo\
?   ??? InstallPath (String)
?   ??? ApiUrl (String)
?   ??? ServerUrl (String)
?   ??? UpdateChannel (String)
?   ??? Version (String)
?   ??? EnableAutoUpdate (DWord: 0 or 1)
? ??? InstallDate (String: yyyy-MM-dd HH:mm:ss)
?   ??? ComponentCount (DWord)
?
??? Components\
    ??? CLI\
    ?   ??? ComponentName (String)
    ?   ??? ComponentId (String)
    ? ??? SizeMB (QWord)
    ?   ??? DownloadUrl (String)
    ?   ??? IsMandatory (DWord: 0 or 1)
    ?   ??? IsSelected (DWord: 0 or 1)
    ???? ManualConfiguration (DWord: 0 or 1)
    ?   ??? FileToChange (String)
    ?   ??? KeyToChange (String)
    ?   ??? KeyToChangeValue (String) ? User configured value
    ?   ??? ConnectedWith (String: comma-separated)
    ?   ??? InstallDate (String)
    ?
    ??? DESKTOP_AGENT\
    ?   ??? ... (same structure)
    ?
    ??? DESKTOP_DRIVER\
     ??? ... (same structure)
```

## Key Features

### 1. **Save Installation to Registry**
```csharp
var registryManager = new RegistryManager();
bool success = registryManager.SaveInstallationToRegistry(installerState);

if (success)
{
    Console.WriteLine("Installation info saved to registry");
}
```

**What gets saved:**
- Installation path
- API and Server URLs
- Version information
- All selected components with their configuration
- User-provided configuration values
- Installation timestamps

### 2. **Read Installation from Registry**
```csharp
var registryManager = new RegistryManager();
InstallerState? state = registryManager.ReadInstallationFromRegistry();

if (state != null)
{
    Console.WriteLine($"HuLoop is installed at: {state.AppDir}");
    Console.WriteLine($"Version: {state.HuloopVersion}");
    Console.WriteLine($"Components: {state.SelectedComponents.Count}");
    
    foreach (var component in state.SelectedComponents)
    {
        Console.WriteLine($"  - {component.ComponentName}");
        if (component.ManualConfiguration)
        {
    Console.WriteLine($"    Config: {component.KeyToChange} = {component.KeyToChangeValue}");
        }
    }
}
else
{
    Console.WriteLine("HuLoop is not installed");
}
```

### 3. **Check if Installed**
```csharp
var registryManager = new RegistryManager();

if (registryManager.IsInstalled())
{
    Console.WriteLine("HuLoop is already installed");
    string version = registryManager.GetInstalledVersion();
    string path = registryManager.GetInstallPath();
  
    Console.WriteLine($"Version: {version}");
    Console.WriteLine($"Location: {path}");
}
```

### 4. **Update Component Configuration**
```csharp
var registryManager = new RegistryManager();

// Update CLI component's server URL
bool success = registryManager.UpdateComponentConfiguration(
    "CLI", 
    "https://new-server.company.com"
);

if (success)
{
    Console.WriteLine("Configuration updated in registry");
}
```

### 5. **Remove from Registry (Uninstall)**
```csharp
var registryManager = new RegistryManager();

if (registryManager.RemoveInstallationFromRegistry())
{
    Console.WriteLine("HuLoop registry entries removed successfully");
}
```

## Usage Examples

### Example 1: Installation Complete
```csharp
// In WizardForm.cs - when user clicks Finish
private void OnInstallationComplete()
{
    var registryManager = new RegistryManager();
    
    if (registryManager.SaveInstallationToRegistry(State))
    {
        MessageBox.Show(
            $"Installation completed successfully!\n\n" +
            $"Path: {State.AppDir}\n" +
$"Components: {State.SelectedComponents.Count}\n\n" +
       "Configuration saved to registry.",
            "Success",
       MessageBoxButtons.OK,
            MessageBoxIcon.Information
     );
    }
}
```

### Example 2: Upgrade Detection
```csharp
// Check if already installed before starting installation
public bool CheckForExistingInstallation()
{
    var registryManager = new RegistryManager();
    
    if (registryManager.IsInstalled())
    {
        string installedVersion = registryManager.GetInstalledVersion();
        string newVersion = "2.0.0";
        
        var result = MessageBox.Show(
            $"HuLoop version {installedVersion} is already installed.\n\n" +
   $"Do you want to upgrade to version {newVersion}?",
  "Existing Installation Found",
      MessageBoxButtons.YesNo,
      MessageBoxIcon.Question
        );
        
        return result == DialogResult.Yes;
    }
    
    return true; // No existing installation, proceed
}
```

### Example 3: Uninstaller Application
```csharp
// Separate uninstaller application
class Uninstaller
{
    static void Main()
    {
        var registryManager = new RegistryManager();
        
        // Read installation info
        var state = registryManager.ReadInstallationFromRegistry();
        
        if (state == null)
        {
  Console.WriteLine("HuLoop is not installed");
  return;
        }
        
        Console.WriteLine($"Found HuLoop installation:");
        Console.WriteLine($"  Path: {state.AppDir}");
   Console.WriteLine($"  Version: {state.HuloopVersion}");
        Console.WriteLine($"  Components: {state.SelectedComponents.Count}");
        
        Console.Write("\nProceed with uninstallation? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
   {
            // Delete files
            if (Directory.Exists(state.AppDir))
     {
         Directory.Delete(state.AppDir, true);
                Console.WriteLine("Files removed");
            }
            
   // Remove registry entries
            if (registryManager.RemoveInstallationFromRegistry())
            {
                Console.WriteLine("Registry entries removed");
    }
            
       Console.WriteLine("Uninstallation complete!");
}
    }
}
```

### Example 4: Configuration Manager
```csharp
// Tool to view/update configuration after installation
class ConfigManager
{
    static void Main()
    {
        var registryManager = new RegistryManager();
  var state = registryManager.ReadInstallationFromRegistry();
        
        if (state == null)
        {
     Console.WriteLine("HuLoop is not installed");
  return;
        }
        
        Console.WriteLine("Installed Components with Configuration:");
Console.WriteLine();
        
        foreach (var component in state.SelectedComponents.Where(c => c.ManualConfiguration))
{
         Console.WriteLine($"Component: {component.ComponentName}");
    Console.WriteLine($"  File: {component.FileToChange}");
Console.WriteLine($"  Key: {component.KeyToChange}");
        Console.WriteLine($"  Current Value: {component.KeyToChangeValue}");
Console.WriteLine();
    
         Console.Write("  Update value? (y/n): ");
      if (Console.ReadLine()?.ToLower() == "y")
{
                Console.Write("  Enter new value: ");
                string newValue = Console.ReadLine();
         
     if (registryManager.UpdateComponentConfiguration(component.ComponentId, newValue))
         {
 Console.WriteLine("  ? Updated in registry");
 
          // Also update the actual config file
          string filePath = Path.Combine(state.AppDir, component.ComponentName, component.FileToChange);
          // ... update file using IniEditor or other method
    }
        }
 }
    }
}
```

## Important Notes

### Permissions
- **Requires Administrator privileges** to write to `HKEY_LOCAL_MACHINE`
- Installer should request elevation on startup
- Run your installer with "Run as Administrator" or use a manifest

### Error Handling
All methods include try-catch blocks and return boolean success values. Always check return values:

```csharp
if (!registryManager.SaveInstallationToRegistry(state))
{
    // Handle failure - maybe save to a local file as backup
    Logger.LogWarning("Failed to save to registry");
}
```

### Registry Value Types
- **String** (REG_SZ): Text values
- **DWord** (REG_DWORD): 32-bit integers (0 or 1 for booleans)
- **QWord** (REG_QWORD): 64-bit integers (for file sizes)

### Data Stored
The following data from `InstallerState` and `InstallerComponent` is persisted:

**InstallerState:**
- AppDir ?
- ApiUrl ?
- ServerUrl ?
- UpdateChannel ?
- HuloopVersion ?
- EnableAutoUpdate ?
- SelectedComponents ?

**InstallerComponent:**
- ComponentId ?
- ComponentName ?
- ComponentSizeMB ?
- DownloadUrl ?
- IsMandatory ?
- IsSelected ?
- ManualConfiguration ?
- FileToChange ?
- KeyToChange ?
- KeyToChangeValue ? (User configured value)
- IsConnectedWith ?

## Integration with Wizard

The `WizardForm` automatically calls `SaveInstallationToRegistry()` when the user clicks "Finish":

```csharp
// In WizardForm.cs
private void nextBtn_Click(object sender, EventArgs e)
{
    // ... validation ...
    
    if (currentStep == lastStep)
    {
     // Save to registry
        SaveInstallationToRegistry();
        
        MessageBox.Show("Installation Complete!");
        Close();
    }
}
```

## Testing

### View Registry Values
1. Open Registry Editor (`regedit`)
2. Navigate to `HKEY_LOCAL_MACHINE\SOFTWARE\HuLoop`
3. View all saved values

### Manual Test
```csharp
// Test save and read
var registryManager = new RegistryManager();

// Save
registryManager.SaveInstallationToRegistry(myState);

// Read back
var retrievedState = registryManager.ReadInstallationFromRegistry();

// Verify
Assert.AreEqual(myState.AppDir, retrievedState.AppDir);
Assert.AreEqual(myState.SelectedComponents.Count, retrievedState.SelectedComponents.Count);
```

## Best Practices

1. **Always check if installed** before attempting to install again
2. **Handle registry access failures** gracefully
3. **Store installation date** for audit/tracking purposes
4. **Update registry** when configuration changes
5. **Remove registry entries** during uninstallation
6. **Use consistent key names** across versions
7. **Validate data** when reading from registry

## Future Enhancements

Potential additions:
- Store installation logs path
- Track update history
- Store license information
- Save telemetry preferences
- Record installation language/locale
- Store custom installation options

## Support for Unattended Installation

The registry data enables:
- Silent uninstallation
- Configuration management scripts
- System inventory tools
- Automated upgrade checks
- Centralized management tools

---

**Created:** 2024
**Version:** 1.0
**Component:** Installer.Core.RegistryManager
