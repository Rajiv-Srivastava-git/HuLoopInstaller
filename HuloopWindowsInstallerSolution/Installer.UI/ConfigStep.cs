using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Installer.Core;
using Installer.Models;

namespace Installer.UI
{
    public partial class ConfigStep : StepBase
    {
        private Panel scrollPanel = null!;
        private Dictionary<string, TextBox> configTextBoxes = new();
        private Label lblInstallPath = null!;

        // Field descriptions to help users
        private static readonly Dictionary<string, string> FieldDescriptions = new()
    {
// General fields
     { "ApplicationName", "The name of the application as it will appear in the system" },
            { "SchedulerId", "Unique identifier for the scheduler (leave blank for auto-generation)" },
     { "Version", "Version number of the component" },

         // Trigger fields
       { "ExePath", "Full path to the executable that will be triggered (e.g., ..\\HuLoopCLI\\HuLoopCLI.exe)" },
            { "HostApiUrl", "API endpoint URL (e.g., https://qa.huloop.ai:8443/)" },
 { "LogFilePath", "Directory where log files will be stored (e.g., ..\\Logs\\Workflow\\)" },
       { "RunSchedule", "Cron expression: 5 space-separated values (month day hour minute second)" },
    { "TriggerType", "Type of trigger mechanism (Workflow or Automation)" },

   // Server fields
   { "ai.huloop.server.url", "HuLoop server URL for API communication" },
            { "Port", "Network port number for the service" },
     { "Timeout", "Connection timeout in seconds" },
      };

        // Read-only fields that should not be editable
        private static readonly HashSet<string> ReadOnlyFields = new()
        {
            "TriggerType",      // Already read-only
      "ApplicationName",  // NEW: Read-only for Scheduler
        "SchedulerId",  // NEW: Read-only for Scheduler
     "Version"           // NEW: Read-only for Scheduler
 };
        public ConfigStep()
        {
            InitializeComponent();

            Title.Text = "Component Configuration";
            Description.Text = "Configure settings for the selected components.";

            BuildLayout();
        }

        // ===================== LAYOUT =====================
        private void BuildLayout()
        {
            var host = ContentPanel;
            host.Controls.Clear();

            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = WizardTheme.Colors.Surface
            };

            // Optional border
            scrollPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(WizardTheme.Colors.BorderLight);
                e.Graphics.DrawRectangle(
          pen,
        0,
             0,
                scrollPanel.Width - 1,
       scrollPanel.Height - 1);
            };

            host.Controls.Add(scrollPanel);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                RefreshConfigurationFields();
        }

        public void RefreshData()
        {
            RefreshConfigurationFields();
        }

        // ===================== UI GENERATION =====================
        private void RefreshConfigurationFields()
        {
            scrollPanel.Controls.Clear();
            configTextBoxes.Clear();

            scrollPanel.SuspendLayout();
            scrollPanel.AutoScrollPosition = new Point(0, 0);

            var state = GetInstallerState();
            if (state == null)
            {
                AddLabel("ERROR: Unable to load installation state.", 10, true, WizardTheme.Colors.Error);
                scrollPanel.ResumeLayout();
                return;
            }

            int y = 12;

            // ---- Install path - Highlighted but not link-like ----
            AddLabel("Installation Path:", y, true);
            lblInstallPath = new Label
            {
                Text = state.AppDir,
                Location = new Point(160, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = WizardTheme.Colors.TextPrimary,
                BackColor = Color.FromArgb(255, 252, 231),   // Light yellow background
                Padding = new Padding(4, 2, 4, 2),
                BorderStyle = BorderStyle.FixedSingle
            };
            scrollPanel.Controls.Add(lblInstallPath);
            y += 40;

            var components = state.SelectedComponents
             .Where(c => c.ManualConfiguration && c.Configuration.Count > 0)
               .ToList();

            if (!components.Any())
            {
                AddLabel("No manual configuration is required for the selected components.", y, false, WizardTheme.Colors.Success);
                AddLabel("\nClick 'Next' to proceed with installation.", y + 30, false, WizardTheme.Colors.TextSecondary, 10, italic: true);
                scrollPanel.ResumeLayout();

                // IMPORTANT: Make sure Next button is enabled even if no config is needed
                UpdateFinishButtonState();
                return;
            }

            AddLabel("Configure the following settings (leave blank to use defaults):",
                       y, false, WizardTheme.Colors.TextTertiary, 10, italic: true);
            y += 28;

            AddLabel("Component Configuration Settings:", y, true, WizardTheme.Colors.TextPrimary, 10, fontSize: 11);
            y += 38;

            foreach (var component in components)
            {
                // Component header with nice styling
                var componentHeader = new Panel
                {
                    Location = new Point(10, y),
                    Size = new Size(760, 32),
                    BackColor = Color.FromArgb(240, 248, 255)  // Light blue background
                };

                var componentLabel = new Label
                {
                    Text = $"● {component.ComponentName}",
                    Location = new Point(8, 6),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = WizardTheme.Colors.TextPrimary,
                    BackColor = Color.Transparent
                };
                componentHeader.Controls.Add(componentLabel);
                scrollPanel.Controls.Add(componentHeader);
                y += 38;

                var flatConfig = ConfigurationHelper.FlattenConfiguration(component.Configuration);
                var keys = flatConfig.Keys.OrderBy(k => k).ToList();

                string lastArrayGroup = string.Empty;

                foreach (var key in keys)
                {
                    string value = flatConfig[key];

                    if (ConfigurationHelper.IsArrayItem(key))
                    {
                        string group = key.Substring(0, key.IndexOf(']') + 1);
                        if (group != lastArrayGroup)
                        {
                            // Array group header
                            var groupPanel = new Panel
                            {
                                Location = new Point(30, y),
                                Size = new Size(740, 24),
                                BackColor = Color.FromArgb(250, 250, 250)
                            };

                            var groupLabel = new Label
                            {
                                Text = $"  {ConfigurationHelper.GetHierarchicalDisplay(group)}",
                                Location = new Point(4, 3),
                                AutoSize = true,
                                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                                ForeColor = WizardTheme.Colors.TextSecondary,
                                BackColor = Color.Transparent
                            };
                            groupPanel.Controls.Add(groupLabel);
                            scrollPanel.Controls.Add(groupPanel);
                            y += 28;
                            lastArrayGroup = group;
                        }
                    }
                    else
                    {
                        lastArrayGroup = string.Empty;
                    }

                    string displayName = GetFriendlyLabel(key);

                    // Check if field is read-only (TriggerType, ApplicationName, SchedulerId, Version)
                    bool isReadOnly = ReadOnlyFields.Contains(displayName);

                    // Field label
                    var fieldLabel = new Label
                    {
                        Text = isReadOnly ? $"{displayName}: (Read-only)" : $"{displayName}:",
                        Location = new Point(40, y + 2),
                        AutoSize = false,
                        Width = 240,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = isReadOnly ? WizardTheme.Colors.Primary : WizardTheme.Colors.TextPrimary
                    };
                    scrollPanel.Controls.Add(fieldLabel);

                    if (isReadOnly)
                    {
                        // Read-only value with styling (displayed as label)
                        var valLabel = new Label
                        {
                            Text = value,
                            Location = new Point(290, y),
                            AutoSize = true,
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            ForeColor = WizardTheme.Colors.Primary,
                            BackColor = Color.FromArgb(240, 248, 255),  // Light blue background
                            Padding = new Padding(6, 3, 6, 3),
                            BorderStyle = BorderStyle.FixedSingle
                        };
                        scrollPanel.Controls.Add(valLabel);

                        // Store value in hidden textbox for saving
                        configTextBoxes[$"{component.ComponentId}:{key}"] =
                            new TextBox { Text = value, Visible = false };

                        y += 28;
                    }
                    else
                    {
                        // Editable textbox
                        var tb = new TextBox
                        {
                            Width = 460,
                            Location = new Point(290, y),
                            Font = new Font("Segoe UI", 9),
                            PlaceholderText = "Leave blank for default"
                        };
                        WizardTheme.StyleTextBox(tb);

                        tb.Text = component.ConfigurationValues.ContainsKey(key)
                         ? component.ConfigurationValues[key]
                           : value;

                        tb.TextChanged += (_, __) => UpdateFinishButtonState();

                        configTextBoxes[$"{component.ComponentId}:{key}"] = tb;
                        scrollPanel.Controls.Add(tb);

                        y += 28;

                        // Add description label below the textbox if available
                        if (FieldDescriptions.TryGetValue(displayName, out string? description))
                        {
                            var helpLabel = new Label
                            {
                                Text = $"ℹ️ {description}",
                                Location = new Point(290, y),
                                AutoSize = true,
                                MaximumSize = new Size(460, 0),
                                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                                ForeColor = Color.FromArgb(100, 100, 100),
                                BackColor = Color.Transparent,
                                Padding = new Padding(0, 2, 0, 4)
                            };
                            scrollPanel.Controls.Add(helpLabel);

                            // Calculate actual height needed and adjust y position
                            y += helpLabel.PreferredHeight + 8;
                        }
                        else
                        {
                            y += 8;
                        }
                    }
                }

                y += 20; // Extra spacing between components
            }

            // 🔑 CRITICAL: allow scrolling
            scrollPanel.AutoScrollMinSize = new Size(
   scrollPanel.ClientSize.Width,
      y + 40);

            scrollPanel.ResumeLayout();
            scrollPanel.PerformLayout();
            scrollPanel.Refresh();

            UpdateFinishButtonState();
        }

        // ===================== HELPERS =====================
        private Label AddLabel(
                   string text,
                   int y,
       bool bold,
                   Color? color = null,
              int x = 10,
              int fontSize = 10,
            bool italic = false)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font(
      "Segoe UI",
          fontSize,
           (bold ? FontStyle.Bold : FontStyle.Regular) |
                 (italic ? FontStyle.Italic : FontStyle.Regular)),
                ForeColor = color ?? WizardTheme.Colors.TextPrimary
            };
            scrollPanel.Controls.Add(lbl);
            return lbl;
        }

        private string GetFriendlyLabel(string key)
        {
            if (key.StartsWith("AppSettings."))
                key = key.Substring("AppSettings.".Length);

            if (key.Contains("]."))
                return key.Substring(key.IndexOf("].") + 2);

            return key;
        }

        private InstallerState? GetInstallerState()
        {
            return (FindForm() as WizardForm)?.State;
        }

        private void UpdateFinishButtonState()
        {
            var wizardForm = FindForm() as WizardForm;
            if (wizardForm == null)
                return;

            var nextBtn = wizardForm.Controls.Find("nextBtn", true).FirstOrDefault() as Button;
            if (nextBtn != null)
            {
                nextBtn.Enabled = true;
                nextBtn.BackColor = WizardTheme.Colors.Primary;
            }
        }

        // ===================== SAVE =====================
        public bool ValidateAndSaveConfiguration()
        {
            var state = GetInstallerState();
            if (state == null)
                return false;

            foreach (var kvp in configTextBoxes)
            {
                var parts = kvp.Key.Split(new[] { ':' }, 2);
                string componentId = parts[0];
                string key = parts[1];

                var component = state.SelectedComponents
                      .FirstOrDefault(c => c.ComponentId == componentId);

                if (component != null)
                    component.ConfigurationValues[key] = kvp.Value.Text.Trim();
            }

            return true;
        }
    }
}
