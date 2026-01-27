using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Installer.Core;
using Installer.Models;

namespace Installer.UI
{
    public partial class DiskSpaceStep : StepBase
    {
        private Label lblRequiredSpace = null!;
        private Label lblAvailableSpace = null!;
        private Label lblStatus = null!;
        private TextBox txtInstallPath = null!;
        private Button btnBrowse = null!;
        private Panel mainPanel = null!;

        public DiskSpaceStep()
        {
            InitializeComponent();
            Title.Text = "Disk Space Check";
            Description.Text = "Verify that you have sufficient disk space for the selected components.";
            SetupControls();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                // Refresh when step becomes visible
                RefreshData();
            }
        }

        public void RefreshData()
        {
            UpdateDiskSpaceInfo();
        }

        private void SetupControls()
        {
            // Create main content panel
            mainPanel = new Panel
            {
                //Location = new Point(40, 20),
                //Size = new Size(820, 360),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = WizardTheme.Colors.Surface
            };

            // Add border to content panel
            mainPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(WizardTheme.Colors.BorderLight, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, mainPanel.Width - 1, mainPanel.Height - 1);
                }
            };

            // Install Path Section
            Label lblPathTitle = WizardTheme.CreateSectionLabel("Installation Location");
            lblPathTitle.Location = new Point(20, 20);

            Label lblPath = new Label
            {
                Text = "Install Path:",
                Location = new Point(20, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = WizardTheme.Colors.TextPrimary
            };

            // Install Path TextBox
            txtInstallPath = new TextBox
            {
                Width = 550,
                Location = new Point(120, 53),
                Font = new Font("Segoe UI", 10F),
                Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "HuLoop"),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtInstallPath.TextChanged += OnInstallPathChanged;
            WizardTheme.StyleTextBox(txtInstallPath);

            // Browse Button
            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new Point(680, 51),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9F),
                BackColor = WizardTheme.Colors.Surface,
                ForeColor = WizardTheme.Colors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderColor = WizardTheme.Colors.BorderMedium;
            btnBrowse.Click += OnBrowseClick;

            // Disk Space Section
            Label lblSpaceTitle = WizardTheme.CreateSectionLabel("Disk Space Information");
            lblSpaceTitle.Location = new Point(20, 110);

            // Required Space Label
            lblRequiredSpace = new Label
            {
                Text = "Required Space: Calculating...",
                Location = new Point(20, 145),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = WizardTheme.Colors.TextPrimary
            };

            // Available Space Label
            lblAvailableSpace = new Label
            {
                Text = "Available Space: Calculating...",
                Location = new Point(20, 175),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = WizardTheme.Colors.TextPrimary
            };

            // Status Panel with background
            Panel statusPanel = new Panel
            {
                Location = new Point(20, 215),
                Size = new Size(760, 120),
                BackColor = WizardTheme.Colors.SurfaceSecondary,
                Padding = new Padding(15)
            };

            statusPanel.Paint += (s, e) =>
       {
           using (Pen pen = new Pen(WizardTheme.Colors.BorderLight, 1))
           {
               e.Graphics.DrawRectangle(pen, 0, 0, statusPanel.Width - 1, statusPanel.Height - 1);
           }
       };

            // Status Label
            lblStatus = new Label
            {
                Location = new Point(15, 15),
                Size = new Size(730, 90),
                Font = new Font("Segoe UI", 10F),
                ForeColor = WizardTheme.Colors.Success,
                Text = "Checking disk space...",
                AutoSize = false
            };

            statusPanel.Controls.Add(lblStatus);

            // Add all controls to main panel
            mainPanel.Controls.Add(lblPathTitle);
            mainPanel.Controls.Add(lblPath);
            mainPanel.Controls.Add(txtInstallPath);
            mainPanel.Controls.Add(btnBrowse);
            mainPanel.Controls.Add(lblSpaceTitle);
            mainPanel.Controls.Add(lblRequiredSpace);
            mainPanel.Controls.Add(lblAvailableSpace);
            mainPanel.Controls.Add(statusPanel);

            // Add main panel to ContentPanel from StepBase
            ContentPanel.Controls.Add(mainPanel);
        }

        private void OnBrowseClick(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select installation directory",
                ShowNewFolderButton = true,
                SelectedPath = txtInstallPath.Text
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtInstallPath.Text = dialog.SelectedPath;
            }
        }

        private void OnInstallPathChanged(object? sender, EventArgs e)
        {
            UpdateDiskSpaceInfo();
        }

        private void UpdateDiskSpaceInfo()
        {
            try
      {
   // Get selected components from InstallerState
       var state = GetInstallerState();

        if (state?.SelectedComponents == null || state.SelectedComponents.Count == 0)
     {
     lblRequiredSpace.Text = "Required Space: No components selected";
     lblAvailableSpace.Text = "Available Space: -";
        lblStatus.Text = "⚠ Please select components in the previous step.";
        lblStatus.ForeColor = WizardTheme.Colors.Warning;
   return;
  }

            // Use platform-specific sizes
long requiredSpaceMB = state.SelectedComponents.Sum(c => c.GetSizeForCurrentPlatform());

      // Add 10% buffer for temporary files and installation overhead
    long requiredSpaceWithBuffer = (long)(requiredSpaceMB * 1.1);

     // Format required space
     string requiredSpaceText = FormatDiskSpace(requiredSpaceWithBuffer);
          lblRequiredSpace.Text = $"Required Space: {requiredSpaceText}";

     // Get available disk space
           string installPath = txtInstallPath.Text;
    if (!string.IsNullOrWhiteSpace(installPath))
     {
      string driveLetter = Path.GetPathRoot(installPath) ?? "C:\\";
      string driveName = GetDriveName(driveLetter);
    var driveInfo = new DriveInfo(driveLetter);

 if (!driveInfo.IsReady)
       {
              lblAvailableSpace.Text = $"Available Space: {driveName} is not ready";
       lblStatus.Text = $"✗ WARNING: The selected drive is not accessible.\n\nPlease choose a different installation location.";
lblStatus.ForeColor = WizardTheme.Colors.Error;
return;
 }

  long availableSpaceMB = driveInfo.AvailableFreeSpace / (1024 * 1024);

        // Format available space
              string availableSpaceText = FormatDiskSpace(availableSpaceMB);
     lblAvailableSpace.Text = $"Available Space on {driveName}: {availableSpaceText}";

       // Check if there's enough space
    if (availableSpaceMB >= requiredSpaceWithBuffer)
       {
      long remainingSpace = availableSpaceMB - requiredSpaceWithBuffer;
       string remainingSpaceText = FormatDiskSpace(remainingSpace);
        lblStatus.Text = $"✓ Disk space verification successful.\n\n{remainingSpaceText} will remain available on {driveName} after installation.";
           lblStatus.ForeColor = WizardTheme.Colors.Success;
    }
      else
      {
      long shortage = requiredSpaceWithBuffer - availableSpaceMB;
              string shortageText = FormatDiskSpace(shortage);
      lblStatus.Text = $"✗ WARNING: Insufficient disk space on {driveName}\n\nAn additional {shortageText} is required.\n\nPlease free up disk space or select a different installation location.";
   lblStatus.ForeColor = WizardTheme.Colors.Error;
              }
    }
            }
     catch (Exception ex)
  {
         lblStatus.Text = $"✗ ERROR: Unable to verify disk space.\n\n{ex.Message}";
     lblStatus.ForeColor = WizardTheme.Colors.Error;
   }
        }

        /// <summary>
        /// Formats disk space in MB or GB depending on size
        /// Shows GB if size is 1000 MB or more
        /// </summary>
        private string FormatDiskSpace(long spaceMB)
        {
            if (spaceMB >= 1000)
            {
                double spaceGB = spaceMB / 1024.0;
                return $"{spaceGB:F2} GB";
            }
            else
            {
                return $"{spaceMB:N0} MB";
            }
        }

        /// <summary>
        /// Converts drive letter to friendly name
        /// Example: "C:\" becomes "C Drive"
        /// </summary>
        private string GetDriveName(string driveLetter)
        {
            // Remove trailing backslash and colon
            string letter = driveLetter.TrimEnd('\\', ':');

            if (string.IsNullOrEmpty(letter))
            {
                return "Drive";
            }

            return $"{letter} Drive";
        }

        private InstallerState? GetInstallerState()
        {
            // Find the parent wizard form and get the installer state
            var wizardForm = FindForm() as WizardForm;
            if (wizardForm != null)
            {
                return wizardForm.State;
            }
            return null;
        }

        public bool HasSufficientSpace()
        {
            try
            {
                var state = GetInstallerState();
  // Use platform-specific sizes
long requiredSpaceMB = (long)((state?.SelectedComponents?.Sum(c => c.GetSizeForCurrentPlatform()) ?? 0) * 1.1);

    string installPath = txtInstallPath.Text;
     if (!string.IsNullOrWhiteSpace(installPath))
   {
   string driveLetter = Path.GetPathRoot(installPath) ?? "C:\\";
   var driveInfo = new DriveInfo(driveLetter);

        if (!driveInfo.IsReady)
      return false;

      long availableSpaceMB = driveInfo.AvailableFreeSpace / (1024 * 1024);

     return availableSpaceMB >= requiredSpaceMB;
   }
      }
    catch
       {
return false;
   }
      return false;
        }

        public string GetInstallPath()
        {
            return txtInstallPath.Text;
        }
    }
}
