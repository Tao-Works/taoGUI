namespace taoGUI
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCompanyId = new System.Windows.Forms.TextBox();
            this.textBoxProjectId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxUserId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxApplicationName = new System.Windows.Forms.TextBox();
            this.monthCalendarExpiry = new System.Windows.Forms.MonthCalendar();
            this.radioButtonTPlus1M = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.radioButtonTPlus3M = new System.Windows.Forms.RadioButton();
            this.radioButtonTPlus6M = new System.Windows.Forms.RadioButton();
            this.radioButtonTPlus12M = new System.Windows.Forms.RadioButton();
            this.radioButtonTPlusX = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxApplicationDescription = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxApplicationFolder = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.None;
            this.button1.Location = new System.Drawing.Point(296, 446);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(377, 446);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Company ID";
            // 
            // textBoxCompanyId
            // 
            this.textBoxCompanyId.Location = new System.Drawing.Point(86, 19);
            this.textBoxCompanyId.Name = "textBoxCompanyId";
            this.textBoxCompanyId.Size = new System.Drawing.Size(342, 20);
            this.textBoxCompanyId.TabIndex = 3;
            // 
            // textBoxProjectId
            // 
            this.textBoxProjectId.Location = new System.Drawing.Point(86, 46);
            this.textBoxProjectId.Name = "textBoxProjectId";
            this.textBoxProjectId.Size = new System.Drawing.Size(342, 20);
            this.textBoxProjectId.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Project ID";
            // 
            // textBoxUserId
            // 
            this.textBoxUserId.Location = new System.Drawing.Point(86, 73);
            this.textBoxUserId.Name = "textBoxUserId";
            this.textBoxUserId.Size = new System.Drawing.Size(342, 20);
            this.textBoxUserId.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "User ID";
            // 
            // textBoxApplicationName
            // 
            this.textBoxApplicationName.Location = new System.Drawing.Point(93, 19);
            this.textBoxApplicationName.Name = "textBoxApplicationName";
            this.textBoxApplicationName.Size = new System.Drawing.Size(334, 20);
            this.textBoxApplicationName.TabIndex = 12;
            // 
            // monthCalendarExpiry
            // 
            System.DateTime licenceEnd = new System.DateTime(System.DateTime.UtcNow.Year, 12, 31);
            if (System.DateTime.UtcNow.Month.Equals(12))
            {
                licenceEnd = licenceEnd.AddYears(1);
            }
            this.monthCalendarExpiry.Location = new System.Drawing.Point(201, 108);
            this.monthCalendarExpiry.Name = "monthCalendarExpiry";
            this.monthCalendarExpiry.TabIndex = 11;
            this.monthCalendarExpiry.SetDate(licenceEnd);
            monthCalendarExpiry.DateChanged += new System.Windows.Forms.DateRangeEventHandler(monthCalendarExpiry_DateChanged);
            // 
            // radioButtonTPlus1M
            // 
            this.radioButtonTPlus1M.AutoSize = true;
            this.radioButtonTPlus1M.Location = new System.Drawing.Point(86, 105);
            this.radioButtonTPlus1M.Name = "radioButtonTPlus1M";
            this.radioButtonTPlus1M.Size = new System.Drawing.Size(64, 17);
            this.radioButtonTPlus1M.TabIndex = 6;
            this.radioButtonTPlus1M.TabStop = true;
            this.radioButtonTPlus1M.Text = "1 Month";
            this.radioButtonTPlus1M.UseVisualStyleBackColor = true;
            radioButtonTPlus1M.Click += new System.EventHandler(radioButtonTPlus1M_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "End Date";
            // 
            // radioButtonTPlus3M
            // 
            this.radioButtonTPlus3M.AutoSize = true;
            this.radioButtonTPlus3M.Location = new System.Drawing.Point(86, 128);
            this.radioButtonTPlus3M.Name = "radioButtonTPlus3M";
            this.radioButtonTPlus3M.Size = new System.Drawing.Size(69, 17);
            this.radioButtonTPlus3M.TabIndex = 7;
            this.radioButtonTPlus3M.TabStop = true;
            this.radioButtonTPlus3M.Text = "3 Months";
            this.radioButtonTPlus3M.UseVisualStyleBackColor = true;
            radioButtonTPlus3M.Click += new System.EventHandler(radioButtonTPlus3M_Click);
            // 
            // radioButtonTPlus6M
            // 
            this.radioButtonTPlus6M.AutoSize = true;
            this.radioButtonTPlus6M.Location = new System.Drawing.Point(86, 151);
            this.radioButtonTPlus6M.Name = "radioButtonTPlus6M";
            this.radioButtonTPlus6M.Size = new System.Drawing.Size(69, 17);
            this.radioButtonTPlus6M.TabIndex = 8;
            this.radioButtonTPlus6M.TabStop = true;
            this.radioButtonTPlus6M.Text = "6 Months";
            this.radioButtonTPlus6M.UseVisualStyleBackColor = true;
            radioButtonTPlus6M.Click += new System.EventHandler(radioButtonTPlus6M_Click);
            // 
            // radioButtonTPlus12M
            // 
            this.radioButtonTPlus12M.AutoSize = true;
            this.radioButtonTPlus12M.Location = new System.Drawing.Point(86, 174);
            this.radioButtonTPlus12M.Name = "radioButtonTPlus12M";
            this.radioButtonTPlus12M.Size = new System.Drawing.Size(75, 17);
            this.radioButtonTPlus12M.TabIndex = 9;
            this.radioButtonTPlus12M.TabStop = true;
            this.radioButtonTPlus12M.Text = "12 Months";
            this.radioButtonTPlus12M.UseVisualStyleBackColor = true;
            radioButtonTPlus12M.Click += new System.EventHandler(radioButtonTPlus12M_Click);
            // 
            // radioButtonTPlusX
            // 
            this.radioButtonTPlusX.AutoSize = true;
            this.radioButtonTPlusX.Location = new System.Drawing.Point(86, 197);
            this.radioButtonTPlusX.Name = "radioButtonTPlusX";
            this.radioButtonTPlusX.Size = new System.Drawing.Size(60, 17);
            this.radioButtonTPlusX.TabIndex = 10;
            this.radioButtonTPlusX.TabStop = true;
            this.radioButtonTPlusX.Text = "Custom";
            this.radioButtonTPlusX.UseVisualStyleBackColor = true;
            this.radioButtonTPlusX.PerformClick();
            radioButtonTPlusX.Click += new System.EventHandler(radioButtonTPlusX_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(73, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Application ID";
            // 
            // textBoxApplicationDescription
            // 
            this.textBoxApplicationDescription.Location = new System.Drawing.Point(93, 45);
            this.textBoxApplicationDescription.Name = "textBoxApplicationDescription";
            this.textBoxApplicationDescription.Size = new System.Drawing.Size(334, 20);
            this.textBoxApplicationDescription.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Description";
            // 
            // textBoxApplicationFolder
            // 
            this.textBoxApplicationFolder.Location = new System.Drawing.Point(93, 71);
            this.textBoxApplicationFolder.Name = "textBoxApplicationFolder";
            this.textBoxApplicationFolder.Size = new System.Drawing.Size(253, 20);
            this.textBoxApplicationFolder.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(36, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Folder";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(352, 69);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 15;
            this.button3.Text = "Browse...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.DialogResult = System.Windows.Forms.DialogResult.None;
            this.button4.Location = new System.Drawing.Point(12, 446);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 30;
            this.button4.Text = "Import...";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxCompanyId);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxProjectId);
            this.groupBox1.Controls.Add(this.textBoxUserId);
            this.groupBox1.Controls.Add(this.radioButtonTPlus1M);
            this.groupBox1.Controls.Add(this.radioButtonTPlus3M);
            this.groupBox1.Controls.Add(this.radioButtonTPlusX);
            this.groupBox1.Controls.Add(this.monthCalendarExpiry);
            this.groupBox1.Controls.Add(this.radioButtonTPlus6M);
            this.groupBox1.Controls.Add(this.radioButtonTPlus12M);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(440, 282);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tao Licence";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.textBoxApplicationDescription);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.textBoxApplicationName);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.textBoxApplicationFolder);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(13, 301);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(439, 104);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tao Application";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "LicData";
            this.openFileDialog2.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog2_FileOk);
            // 
            // Form2
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(464, 481);
            this.ControlBox = false;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form2";
            this.ShowIcon = false;
            this.Text = "New Tao Application";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCompanyId;
        private System.Windows.Forms.TextBox textBoxProjectId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxUserId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxApplicationName;
        private System.Windows.Forms.MonthCalendar monthCalendarExpiry;
        private System.Windows.Forms.RadioButton radioButtonTPlus1M;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButtonTPlus3M;
        private System.Windows.Forms.RadioButton radioButtonTPlus6M;
        private System.Windows.Forms.RadioButton radioButtonTPlus12M;
        private System.Windows.Forms.RadioButton radioButtonTPlusX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxApplicationDescription;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxApplicationFolder;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private string importLicenceKey;
        private string importLicenceText;
    }
}