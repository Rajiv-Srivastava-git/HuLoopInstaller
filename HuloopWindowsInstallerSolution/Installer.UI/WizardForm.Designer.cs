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
            headerPanel.BackColor = Color.FromArgb(0, 120, 212);
            headerPanel.Controls.Add(headerTitle);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(4);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1125, 100);
            headerPanel.TabIndex = 2;
            headerPanel.Visible = false;
            // 
            // headerTitle
            // 
            headerTitle.AutoSize = true;
            headerTitle.BackColor = Color.Transparent;
            headerTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            headerTitle.ForeColor = Color.White;
            headerTitle.Location = new Point(38, 25);
            headerTitle.Margin = new Padding(4, 0, 4, 0);
            headerTitle.Name = "headerTitle";
            headerTitle.Size = new Size(335, 54);
            headerTitle.TabIndex = 0;
            headerTitle.Text = "HuLoop Installer";
            // 
            // footerPanel
            // 
            footerPanel.BackColor = Color.White;
            footerPanel.Controls.Add(prevBtn);
            footerPanel.Controls.Add(nextBtn);
            footerPanel.Controls.Add(cancelBtn);
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Location = new Point(0, 662);
            footerPanel.Margin = new Padding(4);
            footerPanel.Name = "footerPanel";
            footerPanel.Padding = new Padding(25, 15, 25, 15);
            footerPanel.Size = new Size(1125, 88);
            footerPanel.TabIndex = 1;
            // 
            // prevBtn
            // 
            prevBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            prevBtn.Location = new Point(25, 21);
            prevBtn.Margin = new Padding(4);
            prevBtn.Name = "prevBtn";
            prevBtn.Size = new Size(138, 45);
            prevBtn.TabIndex = 0;
            prevBtn.Text = "◄ Back";
            prevBtn.UseVisualStyleBackColor = false;
            prevBtn.Click += prevBtn_Click;
            // 
            // nextBtn
            // 
            nextBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            nextBtn.Location = new Point(810, 21);
            nextBtn.Margin = new Padding(4);
            nextBtn.Name = "nextBtn";
            nextBtn.Size = new Size(138, 45);
            nextBtn.TabIndex = 1;
            nextBtn.Text = "Next ►";
            nextBtn.UseVisualStyleBackColor = false;
            nextBtn.Click += nextBtn_Click;
            // 
            // cancelBtn
            // 
            cancelBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelBtn.DialogResult = DialogResult.Cancel;
            cancelBtn.Location = new Point(990, 21);
            cancelBtn.Margin = new Padding(4);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new Size(138, 45);
            cancelBtn.TabIndex = 2;
            cancelBtn.Text = "Cancel";
            cancelBtn.UseVisualStyleBackColor = false;
            // 
            // bodyPanel
            // 
            bodyPanel.BackColor = Color.FromArgb(243, 243, 243);
            bodyPanel.Dock = DockStyle.Fill;
            bodyPanel.Location = new Point(0, 100);
            bodyPanel.Margin = new Padding(4);
            bodyPanel.Name = "bodyPanel";
            bodyPanel.Size = new Size(1125, 562);
            bodyPanel.TabIndex = 0;
            // 
            // WizardForm
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1125, 750);
            Controls.Add(bodyPanel);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            MinimizeBox = false;
            MinimumSize = new Size(1120, 738);
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