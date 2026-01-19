namespace Installer.UI
{
    partial class WizardForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel headerPanel;
        private Panel footerPanel;
        private Panel bodyPanel;
        private Button nextBtn;
        private Button prevBtn;
        private Button cancelBtn;
        private Label headerTitle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardForm));
            headerPanel = new Panel();
            headerTitle = new Label();
            footerPanel = new Panel();
            prevBtn = new Button();
            nextBtn = new Button();
            cancelBtn = new Button();
            bodyPanel = new Panel();
            headerPanel.SuspendLayout();
            footerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.LightGreen;
            headerPanel.Controls.Add(headerTitle);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(882, 60);
            headerPanel.TabIndex = 2;
            // 
            // headerTitle
            // 
            headerTitle.AutoSize = true;
            headerTitle.Font = new Font("Segoe UI Semibold", 18F);
            headerTitle.Location = new Point(12, 9);
            headerTitle.Name = "headerTitle";
            headerTitle.Size = new Size(245, 41);
            headerTitle.TabIndex = 0;
            headerTitle.Text = "HuLoop Installer";
            // 
            // footerPanel
            // 
            footerPanel.Controls.Add(prevBtn);
            footerPanel.Controls.Add(nextBtn);
            footerPanel.Controls.Add(cancelBtn);
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Location = new Point(0, 493);
            footerPanel.Name = "footerPanel";
            footerPanel.Size = new Size(882, 60);
            footerPanel.TabIndex = 1;
            // 
            // prevBtn
            // 
            prevBtn.Anchor = AnchorStyles.Right;
            prevBtn.Location = new Point(20, 15);
            prevBtn.Name = "prevBtn";
            prevBtn.Size = new Size(90, 32);
            prevBtn.TabIndex = 0;
            prevBtn.Text = "Back";
            prevBtn.Click += prevBtn_Click;
            // 
            // nextBtn
            // 
            nextBtn.Anchor = AnchorStyles.Right;
            nextBtn.Location = new Point(680, 15);
            nextBtn.Name = "nextBtn";
            nextBtn.Size = new Size(90, 32);
            nextBtn.TabIndex = 1;
            nextBtn.Text = "Next";
            nextBtn.BackColor = Color.LightGreen;
            nextBtn.Click += nextBtn_Click;
            // 
            // cancelBtn
            // 
            cancelBtn.Anchor = AnchorStyles.Right;
            cancelBtn.Location = new Point(800, 15);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new Size(90, 32);
            cancelBtn.TabIndex = 2;
            cancelBtn.Text = "Cancel";
            // 
            // bodyPanel
            // 
            bodyPanel.Dock = DockStyle.Fill;
            bodyPanel.Location = new Point(0, 60);
            bodyPanel.Name = "bodyPanel";
            bodyPanel.Size = new Size(882, 433);
            bodyPanel.TabIndex = 0;
            // 
            // WizardForm
            // 
            ClientSize = new Size(882, 553);
            Controls.Add(bodyPanel);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(900, 600);
            Name = "WizardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HuLoop Installation Wizard";
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            footerPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}