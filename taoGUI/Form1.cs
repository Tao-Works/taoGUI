using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace taoGUI {
  public partial class Form1 : Form {
    private int countProjectsInTreeView;
    private TabControl tabCtrlAppStatus;                                            // This is the main "tab control" container for status reports
    private List<TabPage> appStatusTabPages = new List<TabPage>();                  // This is the tab pages, each represent one specific Tao App.
    private List<TabControl> statusReportsTabControl = new List<TabControl>();      // This is a sub "tab control" contained within each Tao App.
    private List<TabPage> statusReportsTabPages = new List<TabPage>();              // This is the sub tab pages per report per Tao Application.

    private string showProjectApplicationInTreeView(string line) {
      string applicationId = line.Substring(line.IndexOf(":") + 3);
      applicationId = applicationId.Substring(0, applicationId.Length - 2);
      taoProjectView.Nodes.Add(applicationId, applicationId);
      taoProjectView.Nodes[countProjectsInTreeView].Nodes.Add(applicationId + "|status", "Application Status");
      taoProjectView.Nodes[countProjectsInTreeView].Nodes[0].Nodes.Add(applicationId + "|status|reports", "Tao Suite Reports");
      taoProjectView.Nodes[countProjectsInTreeView].Nodes[0].Nodes.Add(applicationId + "|status|summary", "Summary");
      taoProjectView.Nodes[countProjectsInTreeView].Nodes[0].Nodes.Add(applicationId + "|status|velocity", "Velocity");
      taoProjectView.Nodes[countProjectsInTreeView].Nodes[0].Nodes.Add(applicationId + "|status|stability", "Stability");
      taoProjectView.Nodes[countProjectsInTreeView].Nodes.Add(applicationId + "|tao", "Tao Sheets");
      taoProjectView.Nodes[countProjectsInTreeView].Nodes.Add(applicationId + "|file", "File Structure");
      return applicationId;
    }

    private void showProjectDescriptionToolTip(string line) {
      string appIdToolTipText = line.Substring(line.IndexOf(":") + 3);
      appIdToolTipText = appIdToolTipText.Substring(0, appIdToolTipText.Length - 2);
      taoProjectView.Nodes[countProjectsInTreeView].ToolTipText = appIdToolTipText;
    }

    private void walkDirectoryStructure(System.IO.DirectoryInfo currentFolder, TreeNode currentNode, string keyName) {
      int nodeIndex = 0;
      foreach (var directory in currentFolder.GetDirectories()) {
        string extendedKeyName = keyName + "|" + directory.ToString();
        currentNode.Nodes.Add(extendedKeyName, directory.ToString());
        walkDirectoryStructure(directory, currentNode.Nodes[nodeIndex], extendedKeyName);
        nodeIndex++;
      }
    }

    private void showFileStructureInTreeView(string line, string applicationId, string keyName) {
      string rootDirectory = line.Substring(line.IndexOf(":") + 3);
      rootDirectory = rootDirectory.Substring(0, rootDirectory.Length - 1);
      string appFolderNmae = rootDirectory + "/" + applicationId;
      if (System.IO.Directory.Exists(appFolderNmae)) {
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(appFolderNmae);
        walkDirectoryStructure(di, taoProjectView.Nodes[countProjectsInTreeView].Nodes[2], keyName);
      }
    }

    public void showProjectsInTreeView() {
      countProjectsInTreeView = 0;
      taoProjectView.Nodes.Clear();
      string fileLocation = Application.StartupPath + @"\taoGUI.resources\projects.tao";
      if (System.IO.File.Exists(fileLocation)) {
        string line;
        System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
        while ((line = file.ReadLine()) != null) {
          if (line.Contains("\"applicationId\"")) {
            string applicationId = showProjectApplicationInTreeView(line);
            if ((line = file.ReadLine()) != null) {
              if (line.Contains("\"description\"")) {
                showProjectDescriptionToolTip(line);
              }
            }
            if ((line = file.ReadLine()) != null) {
              if (line.Contains("\"folder\"")) {
                showFileStructureInTreeView(line, applicationId, applicationId + "|file");
                countProjectsInTreeView++;
              }
            }
          }
        }
        file.Close();
      }
    }

    public void addProjectFile(string appName, string appDesc, string appFolder) {
      string fileLocation = Application.StartupPath + @"\taoGUI.resources\projects.tao";
      if (!System.IO.File.Exists(fileLocation)) {
        // Create a file to write to.
        using (System.IO.StreamWriter sw = System.IO.File.CreateText(fileLocation)) {
          sw.WriteLine("# -- DATA START --");
          sw.WriteLine("[");
          sw.WriteLine("   {");
          sw.WriteLine("      \"applicationId\" : \"" + appName + "\",");
          sw.WriteLine("      \"description\"   : \"" + appDesc + "\",");
          sw.WriteLine("      \"folder\"        : \"" + appFolder + "\"");
          sw.WriteLine("   }");
          sw.WriteLine("]");
          sw.WriteLine("# -- DATA END --");
          sw.Flush();
          sw.Close();
        }
      } else {
        // Cut last three lines off ...
        var lines = System.IO.File.ReadAllLines(fileLocation);
        System.IO.File.WriteAllLines(fileLocation, lines.Take(lines.Length - 3).ToArray());

        // This text is always added, making the file longer over time (if it is not deleted).
        using (System.IO.StreamWriter sw = System.IO.File.AppendText(fileLocation)) {
          sw.WriteLine("   },");
          sw.WriteLine("   {");
          sw.WriteLine("      \"applicationId\" : \"" + appName + "\",");
          sw.WriteLine("      \"description\"   : \"" + appDesc + "\",");
          sw.WriteLine("      \"folder\"        : \"" + appFolder + "\"");
          sw.WriteLine("   }");
          sw.WriteLine("]");
          sw.WriteLine("# -- DATA END --");
          sw.Flush();
          sw.Close();
        }
      }
    }

    public string getProjectFolderName(string appId) {
      string fileLocation = Application.StartupPath + @"\taoGUI.resources\projects.tao";
      string folderLocation = "";
      using (var sr = new System.IO.StreamReader(fileLocation)) {
        string line;
        while ((line = sr.ReadLine()) != null) {
          if ((line.Contains("\"applicationId\"") && line.Contains(appId))) {
            // Skip lines "description" and "folder" ...
            line = sr.ReadLine();
            line = sr.ReadLine();
            if (line.Contains("\"folder\"")) {
              folderLocation = line.Substring(line.IndexOf(":") + 3);
              folderLocation = folderLocation.Substring(0, folderLocation.Length-1);
            }
          }
        }
      }
      return folderLocation;
    }

    public void closeProjectFile(string appName) {
      string fileLocation = Application.StartupPath + @"\taoGUI.resources\projects.tao";
      string tempFile = System.IO.Path.GetTempFileName();
      using (var sr = new System.IO.StreamReader(fileLocation))
      using (var sw = new System.IO.StreamWriter(tempFile)) {
        string line;

        while ((line = sr.ReadLine()) != null) {
          if ((line.Contains("\"applicationId\"") && line.Contains(appName))) {
            // Skip lines "description" and "folder" ...
            line = sr.ReadLine();
            line = sr.ReadLine();
          } else {
            sw.WriteLine(line);
          }
        }
      }
      System.IO.File.Delete(fileLocation);
      System.IO.File.Move(tempFile, fileLocation);
    }

    public Form1() {
      InitializeComponent();
      showProjectsInTreeView();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
      // Display a message box asking users if they want to exit the application.
      if (MessageBox.Show("Do you really want to exit?", "Tao Commander", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
        Application.Exit();
      }

    }

    private void newToolStripMenuItem_Click(object sender, EventArgs e) {

    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e) {
      string DriveLetter = System.Environment.GetEnvironmentVariable("SystemDrive");
      openFileDialog1.Filter = "Licence Key (*.lic)|*.lic|Text Document (*.txt)|*.txt|All files (*.*)|*.*";
      openFileDialog1.Title = "Open Tao Licence";
      openFileDialog1.InitialDirectory = @DriveLetter;
      openFileDialog1.FileName = null;
      openFileDialog1.RestoreDirectory = false;
      if (openFileDialog1.ShowDialog() == DialogResult.OK) {
        string selectedFileName = openFileDialog1.FileName;
        string importLicenceKey = null;
        string importLicenceText = null;
        // Import details from Licence file to form...
        if (selectedFileName.Substring(selectedFileName.LastIndexOf(".") + 1) == "lic") {
          importLicenceKey = selectedFileName;
          importLicenceText = selectedFileName.Substring(0, selectedFileName.LastIndexOf(".") + 1) + "txt";
        } else if (selectedFileName.Substring(selectedFileName.LastIndexOf(".") + 1) == "txt") {
          importLicenceText = selectedFileName;
          importLicenceKey = selectedFileName.Substring(0, selectedFileName.LastIndexOf(".") + 1) + "lic";
        } else {
          MessageBox.Show("Not a valid Tao licence file.", "Open Tao Licence", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        // Check the files, load the content, display the results and disable the Tao Licence group...
        if (importLicenceKey != null && importLicenceText != null) {
          System.IO.FileInfo keyInfo = new System.IO.FileInfo(importLicenceKey);
          System.IO.FileInfo textInfo = new System.IO.FileInfo(importLicenceText);
          try {
            if (keyInfo.Length > 0) {
              // Copy licence key to new Tao application area
            }
          } catch {
            importLicenceKey = null;
            MessageBox.Show("Unable to find related Tao licence KEY (encrypted) file.", "Open Tao Licence", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          }
          try {
            if (textInfo.Length > 0) {
              // Read text file, populate form fields and disable (use file data to populate).
            }
          } catch {
            importLicenceText = null;
            if (importLicenceKey.Length > 0) {
              if (MessageBox.Show("Unable to find related Tao licence DESCRIPTION (text) file however, the licence KEY (encrypted) was found.  Do you want to continue?", "Open Tao Licence", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                // Scratch both licence key and text...
                importLicenceKey = null;
              } else {
                // Populate the form fields and diable the text fields (use licence file name to populate).
              }
            } else {
              MessageBox.Show("Unable to find related Tao licence DESCRIPTION (text) file.", "Open Tao Licence", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
          }
        }
        if (importLicenceKey != null) {
          // All good ...
          string filePath = importLicenceKey.Substring(0, importLicenceKey.LastIndexOf("\\"));
          filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
          string applicationId = filePath.Substring(filePath.LastIndexOf("\\") + 1);
          string rootFolder = filePath.Substring(0, filePath.LastIndexOf("\\"));
          addProjectFile(applicationId, "", rootFolder);
          showProjectsInTreeView();
        }
      }
    }

    private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {

    }

    private void projectToolStripMenuItem1_Click(object sender, EventArgs e) {
      Form2 newProj = new Form2(this);
      newProj.ShowDialog();
    }

    private void helpToolStripMenuItem_Click(object sender, EventArgs e) {
      AboutBox1 box = new AboutBox1();
      box.ShowDialog();
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e) {
      TreeNode currentNode = taoProjectView.SelectedNode;
      if (currentNode != null) {
        if (currentNode.Level > 0) {
          MessageBox.Show("Please select the Tao application ID you wish to close.", "Close Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        } else if ((MessageBox.Show("Do you really want to close Tao application \"" + currentNode.Text + "\"?", "Close Tao Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)) {
          closeProjectFile(currentNode.Text);
          showProjectsInTreeView();
        }
      } else {
        MessageBox.Show("No Tao application selected.", "Close Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void summaryOfDoneToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Summary", "|status|summary");
      }
    }

    private void summaryToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Weather - Actual", "|weather|actual");
      }
    }

    private string getProjectViewAppId() {
      string appId = null;
      if (taoProjectView.SelectedNode != null) {
        if (taoProjectView.SelectedNode.Name.IndexOf("|") > 0) {
          appId = taoProjectView.SelectedNode.Name.Substring(0, taoProjectView.SelectedNode.Name.IndexOf("|"));
        } else {
          appId = taoProjectView.SelectedNode.Name;
        }
      }
      return appId;
    }

    private void showAppStatusTabPage(string appId) {
      if (tabCtrlAppStatus == null) {
        tabCtrlAppStatus = new TabControl();
        appStatusTabPages.Add(new TabPage());
        // 
        // tabControl1
        // 
        tabCtrlAppStatus.Alignment = System.Windows.Forms.TabAlignment.Bottom;
        tabCtrlAppStatus.Controls.Add(appStatusTabPages[0]);
        tabCtrlAppStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        tabCtrlAppStatus.Location = new System.Drawing.Point(0, 24);
        tabCtrlAppStatus.Name = "tabCtrlAppStatus";
        tabCtrlAppStatus.SelectedIndex = 0;
        tabCtrlAppStatus.Size = new System.Drawing.Size(536, 361);
        tabCtrlAppStatus.TabIndex = 0;
        // 
        // tabPage1
        // 
        appStatusTabPages[0].Location = new System.Drawing.Point(4, 4);
        appStatusTabPages[0].Name = "appStatusTabPages_0";
        appStatusTabPages[0].Padding = new System.Windows.Forms.Padding(3);
        appStatusTabPages[0].Size = new System.Drawing.Size(528, 335);
        appStatusTabPages[0].TabIndex = 0;
        appStatusTabPages[0].Text = appId;
        appStatusTabPages[0].UseVisualStyleBackColor = true;
        // 
        // splitContainer1.Panel2
        // 
        splitContainer1.Panel2.Controls.Add(tabCtrlAppStatus);
        splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(0, 24, 0, 0);
        splitContainer1.Size = new System.Drawing.Size(809, 385);
        splitContainer1.SplitterDistance = 269;
        splitContainer1.TabIndex = 2;
      } else {
        // Search to see if the Tao application has been added already...
        int totalTabs = appStatusTabPages.Count;
        int currentTab = 0;
        bool foundTab = false;
        while (!foundTab && currentTab < totalTabs) {
          if (appStatusTabPages[currentTab].Text == appId) {
            foundTab = true;
          } else {
            currentTab++;
          }
        }
        if (foundTab) {
          tabCtrlAppStatus.SelectedIndex = currentTab;
        } else {
          appStatusTabPages.Add(new TabPage());
          tabCtrlAppStatus.Controls.Add(appStatusTabPages[totalTabs]);
          appStatusTabPages[totalTabs].Location = new System.Drawing.Point(4, 4);
          appStatusTabPages[totalTabs].Name = "appStatusTabPages_" + totalTabs.ToString();
          appStatusTabPages[totalTabs].Padding = new System.Windows.Forms.Padding(3);
          appStatusTabPages[totalTabs].Size = new System.Drawing.Size(528, 335);
          appStatusTabPages[totalTabs].TabIndex = totalTabs;
          appStatusTabPages[totalTabs].Text = appId;
          appStatusTabPages[totalTabs].UseVisualStyleBackColor = true;
          tabCtrlAppStatus.SelectedIndex = totalTabs;
        }
      }
    }

    private void addTabContentTaoSuiteReports(string appId, string tabReportName, TabPage tabPageContent) {
      string projectRootFolder = getProjectFolderName(appId) + @"\" + appId;
      // Create a "ribbon" effect for various control items (like filters and search)
      tabPageContent.Padding = new Padding(0, 24, 0, 0);
      // Create a database selector so that users can switch between instances and compare results
      System.Windows.Forms.ComboBox comboDbConnection = new System.Windows.Forms.ComboBox();
      comboDbConnection.FormattingEnabled = true;
      comboDbConnection.Location = new System.Drawing.Point(4, 28);
      comboDbConnection.Name = "comboDbConnection." + appId;
      comboDbConnection.Size = new System.Drawing.Size(200, 21);
      comboDbConnection.TabIndex = 0;
      comboDbConnection.Top = 0;
      comboDbConnection.Left = 0;
      comboDbConnection.Items.Add("All Database Connections");
      comboDbConnection.SelectedIndex = 0;
      // Read the connection files in the project config area
      string taoDbConnections = projectRootFolder + @"\conf";
      if (System.IO.Directory.Exists(taoDbConnections)) {
        string[] dbConnections = System.IO.Directory.GetFiles(taoDbConnections);
        foreach (string fileName in dbConnections) {
          string ConnectionName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          if (ConnectionName.Contains("Connection.")) {
            string dbConnection = ConnectionName.Substring(11);
            comboDbConnection.Items.Add(dbConnection.Substring(0, dbConnection.IndexOf(".")));
            comboDbConnection.SelectedIndex++;
          }
        }
      }
      // Create data table of results
      DataTable tableTaoSuiteReports = new DataTable();
      // Add columns to datatable
      tableTaoSuiteReports.Columns.Add("Tao Suite", typeof(string));
      tableTaoSuiteReports.Columns.Add("Pass Rate", typeof(string));
      tableTaoSuiteReports.Columns.Add("Last Execution", typeof(string));
      tableTaoSuiteReports.Columns.Add("Iterations", typeof(string));
      tableTaoSuiteReports.Columns.Add("Stability", typeof(string));
      // Get Tao Sheet Report data: read the project parameters, assume Tao sheets are located at root/taoSuite_Input/*.xls
      string taoSuiteInputFolder = projectRootFolder + @"\taoSuite_Input";
      if (System.IO.Directory.Exists(taoSuiteInputFolder)) {
        string[] fileEntries = System.IO.Directory.GetFiles(taoSuiteInputFolder);
        foreach (string fileName in fileEntries) {
          string suiteName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          tableTaoSuiteReports.Rows.Add(suiteName);
        }
      }
      // Attach the data table to the data grid view control
      DataGridView taoSheets = new DataGridView();
      taoSheets.DataSource = tableTaoSuiteReports;
      // Disable (for now) user ability to insert, update or delete data in the grid view.
      taoSheets.AllowUserToAddRows = false;
      taoSheets.AllowUserToDeleteRows = false;
      taoSheets.ReadOnly = true;
      // Format the data grid view and then attach to the Tab page.
      taoSheets.BorderStyle = BorderStyle.None;
      taoSheets.BackgroundColor = Color.LightGray;
      taoSheets.Dock = System.Windows.Forms.DockStyle.Fill;
      // Attach the data grid view to a container, add pading top to the container (24px) to give room to some controls (e.g. drop down list and search)
      tabPageContent.Controls.Add(comboDbConnection);
      tabPageContent.Controls.Add(taoSheets);
      // Resize "works" once the data is painted to the control
      taoSheets.AutoResizeColumns(); 
    }

    private void addTabContent(string appId, string tabReportName, TabPage tabPageContent) {
      switch (tabReportName) {
        case "Tao Suite Reports":
          addTabContentTaoSuiteReports(appId, tabReportName, tabPageContent);
          break;
        case "Summary":
          break;
        case "Velocity":
          break;
        case "Stability":
          break;
        case "Weather - Actual":
          break;
        case "Weather - Forecast":
          break;
        default:
          break;
      }
    }

    private void showStatusReprtTabPage(string appId, string tabReportName, string tabReportKey) {
      int currentMainTab = tabCtrlAppStatus.SelectedIndex;
      int totalSubTabs = statusReportsTabControl.Count;
      int currentSubTab = 0;
      bool foundTab = false;

      // Search to see if the Tao application has been added already...
      while (!foundTab && currentSubTab < totalSubTabs) {
        if (statusReportsTabControl[currentSubTab].Name == appId) {
          foundTab = true;
        } else {
          currentSubTab++;
        }
      }

      if (!foundTab) {
        // Add the tab control "application ID" to sit inside "main tab page".
        statusReportsTabControl.Add(new TabControl());
        appStatusTabPages[currentMainTab].Controls.Add(statusReportsTabControl[currentSubTab]);
        statusReportsTabControl[currentSubTab].Alignment = TabAlignment.Bottom;
        statusReportsTabControl[currentSubTab].Dock = System.Windows.Forms.DockStyle.Fill;
        statusReportsTabControl[currentSubTab].Location = new System.Drawing.Point(3, 3);
        statusReportsTabControl[currentSubTab].Name = appId;
        statusReportsTabControl[currentSubTab].TabIndex = currentSubTab;
        statusReportsTabControl[currentSubTab].SelectedIndex = currentSubTab;
        statusReportsTabControl[currentSubTab].Size = new System.Drawing.Size(522, 329);
        // Add "tab page" (name of report) to sit inside tab control "application ID"
        int countStatusReportsTabPages = statusReportsTabPages.Count();
        statusReportsTabPages.Add(new TabPage());
        statusReportsTabControl[currentSubTab].Controls.Add(statusReportsTabPages[countStatusReportsTabPages]);
        statusReportsTabPages[countStatusReportsTabPages].Location = new System.Drawing.Point(4, 4);
        statusReportsTabPages[countStatusReportsTabPages].Name = appId + tabReportKey;
        statusReportsTabPages[countStatusReportsTabPages].Padding = new System.Windows.Forms.Padding(3);
        statusReportsTabPages[countStatusReportsTabPages].Size = new System.Drawing.Size(192, 74);
        statusReportsTabPages[countStatusReportsTabPages].TabIndex = 0; // First tab within first block
        statusReportsTabPages[countStatusReportsTabPages].Text = tabReportName;
        statusReportsTabPages[countStatusReportsTabPages].UseVisualStyleBackColor = true;
        addTabContent(appId, tabReportName, statusReportsTabPages[countStatusReportsTabPages]);
      } else {
        statusReportsTabControl[currentSubTab].SelectedIndex = currentSubTab;
        int totalTabPages = statusReportsTabPages.Count;
        int currentTabPage = 0;
        int currentTabIndex = 0;
        foundTab = false;
        while (!foundTab && currentTabPage < totalTabPages) {
          if (statusReportsTabPages[currentTabPage].Name == appId + tabReportKey) {
            foundTab = true;
          } else {
            if (statusReportsTabPages[currentTabPage].Name.Contains(appId)) {
              currentTabIndex++;
            }
            currentTabPage++;
          }
        }
        if (!foundTab) {
          int countStatusReportsTabPages = statusReportsTabPages.Count();
          statusReportsTabPages.Add(new TabPage());
          statusReportsTabControl[currentSubTab].Controls.Add(statusReportsTabPages[countStatusReportsTabPages]);
          statusReportsTabPages[countStatusReportsTabPages].Location = new System.Drawing.Point(4, 4);
          statusReportsTabPages[countStatusReportsTabPages].Name = appId + tabReportKey;
          statusReportsTabPages[countStatusReportsTabPages].Padding = new System.Windows.Forms.Padding(3);
          statusReportsTabPages[countStatusReportsTabPages].Size = new System.Drawing.Size(192, 74);
          statusReportsTabPages[countStatusReportsTabPages].TabIndex = currentTabIndex;
          statusReportsTabPages[countStatusReportsTabPages].Text = tabReportName;
          statusReportsTabPages[countStatusReportsTabPages].UseVisualStyleBackColor = true;
          statusReportsTabControl[currentSubTab].SelectedIndex = currentTabIndex;
          addTabContent(appId, tabReportName, statusReportsTabPages[countStatusReportsTabPages]);
        } else {
          statusReportsTabControl[currentSubTab].SelectedIndex = currentTabIndex;
        }
      }
    }

    private void taoSuiteReportsToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Tao Suite Reports", "|status|reports");
      }
    }

    private void launchAppStatusTabPage_DoubleClick(object sender, EventArgs e) {
      if (taoProjectView.SelectedNode != null) {
        if (taoProjectView.SelectedNode.Name.Contains("|status|reports")) {
          taoSuiteReportsToolStripMenuItem_Click(sender, e);
        }
        if (taoProjectView.SelectedNode.Name.Contains("|status|summary")) {
          summaryOfDoneToolStripMenuItem_Click(sender, e);
        }
        if (taoProjectView.SelectedNode.Name.Contains("|status|velocity")) {
          velocityOfToolStripMenuItem_Click(sender, e);
        }
        if (taoProjectView.SelectedNode.Name.Contains("|status|stability")) {
          taoApplicationStatisticsToolStripMenuItem_Click(sender, e);
        }
      }
    }

    private void velocityOfToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Velocity", "|status|velocity");
      }
    }

    private void taoApplicationStatisticsToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Stability", "|status|stability");
      }
    }

    private void forecastToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Weather - Forecast", "|weather|forecast");
      }
    }
  }
}
