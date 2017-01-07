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
      this.taoSuiteAction_1 = new System.Windows.Forms.Label();
      this.taoSuiteAction_2 = new System.Windows.Forms.Label();
      this.taoSuiteAction_3 = new System.Windows.Forms.Label();
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
      this.progressBar.Location = new System.Drawing.Point(13, 104);
      this.progressBar.Maximum = 1000;
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(495, 23);
      this.progressBar.Step = 1;
      this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.progressBar.TabIndex = 1;
      this.progressBar.UseWaitCursor = true;
      // 
      // taoSuiteAction_1
      // 
      this.taoSuiteAction_1.AutoSize = true;
      this.taoSuiteAction_1.Location = new System.Drawing.Point(13, 40);
      this.taoSuiteAction_1.Name = "taoSuiteAction_1";
      this.taoSuiteAction_1.Size = new System.Drawing.Size(133, 13);
      this.taoSuiteAction_1.TabIndex = 2;
      this.taoSuiteAction_1.Text = "1 - some Tao Suite actions";
      this.taoSuiteAction_1.UseWaitCursor = true;
      // 
      // taoSuiteAction_2
      // 
      this.taoSuiteAction_2.AutoSize = true;
      this.taoSuiteAction_2.Location = new System.Drawing.Point(13, 57);
      this.taoSuiteAction_2.Name = "taoSuiteAction_2";
      this.taoSuiteAction_2.Size = new System.Drawing.Size(133, 13);
      this.taoSuiteAction_2.TabIndex = 3;
      this.taoSuiteAction_2.Text = "2 - some Tao Suite actions";
      // 
      // taoSuiteAction_3
      // 
      this.taoSuiteAction_3.AutoSize = true;
      this.taoSuiteAction_3.Location = new System.Drawing.Point(13, 74);
      this.taoSuiteAction_3.Name = "taoSuiteAction_3";
      this.taoSuiteAction_3.Size = new System.Drawing.Size(133, 13);
      this.taoSuiteAction_3.TabIndex = 4;
      this.taoSuiteAction_3.Text = "3 - some Tao Suite actions";
      // 
      // taoProgressBar
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(520, 141);
      this.ControlBox = false;
      this.Controls.Add(this.taoSuiteAction_3);
      this.Controls.Add(this.taoSuiteAction_2);
      this.Controls.Add(this.taoSuiteAction_1);
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
    private System.Windows.Forms.Label taoSuiteAction_1;
    private System.Windows.Forms.Label taoSuiteAction_2;
    private System.Windows.Forms.Label taoSuiteAction_3;
  }
}