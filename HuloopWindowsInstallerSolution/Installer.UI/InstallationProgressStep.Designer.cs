namespace Installer.UI
{
    partial class InstallationProgressStep
    {
        /// <summary> 
   /// Required designer variable.
   /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
      {
          if (disposing && (components != null))
       {
       components.Dispose();
   }
      base.Dispose(disposing);
 }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
   this.progressBar = new System.Windows.Forms.ProgressBar();
  this.lblCurrentOperation = new System.Windows.Forms.Label();
    this.lblPhase = new System.Windows.Forms.Label();
            this.lblComponentName = new System.Windows.Forms.Label();
        this.lblStatus = new System.Windows.Forms.Label();
     this.pnlProgress = new System.Windows.Forms.Panel();
        this.pnlProgress.SuspendLayout();
    this.SuspendLayout();
       // 
            // progressBar
         // 
            this.progressBar.Location = new System.Drawing.Point(20, 200);
   this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(600, 30);
         this.progressBar.TabIndex = 0;
            // 
     // lblCurrentOperation
       // 
   this.lblCurrentOperation.AutoSize = true;
       this.lblCurrentOperation.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
     this.lblCurrentOperation.Location = new System.Drawing.Point(20, 170);
         this.lblCurrentOperation.Name = "lblCurrentOperation";
            this.lblCurrentOperation.Size = new System.Drawing.Size(120, 19);
    this.lblCurrentOperation.TabIndex = 1;
    this.lblCurrentOperation.Text = "Initializing...";
            // 
            // lblPhase
            // 
     this.lblPhase.AutoSize = true;
   this.lblPhase.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
     this.lblPhase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
        this.lblPhase.Location = new System.Drawing.Point(20, 120);
   this.lblPhase.Name = "lblPhase";
        this.lblPhase.Size = new System.Drawing.Size(150, 25);
            this.lblPhase.TabIndex = 2;
 this.lblPhase.Text = "Downloading...";
   // 
            // lblComponentName
            // 
            this.lblComponentName.AutoSize = true;
       this.lblComponentName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
          this.lblComponentName.Location = new System.Drawing.Point(20, 145);
        this.lblComponentName.Name = "lblComponentName";
      this.lblComponentName.Size = new System.Drawing.Size(0, 21);
  this.lblComponentName.TabIndex = 3;
         // 
         // lblStatus
            // 
 this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
    this.lblStatus.ForeColor = System.Drawing.Color.Gray;
          this.lblStatus.Location = new System.Drawing.Point(20, 240);
      this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(600, 150);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "";
// 
         // pnlProgress
            // 
  this.pnlProgress.Controls.Add(this.lblPhase);
            this.pnlProgress.Controls.Add(this.lblStatus);
   this.pnlProgress.Controls.Add(this.lblComponentName);
      this.pnlProgress.Controls.Add(this.lblCurrentOperation);
            this.pnlProgress.Controls.Add(this.progressBar);
  this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProgress.Location = new System.Drawing.Point(0, 0);
this.pnlProgress.Name = "pnlProgress";
            this.pnlProgress.Size = new System.Drawing.Size(650, 450);
            this.pnlProgress.TabIndex = 5;
       // 
       // InstallationProgressStep
            // 
   this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlProgress);
       this.Name = "InstallationProgressStep";
  this.Size = new System.Drawing.Size(650, 450);
  this.pnlProgress.ResumeLayout(false);
     this.pnlProgress.PerformLayout();
            this.ResumeLayout(false);

    }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblCurrentOperation;
        private System.Windows.Forms.Label lblPhase;
        private System.Windows.Forms.Label lblComponentName;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel pnlProgress;
    }
}
