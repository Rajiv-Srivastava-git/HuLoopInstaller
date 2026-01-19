using System;
using System.Collections.Generic;
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
        private int _totalSteps = 0;
        private int _currentStepNumber = 0;

        public bool InstallationComplete => _installationComplete;
        public bool InstallationSuccessful { get; private set; }

        public InstallationProgressStep()
        {
            InitializeComponent();
            Title.Text = "Installing Components";
            Description.Text = "Please wait while the selected components are being installed...";
        }

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

                // Calculate total steps: download + extract + cleanup
                _totalSteps = (state.SelectedComponents.Count * 2) + 1; // download, extract per component, + cleanup
                _currentStepNumber = 0;

                // Phase 1: Download all components
                UpdatePhase("Downloading Components", "");
                var downloadTasks = new List<Task>();

                foreach (var component in state.SelectedComponents)
                {
                    await DownloadComponentAsync(component, state.AppDir);
                }

                // Phase 2: Extract all components
                UpdatePhase("Extracting Components", "");
                foreach (var zipPath in _downloadedFiles)
                {
                    await ExtractComponentAsync(zipPath, state.AppDir);
                }

                // Phase 3: Cleanup - Delete zip files
                UpdatePhase("Cleaning Up", "Removing temporary files...");
                await CleanupZipFilesAsync();

                // Installation complete
                _installationComplete = true;
                InstallationSuccessful = true;
                UpdatePhase("Installation Complete", "All components have been installed successfully!");
                UpdateProgress(100);

                // Enable the Finish button
                EnableFinishButton();
            }
            catch (Exception ex)
            {
                InstallationSuccessful = false;
                ShowError($"Installation failed: {ex.Message}");
            }
        }

        private async Task DownloadComponentAsync(InstallerComponent component, string installPath)
        {
            _currentStepNumber++;
            var progress = (_currentStepNumber * 100) / _totalSteps;

            UpdatePhase("Downloading Components", component.ComponentName);
            UpdateProgress(progress);
            UpdateStatus($"Downloading {component.ComponentName}...");

            try
            {
                var fileName = Path.GetFileName(new Uri(component.DownloadUrl).LocalPath);
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"{component.ComponentId}.zip";
                }

                var zipPath = Path.Combine(installPath, fileName);
                Directory.CreateDirectory(installPath);

                // Download with progress
                using (var response = await _httpClient.GetAsync(component.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource!.Token))
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
                                UpdateStatus($"Downloading {component.ComponentName}: {downloadedMB:F2} MB / {totalMB:F2} MB ({percentComplete}%)");
                            }
                        }
                    }
                }

                _downloadedFiles.Add(zipPath);
                AppendToLog($"✓ Downloaded: {component.ComponentName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {component.ComponentName}: {ex.Message}", ex);
            }
        }

        private async Task ExtractComponentAsync(string zipPath, string installPath)
        {
            _currentStepNumber++;
            var progress = (_currentStepNumber * 100) / _totalSteps;

            var fileName = Path.GetFileNameWithoutExtension(zipPath);
            UpdatePhase("Extracting Components", fileName);
            UpdateProgress(progress);
            UpdateStatus($"Extracting {fileName}...");

            try
            {
                await Task.Run(() =>
           {
               var extractPath = Path.Combine(installPath, fileName);

               if (Directory.Exists(extractPath))
               {
                   Directory.Delete(extractPath, true);
               }

               ZipFile.ExtractToDirectory(zipPath, extractPath);
           }, _cancellationTokenSource!.Token);

                AppendToLog($"✓ Extracted: {fileName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract {fileName}: {ex.Message}", ex);
            }
        }

        private async Task CleanupZipFilesAsync()
        {
            _currentStepNumber++;
            var progress = (_currentStepNumber * 100) / _totalSteps;

            UpdatePhase("Cleaning Up", "Removing temporary files...");
            UpdateProgress(progress);

            try
            {
                await Task.Run(() =>
                 {
                     foreach (var zipPath in _downloadedFiles)
                     {
                         if (File.Exists(zipPath))
                         {
                             File.Delete(zipPath);
                             var fileName = Path.GetFileName(zipPath);
                             AppendToLog($"✓ Deleted: {fileName}");
                         }
                     }
                 }, _cancellationTokenSource!.Token);

                UpdateStatus("Cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                // Don't fail installation if cleanup fails
                AppendToLog($"⚠ Cleanup warning: {ex.Message}");
            }
        }

        private void UpdatePhase(string phase, string componentName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePhase(phase, componentName)));
                return;
            }

            lblPhase.Text = phase;
            lblComponentName.Text = componentName;
        }

        private void UpdateProgress(int percentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(percentage)));
                return;
            }

            progressBar.Value = Math.Min(Math.Max(percentage, 0), 100);
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(message)));
                return;
            }

            lblCurrentOperation.Text = message;
        }

        private void AppendToLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendToLog(message)));
                return;
            }

            if (lblStatus.Text.Length > 0)
            {
                lblStatus.Text += Environment.NewLine;
            }
            lblStatus.Text += message;

            // Auto-scroll to bottom
            if (lblStatus.Text.Split('\n').Length > 8)
            {
                var lines = lblStatus.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                lblStatus.Text = string.Join(Environment.NewLine, lines.TakeLast(8));
            }
        }

        private void ShowError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowError(message)));
                return;
            }

            lblPhase.Text = "Installation Failed";
            lblPhase.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69); // Red
            lblCurrentOperation.Text = message;
            lblStatus.Text += Environment.NewLine + $"✗ Error: {message}";

            EnableFinishButton();
        }

        private void EnableFinishButton()
        {
            var wizardForm = FindForm() as WizardForm;
            if (wizardForm != null)
            {
                var nextBtn = wizardForm.Controls.Find("nextBtn", true).FirstOrDefault() as Button;
                if (nextBtn != null)
                {
                    nextBtn.Invoke(new Action(() =>
                        {
                            nextBtn.Enabled = true;
                            nextBtn.Text = "Finish";
                            nextBtn.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
                        }));
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
}
