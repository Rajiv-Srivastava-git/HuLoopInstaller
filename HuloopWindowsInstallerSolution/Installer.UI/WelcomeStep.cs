using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer.UI
{
    public partial class WelcomeStep : StepBase
    {
        public WelcomeStep()
        {
            InitializeComponent();

            // Hide default step title/description
            Title.Visible = false;
            Description.Visible = false;

            // IMPORTANT:
            // Always add content to the wizard's content panel
            var host = this.ContentPanel;
            host.Controls.Clear();

            // Root layout (3 rows to bias content upward)
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));   // top spacer
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // content
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));   // bottom spacer

            // Center content panel
            Panel centerPanel = new Panel
            {
                Size = new Size(700, 220),
                Anchor = AnchorStyles.None,
                BackColor = Color.Transparent
            };

            // Welcome title
            Label welcomeLabel = new Label
            {
                Text = "Welcome to HuLoop",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = WizardTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle
            Label thankYouLabel = new Label
            {
                Text = "Thank you for choosing HuLoop Automation Platform.",
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                ForeColor = WizardTheme.Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Decorative line (centered)
            Panel lineHost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 20
            };

            Panel decorativeLine = new Panel
            {
                Size = new Size(200, 3),
                BackColor = WizardTheme.Colors.Primary
            };

            decorativeLine.Location = new Point(
                (centerPanel.Width - decorativeLine.Width) / 2,
                (lineHost.Height - decorativeLine.Height) / 2
            );

            lineHost.Controls.Add(decorativeLine);

            // Info text
            Label infoLabel = new Label
            {
                Text = "Click Next to begin the installation process",
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = WizardTheme.Colors.TextTertiary,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add controls (bottom-up because Dock = Top)
            centerPanel.Controls.Add(infoLabel);
            centerPanel.Controls.Add(lineHost);
            centerPanel.Controls.Add(thankYouLabel);
            centerPanel.Controls.Add(welcomeLabel);

            // Add center panel to middle row
            layout.Controls.Add(centerPanel, 0, 1);

            // Add layout to wizard content
            host.Controls.Add(layout);

            // Re-center decorative line on resize (DPI safe)
            centerPanel.Resize += (s, e) =>
            {
                decorativeLine.Left = (centerPanel.Width - decorativeLine.Width) / 2;
            };
        }
    }


}
