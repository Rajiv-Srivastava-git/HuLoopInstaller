namespace Installer.UI
{
    partial class ComponentItemControl
    {
        private System.ComponentModel.IContainer components = null;

        private CheckBox chkSelect;
        private Label lblSize;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.chkSelect = new System.Windows.Forms.CheckBox();
            this.lblSize = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkSelect
            // 
            this.chkSelect.AutoSize = true;
            this.chkSelect.Location = new System.Drawing.Point(10, 10);
            this.chkSelect.Name = "chkSelect";
            this.chkSelect.Size = new System.Drawing.Size(15, 14);
            this.chkSelect.TabIndex = 0;
            this.chkSelect.CheckedChanged += new System.EventHandler(this.chkSelect_CheckedChanged);
            // 
            // lblSize
            // 
            this.lblSize.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSize.Location = new System.Drawing.Point(350, 8);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(80, 20);
            this.lblSize.TabIndex = 1;
            this.lblSize.Text = "0 MB";
            this.lblSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ComponentItemControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.chkSelect);
            this.Controls.Add(this.lblSize);
            this.Name = "ComponentItemControl";
            this.Size = new System.Drawing.Size(450, 35);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}