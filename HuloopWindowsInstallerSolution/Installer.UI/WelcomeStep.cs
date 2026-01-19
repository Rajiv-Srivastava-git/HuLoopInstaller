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
    public partial class WelcomeStep : StepBase
    {
        public WelcomeStep()
        {
            Title.Text = "Welcome to HuLoop!";
            Description.Text = "Thank you for choosing HuLoop automation tools.\n\nThis installer allows you to pick the components you want—Desktop Driver, Windows Agent, CLI, Recorder, Scheduler, and more.\n\nWe’ll download, install, and configure everything for you.\n\nClick Next to get started!" ;
        }
    }
}
