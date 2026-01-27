using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Installer.Core;
using Installer.Models;

namespace Installer.UI
{
    public partial class InstallationProgressStep : StepBase
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _installationStarted = false;
        private bool _installationComplete = false;
        private List<string> _downloadedFiles = new List<string>();
        private Panel progressPanel = null!;
        private Label lblOverallStatus = null!;
        private ProgressBar overallProgressBar = null!;
        private Panel logPanel = null!;
        private TextBox txtLog = null!;
        private Dictionary<string, ComponentProgressControl> componentProgress = new();

        public bool InstallationComplete => _installationComplete;
        public bool InstallationSuccessful { get; private set; }

        public InstallationProgressStep()
        {
            InitializeComponent();
            Title.Text = "Installing Components";
            Description.Text = "Please wait while the selected components are being installed";
            BuildLayout();
        }

        private void BuildLayout()
        {
            var host = ContentPanel;
            host.SuspendLayout();
            host.Controls.Clear();

            int margin = 20;

            // ==============================
            // Bottom container (NO OVERLAP)
            // ==============================
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(margin, 8, margin, 10),
                BackColor = Color.Transparent
            };

            lblOverallStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = WizardTheme.Colors.Primary,
                Text = "Preparing installation...",
                TextAlign = ContentAlignment.MiddleLeft
            };

            overallProgressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 18,
                Minimum = 0,
                Maximum = 100,
                Top = lblOverallStatus.Top + lblOverallStatus.Height + 10,
                Style = ProgressBarStyle.Continuous
            };
            WizardTheme.StyleProgressBar(overallProgressBar);

            bottomPanel.Controls.Add(overallProgressBar);
            bottomPanel.Controls.Add(lblOverallStatus);

            // ==============================
            // Progress panel (fills rest)
            // ==============================
            progressPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(margin),
                BackColor = WizardTheme.Colors.Surface,
                AutoScroll = true
            };

            progressPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(WizardTheme.Colors.BorderLight);
                var rect = progressPanel.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                e.Graphics.DrawRectangle(pen, rect);
            };

            // ORDER MATTERS
            host.Controls.Add(progressPanel);
            host.Controls.Add(bottomPanel);

            host.ResumeLayout();
        }


        //    private void BuildLayout()
        //    {
        //        var host = ContentPanel;
        //        host.Controls.Clear();

        //        // Main progress panel with proper margins on all sides
        //        progressPanel = new Panel
        //        {
        //            Location = new Point(20, 20),
        //            Size = new Size(820, 250),  // Reduced from 860 to 820 to add right margin
        //            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
        //            BackColor = WizardTheme.Colors.Surface,
        //            AutoScroll = true
        //        };

        //        progressPanel.Paint += (s, e) =>
        //{
        //    using var pen = new Pen(WizardTheme.Colors.BorderLight);
        //    e.Graphics.DrawRectangle(pen, 0, 0, progressPanel.Width - 1, progressPanel.Height - 1);
        //};

        //        // Overall status at bottom
        //        lblOverallStatus = new Label
        //        {
        //            Location = new Point(20, 280),
        //            Size = new Size(820, 25),  // Match panel width
        //            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        //            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        //            ForeColor = WizardTheme.Colors.Primary,
        //            Text = "Preparing installation..."
        //        };

        //        // Overall progress bar
        //        overallProgressBar = new ProgressBar
        //        {
        //            Location = new Point(20, 315),
        //            Size = new Size(820, 20),  // Match panel width
        //            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        //            Style = ProgressBarStyle.Continuous,
        //            Maximum = 100,
        //            Minimum = 0
        //        };
        //        WizardTheme.StyleProgressBar(overallProgressBar);

        //        host.Controls.Add(progressPanel);
        //        host.Controls.Add(lblOverallStatus);
        //        host.Controls.Add(overallProgressBar);
        //    }

        /// <summary>
        /// Starts the installation process. Called by WizardForm when step is shown.
        /// </summary>
        public void StartInstallation()
        {
            if (!_installationStarted)
            {
                _installationStarted = true;
                _ = StartInstallationAsync();
            }
        }

        private async Task StartInstallationAsync()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var state = GetInstallerState();

                if (state == null || state.SelectedComponents.Count == 0)
                {
                    ShowError("No components selected for installation.");
                    return;
                }

                // Create progress controls for each component
                CreateComponentProgressControls(state.SelectedComponents);

                // Phase 1: Download all components in parallel
                UpdateOverallStatus("Phase 1/4: Downloading Components...", 0);
                var downloadTasks = state.SelectedComponents
                 .Select(component => DownloadComponentAsync(component, state.AppDir))
                       .ToList();

                await Task.WhenAll(downloadTasks);
                UpdateOverallStatus("Phase 1/4: Download Complete ✓", 25);

                // Phase 2: Extract all components
                UpdateOverallStatus("Phase 2/4: Extracting Components...", 25);
                var extractTasks = _downloadedFiles
                   .Select(zipPath => ExtractComponentAsync(zipPath, state.AppDir))
              .ToList();

                await Task.WhenAll(extractTasks);
                UpdateOverallStatus("Phase 2/4: Extraction Complete ✓", 50);

                // Phase 3: Cleanup
                UpdateOverallStatus("Phase 3/4: Cleaning Up Temporary Files...", 50);
                await CleanupZipFilesAsync();
                UpdateOverallStatus("Phase 3/4: Cleanup Complete ✓", 75);

                // Phase 4: Apply Configurations
                UpdateOverallStatus("Phase 4/4: Applying Configurations...", 75);
                await ApplyConfigurationsAsync(state);
                UpdateOverallStatus("Phase 4/4: Configuration Complete ✓", 95);

                // Installation complete
                _installationComplete = true;
                InstallationSuccessful = true;
                UpdateOverallStatus("✓ Installation Complete - All phases finished successfully!", 100);

                EnableFinishButton();
            }
            catch (Exception ex)
            {
                InstallationSuccessful = false;
                ShowError($"Installation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies configuration changes to component configuration files
        /// </summary>
        private async Task ApplyConfigurationsAsync(InstallerState state)
        {
            await Task.Run(() =>
            {
                try
                {
                    var componentsNeedingConfig = state.SelectedComponents
                       .Where(c => c.ManualConfiguration)
                   .ToList();

                    if (componentsNeedingConfig.Count == 0)
                    {
                        UpdateOverallStatus("Phase 4/4: No configuration required", 75);
                        return;
                    }

                    var configWriter = new ConfigurationWriter();

                    foreach (var component in componentsNeedingConfig)
                    {
                        if (componentProgress.TryGetValue(component.ComponentId, out var progressControl))
                        {
                            progressControl.SetPhase("Configuring");
                        }

                        UpdateOverallStatus($"Phase 4/4: Configuring {component.ComponentName}...", 75);

                        bool success = ApplyComponentConfiguration(configWriter, component, state.AppDir);

                        if (success && componentProgress.TryGetValue(component.ComponentId, out var control))
                        {
                            control.SetComplete("Configured");
                        }
                    }

                    UpdateOverallStatus("Phase 4/4: All configurations applied ✓", 95);
                }
                catch (Exception ex)
                {
                    UpdateOverallStatus($"Phase 4/4: Configuration warning - {ex.Message}", 85);
                }
            }, _cancellationTokenSource!.Token);
        }

        /// <summary>
        /// Applies configuration for a single component
        /// </summary>
        private bool ApplyComponentConfiguration(ConfigurationWriter configWriter, InstallerComponent component, string installPath)
        {
            // Get platform-specific config target
            string configTarget = component.GetConfigTargetForCurrentPlatform();

            if (string.IsNullOrEmpty(configTarget))
            {
                return true; // No config to apply
            }

            try
            {
                // Build full path to config file
                string componentFolder = GetComponentFolderName(component);
                string configFilePath = Path.Combine(installPath, componentFolder, configTarget.TrimStart('\\', '/'));

                if (!File.Exists(configFilePath))
                {
                    UpdateOverallStatus($"Warning: Config file not found - {configFilePath}", 80);
                    return false;
                }

                // Apply configuration based on format
                switch (component.ConfigFormat.ToLowerInvariant())
                {
                    case "json":
                        return ApplyJsonConfiguration(configFilePath, component);
                    case "ini":
                        return ApplyIniConfiguration(configFilePath, component);
                    default:
                        return true;
                }
            }
            catch (Exception ex)
            {
                UpdateOverallStatus($"Config error for {component.ComponentName}: {ex.Message}", 80);
                return false;
            }
        }

        /// <summary>
        /// Gets the component folder name from the download URL
        /// </summary>
        private string GetComponentFolderName(InstallerComponent component)
        {
            string downloadUrl = component.GetDownloadUrlForCurrentPlatform();
            string fileName = Path.GetFileNameWithoutExtension(new Uri(downloadUrl).LocalPath);

            // Remove platform suffixes if present
            if (fileName.Contains('-'))
            {
                fileName = fileName.Substring(0, fileName.IndexOf('-'));
            }

            return fileName;
        }

        /// <summary>
        /// Applies JSON configuration
        /// </summary>
        private bool ApplyJsonConfiguration(string configFilePath, InstallerComponent component)
        {
            try
            {
                string existingJson = File.ReadAllText(configFilePath);
                var userValues = component.ConfigurationValues;

                if (component.ConfigurationMode.Equals("Overwrite", StringComparison.OrdinalIgnoreCase))
                {
                    // Overwrite mode
                    var newConfig = ConfigurationHelper.UnflattenConfiguration(userValues);
                    string newJson = System.Text.Json.JsonSerializer.Serialize(newConfig,
              new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(configFilePath, newJson);
                }
                else
                {
                    // Patch mode - merge values
                    var existingDoc = System.Text.Json.JsonDocument.Parse(existingJson);
                    var existingDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                                 existingDoc.RootElement.GetRawText());

                    // Apply user values
                    foreach (var kvp in userValues)
                    {
                        if (!string.IsNullOrWhiteSpace(kvp.Value))
                        {
                            // Set nested value in dictionary
                            SetNestedJsonValue(existingDict, kvp.Key, kvp.Value);
                        }
                    }

                    string patchedJson = System.Text.Json.JsonSerializer.Serialize(existingDict,
                              new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(configFilePath, patchedJson);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sets a nested value in a dictionary using a flattened key path
        /// </summary>
        private void SetNestedJsonValue(Dictionary<string, object>? dict, string flatKey, string value)
        {
            if (dict == null) return;

            var keys = ConfigurationHelper.SplitKey(flatKey);

            if (keys.Length == 1)
            {
                dict[keys[0]] = value;
                return;
            }

            // Navigate to nested location
            object current = dict;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                string key = keys[i];

                if (ConfigurationHelper.IsArrayIndex(key, out string arrayName, out int index))
                {
                    if (current is Dictionary<string, object> currentDict)
                    {
                        if (!currentDict.ContainsKey(arrayName))
                            currentDict[arrayName] = new List<object>();

                        if (currentDict[arrayName] is List<object> list)
                        {
                            while (list.Count <= index)
                                list.Add(new Dictionary<string, object>());
                            current = list[index];
                        }
                    }
                }
                else
                {
                    if (current is Dictionary<string, object> currentDict)
                    {
                        if (!currentDict.ContainsKey(key))
                            currentDict[key] = new Dictionary<string, object>();
                        current = currentDict[key];
                    }
                }
            }

            // Set final value
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

                    bool found = false;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string line = lines[i].Trim();

                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#"))
                            continue;

                        if (line.StartsWith(key + "=") || line.StartsWith(key + " ="))
                        {
                            lines[i] = $"{key}={value}";
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        lines.Add($"{key}={value}");
                    }
                }

                File.WriteAllLines(configFilePath, lines);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task DownloadComponentAsync(InstallerComponent component, string installPath)
        {
            var progressControl = componentProgress[component.ComponentId];
            progressControl.SetPhase("Downloading");

            try
            {
                // Get platform-specific download URL
                string downloadUrl = component.GetDownloadUrlForCurrentPlatform();

                var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"{component.ComponentId}.zip";
                }

                var zipPath = Path.Combine(installPath, fileName);
                Directory.CreateDirectory(installPath);

                using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource!.Token))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1;
                    var totalMB = totalBytes > 0 ? totalBytes / 1024.0 / 1024.0 : 0;

                    using (var contentStream = await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token))
                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
                            totalRead += bytesRead;

                            if (totalBytes > 0)
                            {
                                var downloadedMB = totalRead / 1024.0 / 1024.0;
                                var percentComplete = (int)((totalRead * 100) / totalBytes);
                                progressControl.UpdateProgress(percentComplete, $"{downloadedMB:F1} MB / {totalMB:F1} MB");
                            }
                        }
                    }
                }

                lock (_downloadedFiles)
                {
                    _downloadedFiles.Add(zipPath);
                }

                progressControl.SetComplete($"✓ Downloaded ({FormatSize(component.GetSizeForCurrentPlatform())})");
            }
            catch (Exception ex)
            {
                progressControl.SetError($"Download failed: {ex.Message}");
                throw new Exception($"Failed to download {component.ComponentName}: {ex.Message}", ex);
            }
        }

        private async Task ExtractComponentAsync(string zipPath, string installPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(zipPath);

            // Find the corresponding component progress control
            InstallerComponent? matchingComponent = null;
            ComponentProgressControl? component = null;

            foreach (var kvp in componentProgress)
            {
                if (kvp.Value.ComponentName.Contains(fileName.Split('-')[0]))
                {
                    matchingComponent = GetInstallerState()?.SelectedComponents
                  .FirstOrDefault(c => c.ComponentId == kvp.Key);
                    component = kvp.Value;
                    break;
                }
            }

            if (component != null)
            {
                component.SetPhase("Extracting");
            }

            try
            {
                await Task.Run(() =>
                {
                    var extractPath = Path.Combine(installPath, fileName.Split('-')[0]);

                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true);
                    }

                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                }, _cancellationTokenSource!.Token);

                if (component != null)
                {
                    component.SetComplete("✓ Extracted");
                }
            }
            catch (Exception ex)
            {
                if (component != null)
                {
                    component.SetError($"Extract failed: {ex.Message}");
                }
                throw new Exception($"Failed to extract {fileName}: {ex.Message}", ex);
            }
        }

        private string FormatSize(long sizeMB)
        {
            if (sizeMB >= 1000)
            {
                double sizeGB = sizeMB / 1024.0;
                return $"{sizeGB:F2} GB";
            }
            return $"{sizeMB} MB";
        }

        private async Task CleanupZipFilesAsync()
        {
            try
            {
                await Task.Run(() =>
         {
             int filesDeleted = 0;
             int totalFiles = _downloadedFiles.Count;

             foreach (var zipPath in _downloadedFiles)
             {
                 if (File.Exists(zipPath))
                 {
                     File.Delete(zipPath);
                     filesDeleted++;
                     var fileName = Path.GetFileName(zipPath);

                     // Update progress for each file deleted
                     UpdateOverallStatus($"Phase 3/4: Cleaning up... ({filesDeleted}/{totalFiles} files)",
            50 + (filesDeleted * 25 / totalFiles));
                 }
             }
         }, _cancellationTokenSource!.Token);

                UpdateOverallStatus($"Phase 3/4: Cleanup Complete ✓ ({_downloadedFiles.Count} files removed)", 75);
            }
            catch (Exception ex)
            {
                // Don't fail installation if cleanup fails
                UpdateOverallStatus($"Phase 3/4: Cleanup warning - {ex.Message}", 75);
            }
        }

        private void CreateComponentProgressControls(List<InstallerComponent> components)
        {
            progressPanel.SuspendLayout();
            progressPanel.Controls.Clear();
            componentProgress.Clear();

            int y = 15;  // Start with small top margin

            foreach (var component in components)
            {
                var progressControl = new ComponentProgressControl(component.ComponentName);
                progressControl.Location = new Point(15, y);
                progressControl.Width = progressPanel.ClientSize.Width - 30;  // Use ClientSize and subtract margins
                progressControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                progressPanel.Controls.Add(progressControl);
                componentProgress[component.ComponentId] = progressControl;

                y += progressControl.Height + 10;
            }

            progressPanel.ResumeLayout();
        }

        private void UpdateOverallStatus(string message, int percentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateOverallStatus(message, percentage)));
                return;
            }

            lblOverallStatus.Text = message;
            overallProgressBar.Value = Math.Min(Math.Max(percentage, 0), 100);
        }

        private void ShowError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowError(message)));
                return;
            }

            lblOverallStatus.Text = "✗ Installation Failed";
            lblOverallStatus.ForeColor = WizardTheme.Colors.Error;

            MessageBox.Show(message, "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            EnableFinishButton();
        }

        private void EnableFinishButton()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(EnableFinishButton));
                return;
            }

            var wizardForm = FindForm() as WizardForm;
            if (wizardForm != null)
            {
                var nextBtn = wizardForm.Controls.Find("nextBtn", true).FirstOrDefault() as Button;
                if (nextBtn != null)
                {
                    nextBtn.Enabled = true;
                    nextBtn.Text = "Finish";
                    nextBtn.BackColor = WizardTheme.Colors.Primary;
                }
            }
        }

        private InstallerState? GetInstallerState()
        {
            var wizardForm = FindForm() as WizardForm;
            return wizardForm?.State;
        }

        ~InstallationProgressStep()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _httpClient?.Dispose();
        }
    }

    // Component progress control for individual component display
    public class ComponentProgressControl : Panel
    {
        private Label lblName;
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblPhase;

        public string ComponentName { get; }

        public ComponentProgressControl(string componentName)
        {
            ComponentName = componentName;
            Height = 70;
            BackColor = Color.FromArgb(250, 250, 250);
            Padding = new Padding(10);

            // Component name
            lblName = new Label
            {
                Text = componentName,
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = WizardTheme.Colors.TextPrimary
            };

            // Phase label (Downloading, Extracting, Complete)
            lblPhase = new Label
            {
                Text = "Waiting...",
                Location = new Point(10, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = WizardTheme.Colors.TextSecondary
            };

            // Progress bar - use percentage of width instead of fixed size
            progressBar = new ProgressBar
   {
        Location = new Point(10, 45),
       Width = Math.Max(200, this.Width - 200),  // Dynamic width, minimum 200px
     Height = 18,
  Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
    };

     // Status label (progress details)
        lblStatus = new Label
       {
                Text = "",
                AutoSize = true,
    Font = new Font("Segoe UI", 8),
      ForeColor = WizardTheme.Colors.TextTertiary,
    MaximumSize = new Size(200, 0)  // Limit width
     };
            
       // Position status label after progressBar is sized
            this.Resize += (s, e) => {
            progressBar.Width = Math.Max(200, this.ClientSize.Width - 250);
 lblStatus.Location = new Point(progressBar.Right + 10, 45);
          };

Controls.Add(lblName);
            Controls.Add(lblPhase);
 Controls.Add(progressBar);
        Controls.Add(lblStatus);
        }

        public void SetPhase(string phase)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetPhase(phase)));
                return;
            }

            lblPhase.Text = phase;
            lblPhase.ForeColor = WizardTheme.Colors.Primary;
        }

        public void UpdateProgress(int percentage, string details)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(percentage, details)));
                return;
            }

            progressBar.Value = Math.Min(Math.Max(percentage, 0), 100);
 lblStatus.Text = details;
     }

        public void SetComplete(string message)
        {
   if (InvokeRequired)
            {
         Invoke(new Action(() => SetComplete(message)));
     return;
   }

 lblPhase.Text = message;
            lblPhase.ForeColor = WizardTheme.Colors.Success;
            progressBar.Value = 100;
 lblStatus.Text = "Complete";
     }

        public void SetError(string message)
    {
            if (InvokeRequired)
            {
    Invoke(new Action(() => SetError(message)));
       return;
      }

       lblPhase.Text = $"✗ Error";
    lblPhase.ForeColor = WizardTheme.Colors.Error;
            lblStatus.Text = message;
 }
    }
}
