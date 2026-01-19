using System;
using System.Windows.Forms;
using Installer.Core;

namespace Installer.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            //var state = new InstallerState();
            Application.Run(new WizardForm());
        }
    }
}
