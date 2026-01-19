using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer.UI
{
    public partial class EulaStep : StepBase
    {
        public EulaStep()
        {
            Title.Text = "End-User License Agreement";
            Description.Text = "Please read the license agreement to proceed.";

            var textbox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 600,
                Height = 300,
                Text = "LICENSE AGREEMENT...",
                Location = new Point(10, 100)
            };

            Controls.Add(textbox);
        }
    }
}
