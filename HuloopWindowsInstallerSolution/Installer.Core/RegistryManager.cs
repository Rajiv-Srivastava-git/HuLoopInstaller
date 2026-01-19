using Microsoft.Win32;
using Installer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Installer.Core
{
    /// <summary>
    /// Manages Windows Registry operations for the HuLoop installer
    /// </summary>
    public class RegistryManager
    {
        private const string REGISTRY_BASE_PATH = @"SOFTWARE\HuLoop";
        private const string INSTALL_INFO_KEY = "InstallInfo";
        private const string COMPONENTS_KEY = "Components";

        /// <summary>
        /// Saves installation information to the Windows Registry
        /// </summary>
        /// <param name="state">The installer state containing all installation data</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SaveInstallationToRegistry(InstallerState state)
        {
            try
            {
                // Create or open the HuLoop registry key
                using (var huloopKey = Registry.LocalMachine.CreateSubKey(REGISTRY_BASE_PATH, true))
                {
                    if (huloopKey == null)
                    {
                        throw new Exception("Failed to create registry key");
                    }

                    // Save installation information
                    SaveInstallationInfo(huloopKey, state);

                    // Save component information
                    SaveComponentsInfo(huloopKey, state);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine($"ERROR: Failed to save to registry: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves general installation information
        /// </summary>
        private void SaveInstallationInfo(RegistryKey huloopKey, InstallerState state)
        {
            using (var installInfoKey = huloopKey.CreateSubKey(INSTALL_INFO_KEY, true))
            {
                if (installInfoKey == null) return;

                // Save installation path
                installInfoKey.SetValue("InstallPath", state.AppDir, RegistryValueKind.String);

                // Save API URL
                installInfoKey.SetValue("ApiUrl", state.ApiUrl, RegistryValueKind.String);

                // Save Server URL
                installInfoKey.SetValue("ServerUrl", state.ServerUrl, RegistryValueKind.String);

                // Save update channel
                installInfoKey.SetValue("UpdateChannel", state.UpdateChannel, RegistryValueKind.String);

                // Save HuLoop version
                installInfoKey.SetValue("Version", state.HuloopVersion, RegistryValueKind.String);

                // Save auto-update setting
                installInfoKey.SetValue("EnableAutoUpdate", state.EnableAutoUpdate ? 1 : 0, RegistryValueKind.DWord);

                // Save installation date
                installInfoKey.SetValue("InstallDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), RegistryValueKind.String);

                // Save number of components installed
                installInfoKey.SetValue("ComponentCount", state.SelectedComponents.Count, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Saves information about each installed component
        /// </summary>
        private void SaveComponentsInfo(RegistryKey huloopKey, InstallerState state)
        {
            using (var componentsKey = huloopKey.CreateSubKey(COMPONENTS_KEY, true))
            {
                if (componentsKey == null) return;

                foreach (var component in state.SelectedComponents)
                {
                    // Create a subkey for each component using its ComponentId
                    using (var componentKey = componentsKey.CreateSubKey(component.ComponentId, true))
                    {
                        if (componentKey == null) continue;

                        // Save component details
                        componentKey.SetValue("ComponentName", component.ComponentName, RegistryValueKind.String);
                        componentKey.SetValue("ComponentId", component.ComponentId, RegistryValueKind.String);
                        componentKey.SetValue("SizeMB", component.ComponentSizeMB, RegistryValueKind.QWord);
                        componentKey.SetValue("DownloadUrl", component.DownloadUrl, RegistryValueKind.String);
                        componentKey.SetValue("IsMandatory", component.IsMandatory ? 1 : 0, RegistryValueKind.DWord);
                        componentKey.SetValue("IsSelected", component.IsSelected ? 1 : 0, RegistryValueKind.DWord);

                        // Save configuration information if component has manual configuration
                        if (component.ManualConfiguration)
                        {
                            componentKey.SetValue("ManualConfiguration", 1, RegistryValueKind.DWord);
                            componentKey.SetValue("FileToChange", component.FileToChange, RegistryValueKind.String);
                            componentKey.SetValue("KeyToChange", component.KeyToChange, RegistryValueKind.String);

                            // Save the user-configured value
                            if (!string.IsNullOrWhiteSpace(component.KeyToChangeValue))
                            {
                                componentKey.SetValue("KeyToChangeValue", component.KeyToChangeValue, RegistryValueKind.String);
                            }
                        }
                        else
                        {
                            componentKey.SetValue("ManualConfiguration", 0, RegistryValueKind.DWord);
                        }

                        // Save connected components as comma-separated string
                        if (component.IsConnectedWith != null && component.IsConnectedWith.Count > 0)
                        {
                            componentKey.SetValue("ConnectedWith", string.Join(",", component.IsConnectedWith), RegistryValueKind.String);
                        }

                        // Save installation date for this component
                        componentKey.SetValue("InstallDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), RegistryValueKind.String);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves installation information from the registry
        /// </summary>
        /// <returns>InstallerState object with data from registry, or null if not found</returns>
        public InstallerState? ReadInstallationFromRegistry()
        {
            try
            {
                using (var huloopKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_PATH, false))
                {
                    if (huloopKey == null)
                    {
                        return null; // Not installed
                    }

                    var state = new InstallerState();

                    // Read installation info
                    using (var installInfoKey = huloopKey.OpenSubKey(INSTALL_INFO_KEY, false))
                    {
                        if (installInfoKey != null)
                        {
                            state.AppDir = installInfoKey.GetValue("InstallPath") as string ?? state.AppDir;
                            state.ApiUrl = installInfoKey.GetValue("ApiUrl") as string ?? state.ApiUrl;
                            state.ServerUrl = installInfoKey.GetValue("ServerUrl") as string ?? state.ServerUrl;
                            state.UpdateChannel = installInfoKey.GetValue("UpdateChannel") as string ?? state.UpdateChannel;
                            state.HuloopVersion = installInfoKey.GetValue("Version") as string ?? state.HuloopVersion;
                            state.EnableAutoUpdate = (int)(installInfoKey.GetValue("EnableAutoUpdate") ?? 1) == 1;
                        }
                    }

                    // Read components info
                    using (var componentsKey = huloopKey.OpenSubKey(COMPONENTS_KEY, false))
                    {
                        if (componentsKey != null)
                        {
                            foreach (var componentId in componentsKey.GetSubKeyNames())
                            {
                                using (var componentKey = componentsKey.OpenSubKey(componentId, false))
                                {
                                    if (componentKey == null) continue;

                                    var component = new InstallerComponent
                                    {
                                        ComponentId = componentKey.GetValue("ComponentId") as string ?? componentId,
                                        ComponentName = componentKey.GetValue("ComponentName") as string ?? string.Empty,
                                        ComponentSizeMB = (long)(componentKey.GetValue("SizeMB") ?? 0L),
                                        DownloadUrl = componentKey.GetValue("DownloadUrl") as string ?? string.Empty,
                                        IsMandatory = (int)(componentKey.GetValue("IsMandatory") ?? 0) == 1,
                                        IsSelected = (int)(componentKey.GetValue("IsSelected") ?? 0) == 1,
                                        ManualConfiguration = (int)(componentKey.GetValue("ManualConfiguration") ?? 0) == 1,
                                        FileToChange = componentKey.GetValue("FileToChange") as string ?? string.Empty,
                                        KeyToChange = componentKey.GetValue("KeyToChange") as string ?? string.Empty,
                                        KeyToChangeValue = componentKey.GetValue("KeyToChangeValue") as string ?? string.Empty
                                    };

                                    // Read connected components
                                    var connectedWith = componentKey.GetValue("ConnectedWith") as string;
                                    if (!string.IsNullOrWhiteSpace(connectedWith))
                                    {
                                        component.IsConnectedWith = connectedWith.Split(',').ToList();
                                    }

                                    state.SelectedComponents.Add(component);
                                }
                            }
                        }
                    }

                    return state;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to read from registry: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if HuLoop is already installed
        /// </summary>
        /// <returns>True if installation registry key exists</returns>
        public bool IsInstalled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_PATH, false))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the installed version from registry
        /// </summary>
        /// <returns>Version string or null if not found</returns>
        public string? GetInstalledVersion()
        {
            try
            {
                using (var huloopKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_PATH, false))
                {
                    if (huloopKey == null) return null;

                    using (var installInfoKey = huloopKey.OpenSubKey(INSTALL_INFO_KEY, false))
                    {
                        return installInfoKey?.GetValue("Version") as string;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the installation path from registry
        /// </summary>
        /// <returns>Installation path or null if not found</returns>
        public string? GetInstallPath()
        {
            try
            {
                using (var huloopKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_PATH, false))
                {
                    if (huloopKey == null) return null;

                    using (var installInfoKey = huloopKey.OpenSubKey(INSTALL_INFO_KEY, false))
                    {
                        return installInfoKey?.GetValue("InstallPath") as string;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Removes all HuLoop registry entries (for uninstallation)
        /// </summary>
        /// <returns>True if successful</returns>
        public bool RemoveInstallationFromRegistry()
        {
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(REGISTRY_BASE_PATH, false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to remove registry entries: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates a specific component's configuration value in the registry
        /// </summary>
        public bool UpdateComponentConfiguration(string componentId, string keyToChangeValue)
        {
            try
            {
                using (var huloopKey = Registry.LocalMachine.OpenSubKey(REGISTRY_BASE_PATH, true))
                {
                    if (huloopKey == null) return false;

                    using (var componentsKey = huloopKey.OpenSubKey(COMPONENTS_KEY, true))
                    {
                        if (componentsKey == null) return false;

                        using (var componentKey = componentsKey.OpenSubKey(componentId, true))
                        {
                            if (componentKey == null) return false;

                            componentKey.SetValue("KeyToChangeValue", keyToChangeValue, RegistryValueKind.String);
                            componentKey.SetValue("LastModified", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), RegistryValueKind.String);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to update component configuration: {ex.Message}");
                return false;
            }
        }
    }
}
