using System.Drawing;
using System.Windows.Forms;

namespace Installer.UI
{
    public class StepBase : UserControl
    {
        public Label Title = new();
        public Label Description = new();

        public StepBase()
        {
            BackColor = Color.White;
            Padding = new Padding(60, 40, 60, 40);

            Title.Font = new Font("Segoe UI Semibold", 20);
            Title.AutoSize = true;
            Title.ForeColor = Color.FromArgb(51, 51, 51);
            Title.Location = new Point(10, 0);

            Description.Font = new Font("Segoe UI", 10);
            Description.AutoSize = true;
            Description.ForeColor = Color.FromArgb(85, 85, 85);
            Description.MaximumSize = new Size(600, 0);
            Description.Location = new Point(10, 60);

            Controls.Add(Title);
            Controls.Add(Description);

        }
    }
}
