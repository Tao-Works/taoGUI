namespace taoGUI {
  partial class formGroupByDimensions {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formGroupByDimensions));
      this.menuGroupByDimensions = new System.Windows.Forms.MenuStrip();
      this.toolStripUserDimension = new System.Windows.Forms.ToolStripTextBox();
      this.menuButtonAddDimension = new System.Windows.Forms.ToolStripMenuItem();
      this.menuButtonRemoveDimension = new System.Windows.Forms.ToolStripMenuItem();
      this.menuButtonAddToTaoSuite = new System.Windows.Forms.ToolStripMenuItem();
      this.menuButtonRemoveFromTaoSuite = new System.Windows.Forms.ToolStripMenuItem();
      this.menuButtonShowTaoSuite = new System.Windows.Forms.ToolStripMenuItem();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.treeViewUserDimensions = new System.Windows.Forms.TreeView();
      this.dataGridViewUserGroups = new System.Windows.Forms.DataGridView();
      this.menuGroupByDimensions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserGroups)).BeginInit();
      this.SuspendLayout();
      // 
      // menuGroupByDimensions
      // 
      this.menuGroupByDimensions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripUserDimension,
            this.menuButtonAddDimension,
            this.menuButtonRemoveDimension,
            this.menuButtonAddToTaoSuite,
            this.menuButtonRemoveFromTaoSuite,
            this.menuButtonShowTaoSuite});
      this.menuGroupByDimensions.Location = new System.Drawing.Point(0, 0);
      this.menuGroupByDimensions.Name = "menuGroupByDimensions";
      this.menuGroupByDimensions.Size = new System.Drawing.Size(824, 27);
      this.menuGroupByDimensions.TabIndex = 0;
      this.menuGroupByDimensions.Text = "menuGroupByDimensions";
      // 
      // toolStripUserDimension
      // 
      this.toolStripUserDimension.AutoToolTip = true;
      this.toolStripUserDimension.Name = "toolStripUserDimension";
      this.toolStripUserDimension.Size = new System.Drawing.Size(100, 23);
      this.toolStripUserDimension.ToolTipText = "Name of new dimension";
      // 
      // menuButtonAddDimension
      // 
      this.menuButtonAddDimension.AutoToolTip = true;
      this.menuButtonAddDimension.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.menuButtonAddDimension.Image = global::taoGUI.Properties.Resources.Ok;
      this.menuButtonAddDimension.Name = "menuButtonAddDimension";
      this.menuButtonAddDimension.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.menuButtonAddDimension.Size = new System.Drawing.Size(24, 23);
      this.menuButtonAddDimension.ToolTipText = "Add dimension to group-by category";
      this.menuButtonAddDimension.Click += new System.EventHandler(this.menuButtonAddDimension_Click);
      // 
      // menuButtonRemoveDimension
      // 
      this.menuButtonRemoveDimension.AutoToolTip = true;
      this.menuButtonRemoveDimension.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.menuButtonRemoveDimension.Image = global::taoGUI.Properties.Resources.Cancel;
      this.menuButtonRemoveDimension.Name = "menuButtonRemoveDimension";
      this.menuButtonRemoveDimension.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.menuButtonRemoveDimension.Size = new System.Drawing.Size(24, 23);
      this.menuButtonRemoveDimension.ToolTipText = "Remove dimension from group-by category";
      this.menuButtonRemoveDimension.Click += new System.EventHandler(this.menuButtonRemoveDimension_Click);
      // 
      // menuButtonAddToTaoSuite
      // 
      this.menuButtonAddToTaoSuite.AutoToolTip = true;
      this.menuButtonAddToTaoSuite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.menuButtonAddToTaoSuite.Image = global::taoGUI.Properties.Resources.Plus;
      this.menuButtonAddToTaoSuite.Name = "menuButtonAddToTaoSuite";
      this.menuButtonAddToTaoSuite.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.menuButtonAddToTaoSuite.Size = new System.Drawing.Size(24, 23);
      this.menuButtonAddToTaoSuite.ToolTipText = "Add dimension to Tao Suite";
      this.menuButtonAddToTaoSuite.Click += new System.EventHandler(this.menuButtonAddToTaoSuite_Click);
      // 
      // menuButtonRemoveFromTaoSuite
      // 
      this.menuButtonRemoveFromTaoSuite.AutoToolTip = true;
      this.menuButtonRemoveFromTaoSuite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.menuButtonRemoveFromTaoSuite.Image = global::taoGUI.Properties.Resources.Minus;
      this.menuButtonRemoveFromTaoSuite.Name = "menuButtonRemoveFromTaoSuite";
      this.menuButtonRemoveFromTaoSuite.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.menuButtonRemoveFromTaoSuite.Size = new System.Drawing.Size(24, 23);
      this.menuButtonRemoveFromTaoSuite.ToolTipText = "Remove dimension from Tao Suite";
      this.menuButtonRemoveFromTaoSuite.Click += new System.EventHandler(this.menuButtonRemoveFromTaoSuite_Click);
      // 
      // menuButtonShowTaoSuite
      // 
      this.menuButtonShowTaoSuite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.menuButtonShowTaoSuite.Image = global::taoGUI.Properties.Resources.Table;
      this.menuButtonShowTaoSuite.Name = "menuButtonShowTaoSuite";
      this.menuButtonShowTaoSuite.Size = new System.Drawing.Size(28, 23);
      this.menuButtonShowTaoSuite.Click += new System.EventHandler(this.menuButtonShowTaoSuite_Click);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 27);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.treeViewUserDimensions);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.dataGridViewUserGroups);
      this.splitContainer1.Size = new System.Drawing.Size(824, 414);
      this.splitContainer1.SplitterDistance = 200;
      this.splitContainer1.TabIndex = 1;
      // 
      // treeViewUserDimensions
      // 
      this.treeViewUserDimensions.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeViewUserDimensions.Location = new System.Drawing.Point(0, 0);
      this.treeViewUserDimensions.Name = "treeViewUserDimensions";
      this.treeViewUserDimensions.Size = new System.Drawing.Size(200, 414);
      this.treeViewUserDimensions.TabIndex = 0;
      // 
      // dataGridViewUserGroups
      // 
      this.dataGridViewUserGroups.AllowUserToAddRows = false;
      this.dataGridViewUserGroups.AllowUserToDeleteRows = false;
      this.dataGridViewUserGroups.AllowUserToOrderColumns = true;
      this.dataGridViewUserGroups.BackgroundColor = System.Drawing.Color.LightGray;
      this.dataGridViewUserGroups.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.dataGridViewUserGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridViewUserGroups.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGridViewUserGroups.Location = new System.Drawing.Point(0, 0);
      this.dataGridViewUserGroups.Name = "dataGridViewUserGroups";
      this.dataGridViewUserGroups.ReadOnly = true;
      this.dataGridViewUserGroups.Size = new System.Drawing.Size(620, 414);
      this.dataGridViewUserGroups.StandardTab = true;
      this.dataGridViewUserGroups.TabIndex = 0;
      // 
      // formGroupByDimensions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(824, 441);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.menuGroupByDimensions);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.menuGroupByDimensions;
      this.Name = "formGroupByDimensions";
      this.Text = "Group-By Dimensions";
      this.menuGroupByDimensions.ResumeLayout(false);
      this.menuGroupByDimensions.PerformLayout();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUserGroups)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip menuGroupByDimensions;
    private System.Windows.Forms.ToolStripTextBox toolStripUserDimension;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.TreeView treeViewUserDimensions;
    private System.Windows.Forms.DataGridView dataGridViewUserGroups;
    private System.Windows.Forms.ToolStripMenuItem menuButtonAddDimension;
    private System.Windows.Forms.ToolStripMenuItem menuButtonRemoveDimension;
    private System.Windows.Forms.ToolStripMenuItem menuButtonAddToTaoSuite;
    private System.Windows.Forms.ToolStripMenuItem menuButtonRemoveFromTaoSuite;
    private System.Windows.Forms.ToolStripMenuItem menuButtonShowTaoSuite;
  }
}