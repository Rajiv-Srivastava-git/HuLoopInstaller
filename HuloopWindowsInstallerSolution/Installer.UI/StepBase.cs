using System.Drawing;
using System.Windows.Forms;

namespace Installer.UI
{
    public class StepBase : UserControl
    {
        public Label Title = new();
        public Label Description = new();
        private Panel contentPanel = new();

        public StepBase()
        {
            BackColor = WizardTheme.Colors.Background;
            Padding = new Padding(0);

            // Title
            Title.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            Title.AutoSize = true;
            Title.ForeColor = WizardTheme.Colors.TextPrimary;
            Title.Location = new Point(40, 30);

            // Description
            Description.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            Description.AutoSize = true;
            Description.ForeColor = WizardTheme.Colors.TextSecondary;
            Description.MaximumSize = new Size(750, 0);
            Description.Location = new Point(40, 80);

            // Content Panel - for step-specific content
            contentPanel.Location = new Point(0, 120);
            contentPanel.Size = new Size(this.Width, this.Height - 120);
            contentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contentPanel.BackColor = Color.Transparent;

            Controls.Add(Title);
            Controls.Add(Description);
            Controls.Add(contentPanel);
        }

        /// <summary>
        /// Gets the content panel where step-specific controls should be added
        /// </summary>
        protected Panel ContentPanel => contentPanel;

        /// <summary>
        /// Override to add custom step content easily
        /// </summary>
        protected virtual void InitializeStepContent()
        {
            // Override in derived classes to add content to ContentPanel
        }
    }
}
