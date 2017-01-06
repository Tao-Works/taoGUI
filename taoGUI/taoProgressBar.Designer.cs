namespace taoGUI {
  partial class taoProgressBar {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.progressDescription = new System.Windows.Forms.Label();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.taoSuiteAction = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // progressDescription
      // 
      this.progressDescription.AutoSize = true;
      this.progressDescription.Location = new System.Drawing.Point(13, 13);
      this.progressDescription.Name = "progressDescription";
      this.progressDescription.Size = new System.Drawing.Size(142, 13);
      this.progressDescription.TabIndex = 0;
      this.progressDescription.Text = "Count out of total Tao Suites";
      this.progressDescription.UseWaitCursor = true;
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(16, 68);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(492, 23);
      this.progressBar.Step = 1;
      this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.progressBar.TabIndex = 1;
      this.progressBar.UseWaitCursor = true;
      // 
      // taoSuiteAction
      // 
      this.taoSuiteAction.AutoSize = true;
      this.taoSuiteAction.Location = new System.Drawing.Point(13, 40);
      this.taoSuiteAction.Name = "taoSuiteAction";
      this.taoSuiteAction.Size = new System.Drawing.Size(127, 13);
      this.taoSuiteAction.TabIndex = 2;
      this.taoSuiteAction.Text = "Current Tao Suite actions";
      this.taoSuiteAction.UseWaitCursor = true;
      // 
      // taoProgressBar
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(524, 108);
      this.ControlBox = false;
      this.Controls.Add(this.taoSuiteAction);
      this.Controls.Add(this.progressBar);
      this.Controls.Add(this.progressDescription);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "taoProgressBar";
      this.ShowIcon = false;
      this.Text = "Re-calculating Statistics";
      this.UseWaitCursor = true;
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label progressDescription;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Label taoSuiteAction;
  }
}