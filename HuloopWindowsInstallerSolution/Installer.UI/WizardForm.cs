using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Installer.Core;

namespace Installer.UI
{
    public partial class WizardForm : Form
    {
        private List<UserControl> steps = new();
        private int currentStep = 0;

        public InstallerState State { get; private set; } = new InstallerState();

        public WizardForm()
        {
            InitializeComponent();
            WizardTheme.ApplyTheme(this);
            LoadSteps();
            ShowStep(0);
        }

        private void LoadSteps()
        {
            steps.Add(new WelcomeStep());
            steps.Add(new EulaStep());
            steps.Add(new ComponentSelectionStep());
            steps.Add(new DiskSpaceStep());
            steps.Add(new ConfigStep());
            steps.Add(new InstallationProgressStep()); // Add installation progress step
        }

        private void ShowStep(int index)
        {
            bodyPanel.Controls.Clear();
            var step = steps[index];
            step.Dock = DockStyle.Fill;
            bodyPanel.Controls.Add(step);

            // Hide Back button on first step (Welcome step)
            if (index == 0)
            {
                prevBtn.Visible = false;
            }
            else
            {
                prevBtn.Visible = true;
                prevBtn.Enabled = true;
            }

            nextBtn.Text = index == steps.Count - 1 ? "Finish" : "Next";

            // Handle EulaStep - disable Next until accepted
            if (step is EulaStep eulaStep)
            {
                // Next button will be managed by EulaStep checkbox
                nextBtn.Enabled = eulaStep.IsAccepted;
                nextBtn.BackColor = eulaStep.IsAccepted
                    ? Color.FromArgb(0, 122, 204)
                    : Color.Gray;
            }
            // Refresh data for DiskSpaceStep when it's shown
            else if (step is DiskSpaceStep diskSpaceStep)
            {
                diskSpaceStep.RefreshData();
            }

            // Refresh data for ConfigStep when it's shown
            else if (step is ConfigStep configStep)
            {
                configStep.RefreshData();
                // ConfigStep will manage the button state itself
            }
            // Handle InstallationProgressStep
            else if (step is InstallationProgressStep progressStep)
            {
                // Disable navigation buttons during installation
                prevBtn.Visible = false;  // Also hide back button during installation
                nextBtn.Enabled = false;
                nextBtn.Text = "Finish";

                // Start installation immediately
                progressStep.StartInstallation();
            }
            else
            {
                // Re-enable button for other steps
                nextBtn.Enabled = true;
                nextBtn.BackColor = Color.FromArgb(0, 122, 204);
            }
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            // Validate and collect data from current step before proceeding
            if (!ValidateAndCollectCurrentStepData())
                return;

            if (currentStep < steps.Count - 1)
            {
                currentStep++;
                ShowStep(currentStep);
            }
            else
            {
                // Check if we're on the installation progress step
                if (steps[currentStep] is InstallationProgressStep progressStep)
                {
                    if (!progressStep.InstallationComplete)
                    {
                        MessageBox.Show(
                            "Installation is still in progress. Please wait...",
                            "Installation In Progress",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    if (!progressStep.InstallationSuccessful)
                    {
                        var result = MessageBox.Show(
                            "Installation failed. Do you want to close the installer?",
                            "Installation Failed",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Error);

                        if (result == DialogResult.Yes)
                        {
                            Close();
                        }
                        return;
                    }
                }

                // Installation complete - save to registry
                if (SaveInstallationToRegistry())
                {
                    MessageBox.Show(
                        "Installation completed successfully!\n\n" +
                        $"Installation path: {State.AppDir}\n" +
                        $"Components installed: {State.SelectedComponents.Count}\n\n" +
                        "Configuration has been saved to the Windows Registry.",
                        "Installation Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Installation completed but failed to save configuration to registry.\n" +
                        "The application may still work correctly.",
                        "Installation Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                Close();
            }
        }

        private bool SaveInstallationToRegistry()
        {
            try
            {
                var registryManager = new RegistryManager();
                return registryManager.SaveInstallationToRegistry(State);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save installation to registry: {ex.Message}",
                    "Registry Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private bool ValidateAndCollectCurrentStepData()
        {
            var currentStepControl = steps[currentStep];

            // Validate EulaStep - ensure license is accepted
            if (currentStepControl is EulaStep eulaStep)
            {
                if (!eulaStep.IsAccepted)
                {
                    MessageBox.Show(
                        "You must accept the license agreement to proceed.",
                        "License Agreement Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }
            }

            // Collect and validate ComponentSelectionStep
            if (currentStepControl is ComponentSelectionStep componentStep)
            {
                var selectedComponents = componentStep.GetSelectedComponents();
                if (selectedComponents.Count == 0)
                {
                    MessageBox.Show("Please select at least one component to install.", "No Components Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                State.SelectedComponents = selectedComponents;
            }

            // Validate DiskSpaceStep
            if (currentStepControl is DiskSpaceStep diskSpaceStep)
            {
                if (!diskSpaceStep.HasSufficientSpace())
                {
                    var result = MessageBox.Show(
                        "You do not have sufficient disk space for the selected components.\n\n" +
                        "Do you want to continue anyway? (Not recommended)",
                        "Insufficient Disk Space",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                        return false;
                }
                State.AppDir = diskSpaceStep.GetInstallPath();
            }

            // Validate ConfigStep
            if (currentStepControl is ConfigStep configStep)
            {
                if (!configStep.ValidateAndSaveConfiguration())
                {
                    return false;
                }
            }

            return true;
        }

        private void prevBtn_Click(object sender, EventArgs e)
        {
            if (currentStep > 0)
            {
                currentStep--;
                ShowStep(currentStep);
            }
        }
    }
}
