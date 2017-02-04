namespace taoGUI
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.projectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.taoSuiteReportsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.summaryOfDoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.velocityOfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.taoApplicationStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.weatherReportsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.summaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.forecastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.teamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.analyzeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.taoProjectView = new System.Windows.Forms.TreeView();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.menuStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.projectToolStripMenuItem,
            this.buildToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.teamToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.analyzeToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(809, 24);
      this.menuStrip1.TabIndex = 0;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator1,
            this.closeToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "&File";
      // 
      // newToolStripMenuItem
      // 
      this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem1});
      this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
      this.newToolStripMenuItem.Name = "newToolStripMenuItem";
      this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
      this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.newToolStripMenuItem.Text = "&New";
      this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
      // 
      // projectToolStripMenuItem1
      // 
      this.projectToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("projectToolStripMenuItem1.Image")));
      this.projectToolStripMenuItem1.Name = "projectToolStripMenuItem1";
      this.projectToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
      this.projectToolStripMenuItem1.Size = new System.Drawing.Size(241, 22);
      this.projectToolStripMenuItem1.Text = "&Tao Application...";
      this.projectToolStripMenuItem1.Click += new System.EventHandler(this.projectToolStripMenuItem1_Click);
      // 
      // openToolStripMenuItem
      // 
      this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
      this.openToolStripMenuItem.Name = "openToolStripMenuItem";
      this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.openToolStripMenuItem.Text = "&Open";
      this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(143, 6);
      // 
      // closeToolStripMenuItem
      // 
      this.closeToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Cancel;
      this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
      this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.C)));
      this.closeToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.closeToolStripMenuItem.Text = "&Close";
      this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 6);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("exitToolStripMenuItem.Image")));
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.exitToolStripMenuItem.Text = "E&xit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
      // 
      // editToolStripMenuItem
      // 
      this.editToolStripMenuItem.Name = "editToolStripMenuItem";
      this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
      this.editToolStripMenuItem.Text = "&Edit";
      // 
      // viewToolStripMenuItem
      // 
      this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.taoSuiteReportsToolStripMenuItem,
            this.summaryOfDoneToolStripMenuItem,
            this.velocityOfToolStripMenuItem,
            this.taoApplicationStatisticsToolStripMenuItem,
            this.weatherReportsToolStripMenuItem});
      this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
      this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.viewToolStripMenuItem.Text = "&View";
      // 
      // taoSuiteReportsToolStripMenuItem
      // 
      this.taoSuiteReportsToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Stats2;
      this.taoSuiteReportsToolStripMenuItem.Name = "taoSuiteReportsToolStripMenuItem";
      this.taoSuiteReportsToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.taoSuiteReportsToolStripMenuItem.Text = "Tao Suite &Reports";
      this.taoSuiteReportsToolStripMenuItem.Click += new System.EventHandler(this.taoSuiteReportsToolStripMenuItem_Click);
      // 
      // summaryOfDoneToolStripMenuItem
      // 
      this.summaryOfDoneToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Stats;
      this.summaryOfDoneToolStripMenuItem.Name = "summaryOfDoneToolStripMenuItem";
      this.summaryOfDoneToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.summaryOfDoneToolStripMenuItem.Text = "Summary of &Done";
      this.summaryOfDoneToolStripMenuItem.Click += new System.EventHandler(this.summaryOfDoneToolStripMenuItem_Click);
      // 
      // velocityOfToolStripMenuItem
      // 
      this.velocityOfToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Dots_Up;
      this.velocityOfToolStripMenuItem.Name = "velocityOfToolStripMenuItem";
      this.velocityOfToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.velocityOfToolStripMenuItem.Text = "&Velocity of Alignment";
      this.velocityOfToolStripMenuItem.Click += new System.EventHandler(this.velocityOfToolStripMenuItem_Click);
      // 
      // taoApplicationStatisticsToolStripMenuItem
      // 
      this.taoApplicationStatisticsToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Percent;
      this.taoApplicationStatisticsToolStripMenuItem.Name = "taoApplicationStatisticsToolStripMenuItem";
      this.taoApplicationStatisticsToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.taoApplicationStatisticsToolStripMenuItem.Text = "Tao Application &Stability";
      this.taoApplicationStatisticsToolStripMenuItem.Click += new System.EventHandler(this.taoApplicationStatisticsToolStripMenuItem_Click);
      // 
      // weatherReportsToolStripMenuItem
      // 
      this.weatherReportsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.summaryToolStripMenuItem,
            this.forecastToolStripMenuItem});
      this.weatherReportsToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Weather_Cloud;
      this.weatherReportsToolStripMenuItem.Name = "weatherReportsToolStripMenuItem";
      this.weatherReportsToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
      this.weatherReportsToolStripMenuItem.Text = "&Weather Report";
      // 
      // summaryToolStripMenuItem
      // 
      this.summaryToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Weather_Rain;
      this.summaryToolStripMenuItem.Name = "summaryToolStripMenuItem";
      this.summaryToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
      this.summaryToolStripMenuItem.Text = "C&urrent";
      this.summaryToolStripMenuItem.Click += new System.EventHandler(this.summaryToolStripMenuItem_Click);
      // 
      // forecastToolStripMenuItem
      // 
      this.forecastToolStripMenuItem.Image = global::taoGUI.Properties.Resources.Weather_Sun;
      this.forecastToolStripMenuItem.Name = "forecastToolStripMenuItem";
      this.forecastToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
      this.forecastToolStripMenuItem.Text = "&Forecast";
      this.forecastToolStripMenuItem.Click += new System.EventHandler(this.forecastToolStripMenuItem_Click);
      // 
      // projectToolStripMenuItem
      // 
      this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
      this.projectToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
      this.projectToolStripMenuItem.Text = "&Project";
      // 
      // buildToolStripMenuItem
      // 
      this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
      this.buildToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
      this.buildToolStripMenuItem.Text = "&Build";
      // 
      // debugToolStripMenuItem
      // 
      this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
      this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
      this.debugToolStripMenuItem.Text = "&Debug";
      // 
      // teamToolStripMenuItem
      // 
      this.teamToolStripMenuItem.Name = "teamToolStripMenuItem";
      this.teamToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
      this.teamToolStripMenuItem.Text = "&Team";
      // 
      // toolsToolStripMenuItem
      // 
      this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
      this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
      this.toolsToolStripMenuItem.Text = "T&ools";
      // 
      // analyzeToolStripMenuItem
      // 
      this.analyzeToolStripMenuItem.Name = "analyzeToolStripMenuItem";
      this.analyzeToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
      this.analyzeToolStripMenuItem.Text = "&Analyze";
      // 
      // windowToolStripMenuItem
      // 
      this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
      this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
      this.windowToolStripMenuItem.Text = "&Window";
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.helpToolStripMenuItem.Text = "&Help";
      this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.FileName = "openFileDialog1";
      this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
      // 
      // taoProjectView
      // 
      this.taoProjectView.AllowDrop = true;
      this.taoProjectView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.taoProjectView.FullRowSelect = true;
      this.taoProjectView.HideSelection = false;
      this.taoProjectView.Location = new System.Drawing.Point(0, 24);
      this.taoProjectView.Name = "taoProjectView";
      this.taoProjectView.ShowNodeToolTips = true;
      this.taoProjectView.Size = new System.Drawing.Size(269, 361);
      this.taoProjectView.TabIndex = 1;
      this.taoProjectView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.launchAppStatusTabPage_DoubleClick);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 24, 3, 3);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.taoProjectView);
      this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(0, 24, 0, 0);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(0, 24, 0, 0);
      this.splitContainer1.Size = new System.Drawing.Size(809, 385);
      this.splitContainer1.SplitterDistance = 269;
      this.splitContainer1.TabIndex = 2;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(809, 385);
      this.Controls.Add(this.menuStrip1);
      this.Controls.Add(this.splitContainer1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "Form1";
      this.Text = "Tao Commander";
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.splitContainer1.Panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analyzeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem teamToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TreeView taoProjectView;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem taoSuiteReportsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem summaryOfDoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem velocityOfToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem taoApplicationStatisticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem weatherReportsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem summaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forecastToolStripMenuItem;
  }
}

