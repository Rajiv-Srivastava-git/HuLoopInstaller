using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer.UI
{
    public static class WizardTheme
    {
        // Modern Color Palette
        public static class Colors
        {
            // Primary brand colors
            public static readonly Color Primary = Color.FromArgb(0, 120, 212);        // Modern blue
            public static readonly Color PrimaryHover = Color.FromArgb(0, 100, 180);   // Darker blue on hover
            public static readonly Color PrimaryPressed = Color.FromArgb(0, 80, 150);  // Even darker when pressed

            // Accent colors
            public static readonly Color Accent = Color.FromArgb(16, 137, 255);        // Bright blue accent
            public static readonly Color Success = Color.FromArgb(16, 124, 16);        // Green for success
            public static readonly Color Warning = Color.FromArgb(255, 185, 0);   // Amber for warnings
            public static readonly Color Error = Color.FromArgb(232, 17, 35);          // Red for errors

            // Neutral colors
            public static readonly Color White = Color.White;
            public static readonly Color Background = Color.FromArgb(243, 243, 243);   // Light gray background
            public static readonly Color Surface = Color.White;
            public static readonly Color SurfaceSecondary = Color.FromArgb(250, 250, 250);

            // Text colors
            public static readonly Color TextPrimary = Color.FromArgb(32, 32, 32);     // Dark gray, almost black
            public static readonly Color TextSecondary = Color.FromArgb(96, 96, 96);   // Medium gray
            public static readonly Color TextTertiary = Color.FromArgb(160, 160, 160); // Light gray
            public static readonly Color TextOnPrimary = Color.White; // White text on primary

            // Border colors
            public static readonly Color BorderLight = Color.FromArgb(225, 225, 225);  // Light border
            public static readonly Color BorderMedium = Color.FromArgb(200, 200, 200); // Medium border
            public static readonly Color BorderDark = Color.FromArgb(160, 160, 160);   // Dark border

            // Header gradient
            public static readonly Color HeaderStart = Color.FromArgb(0, 120, 212);
            public static readonly Color HeaderEnd = Color.FromArgb(0, 95, 184);
        }

        public static void ApplyTheme(WizardForm form)
        {
            // Form styling
            form.BackColor = Colors.Background;
            form.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Header Panel with gradient
            Panel headerPanel = (Panel)form.Controls["headerPanel"];
            headerPanel.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                        headerPanel.ClientRectangle,
                Colors.HeaderStart,
              Colors.HeaderEnd,
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
                }
            };
            headerPanel.Height = 80;

            // Header Title
            Label headerTitle = headerPanel.Controls.OfType<Label>().FirstOrDefault();
            if (headerTitle != null)
            {
                headerTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
                headerTitle.ForeColor = Colors.White;
                headerTitle.Location = new Point(30, 20);
                headerTitle.BackColor = Color.Transparent;
            }

            // Footer Panel with shadow effect
            Panel footerPanel = (Panel)form.Controls["footerPanel"];
            footerPanel.BackColor = Colors.Surface;
            footerPanel.Padding = new Padding(20, 12, 20, 12);
            footerPanel.Height = 70;

            // Add top border to footer
            footerPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Colors.BorderLight, 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, footerPanel.Width, 0);
                }
            };

            // Body Panel
            Panel bodyPanel = (Panel)form.Controls["bodyPanel"];
            bodyPanel.BackColor = Colors.Background;
            bodyPanel.Padding = new Padding(0);

            // Style Buttons
            StylePrimaryButton(footerPanel.Controls["nextBtn"] as Button);
            StyleSecondaryButton(footerPanel.Controls["prevBtn"] as Button);
            StyleSecondaryButton(footerPanel.Controls["cancelBtn"] as Button);
        }

        private static void StylePrimaryButton(Button btn)
        {
            if (btn == null) return;

            btn.BackColor = Colors.Primary;
            btn.ForeColor = Colors.TextOnPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            btn.Size = new Size(110, 36);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Colors.PrimaryHover;
            btn.FlatAppearance.MouseDownBackColor = Colors.PrimaryPressed;

            // Add rounded corners effect
            btn.Paint += (s, e) =>
            {
                Button button = s as Button;
                if (button == null) return;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath path = GetRoundedRectangle(button.ClientRectangle, 4))
                {
                    button.Region = new Region(path);
                }
            };

            // Hover effect
            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Colors.PrimaryHover;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn.Enabled)
                    btn.BackColor = Colors.Primary;
            };
        }

        private static void StyleSecondaryButton(Button btn)
        {
            if (btn == null) return;

            btn.BackColor = Colors.Surface;
            btn.ForeColor = Colors.TextPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Colors.BorderMedium;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            btn.Size = new Size(110, 36);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Colors.SurfaceSecondary;

            // Add rounded corners effect
            btn.Paint += (s, e) =>
            {
                Button button = s as Button;
                if (button == null) return;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath path = GetRoundedRectangle(button.ClientRectangle, 4))
                {
                    button.Region = new Region(path);
                }
            };

            // Hover effect
            btn.MouseEnter += (s, e) =>
            {
                btn.FlatAppearance.BorderColor = Colors.Primary;
                btn.ForeColor = Colors.Primary;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.FlatAppearance.BorderColor = Colors.BorderMedium;
                btn.ForeColor = Colors.TextPrimary;
            };
        }

        public static void StyleProgressBar(ProgressBar progressBar)
        {
            if (progressBar == null) return;

            progressBar.Height = 8;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.ForeColor = Colors.Primary;
        }

        public static void StyleCheckBox(CheckBox checkBox)
        {
            if (checkBox == null) return;

            checkBox.ForeColor = Colors.TextPrimary;
            checkBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            checkBox.FlatStyle = FlatStyle.Flat;
            checkBox.Cursor = Cursors.Hand;
        }

        public static void StyleTextBox(TextBox textBox)
        {
            if (textBox == null) return;

            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            textBox.ForeColor = Colors.TextPrimary;
            textBox.BackColor = Colors.Surface;
        }

        public static void StyleRichTextBox(RichTextBox richTextBox)
        {
            if (richTextBox == null) return;

            richTextBox.BorderStyle = BorderStyle.FixedSingle;
            richTextBox.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            richTextBox.ForeColor = Colors.TextPrimary;
            richTextBox.BackColor = Colors.Surface;
        }

        public static void StylePanel(Panel panel, bool withBorder = true)
        {
            if (panel == null) return;

            panel.BackColor = Colors.Surface;

            if (withBorder)
            {
                panel.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(Colors.BorderLight, 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                    }
                };
            }
        }

        public static Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                AutoSize = true
            };
        }

        public static Label CreateDescriptionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                MaximumSize = new Size(700, 0)
            };
        }

        public static Label CreateInfoLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Colors.TextTertiary,
                AutoSize = true
            };
        }

        private static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
