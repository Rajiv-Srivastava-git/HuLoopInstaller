using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer.UI
{
    public static class WizardTheme
    {
        public static void ApplyTheme(WizardForm form)
        {
            form.BackColor = Color.White;

            // Header
            form.Controls["headerPanel"].BackColor = Color.LightGreen;

            // Footer
            Panel footer = (Panel)form.Controls["footerPanel"];
            footer.BackColor = Color.White;
            footer.Padding = new Padding(10, 8, 10, 8);
            footer.BorderStyle = BorderStyle.FixedSingle;

            StylePrimary(footer.Controls["nextBtn"] as Button);
            StyleSecondary(footer.Controls["prevBtn"] as Button);
            StyleCancel(footer.Controls["cancelBtn"] as Button);
        }

        private static void StylePrimary(Button btn)
        {
            btn.BackColor = Color.FromArgb(0, 122, 204);
            btn.ForeColor = Color.Black; 
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI Semibold", 10);
        }

        private static void StyleSecondary(Button btn)
        {
            btn.BackColor = Color.White;
            btn.ForeColor = Color.Black;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = new Font("Segoe UI", 10);
        }

        private static void StyleCancel(Button btn)
        {
            btn.BackColor = Color.White;
            btn.ForeColor = Color.Black;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = new Font("Segoe UI", 10);
        }
    }
}
