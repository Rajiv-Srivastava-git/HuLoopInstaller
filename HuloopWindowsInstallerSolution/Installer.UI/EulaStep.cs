using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Installer.UI
{
    public partial class EulaStep : StepBase
    {
        private CheckBox chkAccept = null!;
        private RichTextBox rtbLicense = null!;
        private Panel licensePanel = null!;

        public bool IsAccepted => chkAccept?.Checked ?? false;

        public EulaStep()
        {
            InitializeComponent();

            // Step header text
            Title.Text = "End-User License Agreement";
            Description.Text = "Please read and accept the license agreement to continue with the installation.";

            // Always work inside the wizard content panel
            var host = this.ContentPanel;
            host.Controls.Clear();

            // Root layout
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(40, 20, 40, 20)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // license
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // checkbox

            // ===== License panel =====
            licensePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = WizardTheme.Colors.Surface,
                Padding = new Padding(1)
            };

            // Subtle border
            licensePanel.Paint += (s, e) =>
            {
                using var pen = new Pen(WizardTheme.Colors.BorderLight);
                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    licensePanel.Width - 1,
                    licensePanel.Height - 1
                );
            };

            // License text box
            rtbLicense = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = WizardTheme.Colors.Surface,
                ForeColor = WizardTheme.Colors.TextPrimary
            };

            LoadLicenseText();

            licensePanel.Controls.Add(rtbLicense);

            // ===== Acceptance checkbox =====
            chkAccept = new CheckBox
            {
                Text = "I have read and accept the terms and conditions",
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = WizardTheme.Colors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 16, 0, 0),
                Anchor = AnchorStyles.Left
            };

            chkAccept.CheckedChanged += OnAcceptanceChanged;

            // Add to layout
            layout.Controls.Add(licensePanel, 0, 0);
            layout.Controls.Add(chkAccept, 0, 1);

            host.Controls.Add(layout);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                UpdateNextButtonState();
        }

        private void OnAcceptanceChanged(object? sender, EventArgs e)
        {
            UpdateNextButtonState();
        }

        private void UpdateNextButtonState()
        {
            var wizardForm = FindForm() as WizardForm;
            if (wizardForm == null)
                return;

            var nextBtn = wizardForm.Controls
                .Find("nextBtn", true)
                .FirstOrDefault() as Button;

            if (nextBtn != null)
            {
                nextBtn.Enabled = chkAccept.Checked;
                nextBtn.BackColor = chkAccept.Checked
                    ? WizardTheme.Colors.Primary
                    : WizardTheme.Colors.BorderMedium;
            }
        }

        private void LoadLicenseText()
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory;

                var rtfPath = Path.Combine(baseDirectory, "LICENSE.rtf");
                if (File.Exists(rtfPath))
                {
                    rtbLicense.LoadFile(rtfPath, RichTextBoxStreamType.RichText);
                    return;
                }

                var txtPath = Path.Combine(baseDirectory, "LICENSE.txt");
                if (File.Exists(txtPath))
                {
                    rtbLicense.Text = File.ReadAllText(txtPath);
                    return;
                }

                rtbLicense.Text = GetDefaultLicenseText();
            }
            catch
            {
                rtbLicense.Text = GetDefaultLicenseText();
            }
        }

        private string GetDefaultLicenseText()
        {
            return @"END-USER LICENSE AGREEMENT

IMPORTANT - READ CAREFULLY:

This End-User License Agreement (""EULA"") is a legal agreement between you and HuLoop for the HuLoop software product.

By installing, copying, or otherwise using the software, you agree to be bound by the terms of this EULA.

1. GRANT OF LICENSE
You may install and use the software.

2. LIMITATIONS
You may not reverse engineer, decompile, or disassemble the software.

3. COPYRIGHT
All rights are owned by HuLoop.

4. LIMITED WARRANTY
The software is provided ""as is"" without warranty of any kind.

5. LIMITATION OF LIABILITY
HuLoop shall not be liable for any damages arising from use of this software.

By accepting below, you acknowledge that you have read and agree to this license.";
        }
    }
}
