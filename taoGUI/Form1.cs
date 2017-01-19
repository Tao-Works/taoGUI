using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using System.IO;
using taoGUI.Json;
using static taoGUI.Json.TaoJsonConfigReader;

namespace taoGUI {
  public partial class Form1 : Form {

    public static string TAO_PROJECT_FILE = Application.StartupPath + @"\taoGUI.resources\projects.tao";

    private TabControl tabCtrlAppStatus;                                            // This is the main "tab control" container for status reports
    private List<TabPage> appStatusTabPages = new List<TabPage>();                  // This is the tab pages, each represent one specific Tao App.
    private List<TabControl> statusReportsTabControl = new List<TabControl>();      // This is a sub "tab control" contained within each Tao App.
    private List<TabPage> statusReportsTabPages = new List<TabPage>();              // This is the sub tab pages per report per Tao Application.

    private void walkDirectoryStructure(System.IO.DirectoryInfo currentFolder, TreeNode currentNode, string keyName) {
      int nodeIndex = 0;
      foreach (var directory in currentFolder.GetDirectories()) {
        string extendedKeyName = keyName + "|" + directory.ToString();
        currentNode.Nodes.Add(extendedKeyName, directory.ToString());
        walkDirectoryStructure(directory, currentNode.Nodes[nodeIndex], extendedKeyName);
        nodeIndex++;
      }
    }

    public void showProjectsInTreeView() {
      taoProjectView.Nodes.Clear();
      if (File.Exists(TAO_PROJECT_FILE)) {
        Dictionary<string, TaoJsonProjectCtx> projectContextMap = TaoJsonConfigReader.getTaoProjectCtxMap(TAO_PROJECT_FILE);
        foreach (TaoJsonProjectCtx ctx in projectContextMap.Values) {
          string applicationId = ctx.applicationId;
          TreeNode prjNode = taoProjectView.Nodes.Add(applicationId, applicationId);
          prjNode.ToolTipText = applicationId;
          TreeNode l1_node = prjNode.Nodes.Add(applicationId + "|status", "Application Status");
          l1_node.Nodes.Add(applicationId + "|status|reports", "Tao Suite Reports");
          l1_node.Nodes.Add(applicationId + "|status|summary", "Summary");
          l1_node.Nodes.Add(applicationId + "|status|velocity", "Velocity");
          l1_node.Nodes.Add(applicationId + "|status|stability", "Stability");
          prjNode.Nodes.Add(applicationId + "|tao", "Tao Sheets");
          TreeNode fileStrctNode = prjNode.Nodes.Add(applicationId + "|file", "File Structure");
          string appFolderName = ctx.getAppFolder();
          if (System.IO.Directory.Exists(appFolderName)) {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(appFolderName);
            walkDirectoryStructure(di, fileStrctNode, applicationId + "|file");
          }
        }
      }
    }

    public void addProjectFile(string appName, string appDesc, string appFolder) {
      string fileLocation = TAO_PROJECT_FILE;
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
      string fileLocation = TAO_PROJECT_FILE;
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
              folderLocation = folderLocation.Substring(0, folderLocation.Length - 1);
            }
          }
        }
      }
      return folderLocation;
    }

    public void closeProjectFile(string appName) {
      string fileLocation = TAO_PROJECT_FILE;
      string tempFile = System.IO.Path.GetTempFileName();
      using (var sr = new System.IO.StreamReader(fileLocation))
      using (var sw = new System.IO.StreamWriter(tempFile)) {
        string line = string.Empty;
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

    ~Form1() {
      GC.Collect();
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

    private void changeDbConnectionTaoSuiteReports(object sender, EventArgs e, string appId, string projectRootFolder, string dbInstance, DataTable tableTaoSuiteReports) {
      TaoReportCache tmpCacheStatus = new TaoReportCache(projectRootFolder, appId, dbInstance);
      if (!tmpCacheStatus.isCacheCurrent()) {
        MessageBox.Show("The statistics for the Tao Suite Reports need re-calculating.  This will take a short while (please be patient).", "Re-calculating Statistics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        tmpCacheStatus.updateCacheResults();
      }
      DataTable tmpCache = tmpCacheStatus.getCacheDataTable();
      int totalRows = tmpCache.Rows.Count;
      for (int i = 0; i < totalRows; i++) {
        var t = tableTaoSuiteReports.Rows[i];
        var s = tmpCache.Rows[i];
        t["taoSuiteName"] = s["taoSuiteName"];
        t["taoSuiteFirstRun"] = s["taoSuiteFirstRun"];
        t["taoSuiteLastRun"] = s["taoSuiteLastRun"];
        t["taoSuiteIterations"] = s["taoSuiteIterations"];
        t["passRate"] = s["passRate"];
        t["passRateDelta"] = s["passRateDelta"];
        t["passRateMean"] = s["passRateMean"];
        t["passRateStdDev"] = s["passRateStdDev"];
        t["lowerBollingerBand"] = s["lowerBollingerBand"];
        t["upperBollingerBand"] = s["upperBollingerBand"];
        t["impliedVolatility"] = s["impliedVolatility"];
      }
    }

    private void changeDgvFilter(object sender, EventArgs e, string dgvFilter, DataGridView taoSheetView) {
      switch (dgvFilter) {
        case @"Filter All Rows":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
          break;
        case @"Success - pass rate = 100%":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRate = 100 AND taoSuiteIterations > 0");
          break;
        case @"Failure - pass rate < 100%":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRate < 100 AND taoSuiteIterations > 0");
          break;
        case @"Moderate Failure - pass rate < mean":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRate < passRateMean AND taoSuiteIterations > 0");
          break;
        case @"Heavy Failure - pass rate < lower band":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRate < lowerBollingerBand AND taoSuiteIterations > 0");
          break;
        case @"Trending Up - delta > 0":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRateDelta > 0 AND taoSuiteIterations > 0");
          break;
        case @"Trending Down - delta < 0":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRateDelta < 0 AND taoSuiteIterations > 0");
          break;
        case @"Progress - pass < 100% and delta > 0":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRate < 100 AND passRateDelta > 0 AND taoSuiteIterations > 0");
          break;
        case @"Regress - pass < 100% and delta < 0":
          (taoSheetView.DataSource as DataTable).DefaultView.RowFilter = string.Format(@"passRate < 100 AND passRateDelta < 0 AND taoSuiteIterations > 0");
          break;
        default:
          break;
      }
    }

    private void taoSheets_CellFormatting(object sender, System.Windows.Forms.DataGridViewCellFormattingEventArgs e, DataGridView taoSheetData) {
      if (taoSheetData.Columns[e.ColumnIndex].Name.Equals("passRate")) {
        if (0.0 < (double)e.Value && (double)e.Value < 100.0) {
          e.CellStyle.BackColor = Color.LemonChiffon;
          e.CellStyle.SelectionBackColor = Color.DarkOrange;
        }
      }
      if (taoSheetData.Columns[e.ColumnIndex].Name.Equals("passRateDelta")) {
        if ((double)e.Value < 0.0) {
          e.CellStyle.BackColor = Color.LightPink;
          e.CellStyle.SelectionBackColor = Color.DarkRed;
        } else if ((double)e.Value > 0.0) {
          e.CellStyle.BackColor = Color.LightGreen;
          e.CellStyle.SelectionBackColor = Color.DarkGreen;
        }
      }
    }

    private void button_ShowTaoSuiteHistogram(object sender, EventArgs e, string projectRootFolder, DataGridView taoSheetData, string dbInstance) {
      DataGridViewSelectedRowCollection rows = taoSheetData.SelectedRows;
      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          string taoSheet = myRow.ItemArray[0].ToString();
          double passRateMean = Convert.ToDouble(myRow.ItemArray[6].ToString());
          double passRateStdDev = Convert.ToDouble(myRow.ItemArray[7].ToString());
          double lowerBollingerBand = Convert.ToDouble(myRow.ItemArray[8].ToString());
          double upperBollingerBand = Convert.ToDouble(myRow.ItemArray[9].ToString());
          double impliedVolatility = Convert.ToDouble(myRow.ItemArray[10].ToString());
          TaoSuiteReportChart taoChart = new TaoSuiteReportChart(projectRootFolder, taoSheet, dbInstance, passRateMean, passRateStdDev, lowerBollingerBand);
          taoChart.Show();
        }
      } else {
        Int32 selectedCellCount = taoSheetData.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          List<string> taoSheets = new List<string>();
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = taoSheetData.SelectedCells[i].RowIndex;
            string taoSheet = taoSheetData.Rows[rowIndex].Cells[0].Value.ToString();
            double passRateMean = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[6].Value.ToString());
            double passRateStdDev = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[7].Value.ToString());
            double lowerBollingerBand = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[8].Value.ToString());
            double upperBollingerBand = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[9].Value.ToString());
            double impliedVolatility = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[10].Value.ToString());
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
              TaoSuiteReportChart taoChart = new TaoSuiteReportChart(projectRootFolder, taoSheet, dbInstance, passRateMean, passRateStdDev, lowerBollingerBand);
              taoChart.Show();
            }
          }
        } else {
          MessageBox.Show("Unable to chart the pass rate histogramm as no Tao Suite was selected.", "Pass Rate Statistics", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
    }

    private void button_ShowTaoSuiteStatistics(object sender, EventArgs e, string projectRootFolder, DataGridView taoSheetData, string dbInstance) {
      DataGridViewSelectedRowCollection rows = taoSheetData.SelectedRows;
      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          string taoSheet = myRow.ItemArray[0].ToString();
          double passRateMean = Convert.ToDouble(myRow.ItemArray[6].ToString());
          double passRateStdDev = Convert.ToDouble(myRow.ItemArray[7].ToString());
          double lowerBollingerBand = Convert.ToDouble(myRow.ItemArray[8].ToString());
          double upperBollingerBand = Convert.ToDouble(myRow.ItemArray[9].ToString());
          double impliedVolatility = Convert.ToDouble(myRow.ItemArray[10].ToString());
          TaoSuiteReportChart taoChart = new TaoSuiteReportChart(projectRootFolder, taoSheet, dbInstance, passRateMean, passRateStdDev, lowerBollingerBand, upperBollingerBand, impliedVolatility);
          taoChart.Show();
        }
      } else {
        Int32 selectedCellCount = taoSheetData.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          List<string> taoSheets = new List<string>();
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = taoSheetData.SelectedCells[i].RowIndex;
            string taoSheet = taoSheetData.Rows[rowIndex].Cells[0].Value.ToString();
            double passRateMean = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[6].Value.ToString());
            double passRateStdDev = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[7].Value.ToString());
            double lowerBollingerBand = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[8].Value.ToString());
            double upperBollingerBand = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[9].Value.ToString());
            double impliedVolatility = Convert.ToDouble(taoSheetData.Rows[rowIndex].Cells[10].Value.ToString());
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
              TaoSuiteReportChart taoChart = new TaoSuiteReportChart(projectRootFolder, taoSheet, dbInstance, passRateMean, passRateStdDev, lowerBollingerBand, upperBollingerBand, impliedVolatility);
              taoChart.Show();
            }
          }
        } else {
          MessageBox.Show("Unable to chart the pass rate statistics as no Tao Suite was selected.", "Pass Rate Statistics", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
    }

    private void button_ShowTaoSuiteHistory(object sender, EventArgs e, SeriesChartType targetChartType, string projectRootFolder, DataGridView taoSheetData, string dbInstance) {
      DataGridViewSelectedRowCollection rows = taoSheetData.SelectedRows;
      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          TaoSuiteReportChart taoChart = new TaoSuiteReportChart(targetChartType, projectRootFolder, myRow.ItemArray[0].ToString(), dbInstance);
          taoChart.Show();
        }
      } else {
        Int32 selectedCellCount = taoSheetData.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          List<string> taoSheets = new List<string>();
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = taoSheetData.SelectedCells[i].RowIndex;
            string taoSheet = taoSheetData.Rows[rowIndex].Cells[0].Value.ToString();
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
              TaoSuiteReportChart taoChart = new TaoSuiteReportChart(targetChartType, projectRootFolder, taoSheet, dbInstance);
              taoChart.Show();
            }
          }
        } else {
          MessageBox.Show("Unable to chart the pass rate history as no Tao Suite was selected.", "Pass Rate History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
    }

    private void button_OpenTaoSheet(object sender, EventArgs e) {
    }

    private void addTabContentTaoSuiteReports(string appId, TabPage tabPageContent) {

      DataTable tableTaoSuiteReports;
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
      string dirLocOfDbConnection = projectRootFolder + @"\conf";
      if (System.IO.Directory.Exists(dirLocOfDbConnection)) {
        string[] dbConnections = System.IO.Directory.GetFiles(dirLocOfDbConnection);
        foreach (string fileName in dbConnections) {
          string ConnectionName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          if (ConnectionName.Contains("Connection.")) {
            string dbConnection = ConnectionName.Substring(11);
            comboDbConnection.Items.Add(dbConnection.Substring(0, dbConnection.IndexOf(".")));
            comboDbConnection.SelectedIndex++;
          }
        }
      }
      string dbInstance = comboDbConnection.Items[comboDbConnection.SelectedIndex].ToString();

      // Create some filters so that user can focus on specific Tao Suite Result constellations
      System.Windows.Forms.ComboBox comboDgvFilter = new System.Windows.Forms.ComboBox();
      comboDgvFilter.FormattingEnabled = true;
      comboDgvFilter.Location = new System.Drawing.Point(4, 28);
      comboDgvFilter.Name = "comboDgvFilter." + appId;
      comboDgvFilter.Size = new System.Drawing.Size(204, 21);
      comboDgvFilter.TabIndex = 0;
      comboDgvFilter.Top = 0;
      comboDgvFilter.Left = 204;
      comboDgvFilter.Items.Add("Filter All Rows");
      comboDgvFilter.Items.Add("Success - pass rate = 100%");
      comboDgvFilter.Items.Add("Failure - pass rate < 100%");
      comboDgvFilter.Items.Add("Moderate Failure - pass rate < mean");
      comboDgvFilter.Items.Add("Heavy Failure - pass rate < lower band");
      comboDgvFilter.Items.Add("Trending Up - delta > 0");
      comboDgvFilter.Items.Add("Trending Down - delta < 0");
      comboDgvFilter.Items.Add("Progress - pass < 100% and delta > 0");
      comboDgvFilter.Items.Add("Regress - pass < 100% and delta < 0");
      comboDgvFilter.SelectedIndex = 0;

      // Check cache status and alert if update necessary...
      TaoReportCache currentCacheStatus = new TaoReportCache(projectRootFolder, appId, dbInstance);
      if (!currentCacheStatus.isCacheCurrent()) {
        MessageBox.Show("The statistics for the Tao Suite Reports need re-calculating.  This will take a short while (please be patient).", "Re-calculating Statistics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        currentCacheStatus.updateCacheResults();
      }
      tableTaoSuiteReports = currentCacheStatus.getCacheDataTable();
      // Register a method on the event "change list" and pass data table as parameter
      comboDbConnection.SelectedValueChanged += new System.EventHandler((sender, e) => changeDbConnectionTaoSuiteReports(sender, e, appId, projectRootFolder, comboDbConnection.Items[comboDbConnection.SelectedIndex].ToString(), tableTaoSuiteReports));
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
      tabPageContent.Controls.Add(comboDgvFilter);
      tabPageContent.Controls.Add(taoSheets);
      // Formats
      taoSheets.Columns["taoSuiteName"].HeaderText = "Tao Suite";
      taoSheets.Columns["taoSuiteFirstRun"].HeaderText = "First Run";
      taoSheets.Columns["taoSuiteLastRun"].HeaderText = "Last Run";
      taoSheets.Columns["taoSuiteIterations"].HeaderText = "Iterations";
      taoSheets.Columns["passRate"].HeaderText = "Pass Rate";
      taoSheets.Columns["passRateDelta"].HeaderText = "Delta";
      taoSheets.Columns["passRateMean"].HeaderText = "Mean";
      taoSheets.Columns["passRateStdDev"].HeaderText = "Std. Dev.";
      taoSheets.Columns["lowerBollingerBand"].HeaderText = "Lower Band";
      taoSheets.Columns["upperBollingerBand"].HeaderText = "Upper Band";
      taoSheets.Columns["impliedVolatility"].HeaderText = "Volatility";
      taoSheets.Columns["taoSuiteFirstRun"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      taoSheets.Columns["taoSuiteLastRun"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      taoSheets.Columns["taoSuiteIterations"].DefaultCellStyle.Format = "N0";
      taoSheets.Columns["taoSuiteIterations"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.Columns["passRate"].DefaultCellStyle.Format = "N4";
      taoSheets.Columns["passRate"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.Columns["passRateDelta"].DefaultCellStyle.Format = "N4";
      taoSheets.Columns["passRateDelta"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.Columns["passRateMean"].DefaultCellStyle.Format = "N4";
      taoSheets.Columns["passRateMean"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.Columns["passRateStdDev"].DefaultCellStyle.Format = "N4";
      taoSheets.Columns["passRateStdDev"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.Columns["lowerBollingerBand"].DefaultCellStyle.Format = "N4";
      taoSheets.Columns["lowerBollingerBand"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.Columns["upperBollingerBand"].DefaultCellStyle.Format = "N4";
      taoSheets.Columns["upperBollingerBand"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.Columns["impliedVolatility"].DefaultCellStyle.Format = "N4";
      taoSheets.Columns["impliedVolatility"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      taoSheets.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler((sender, e) => taoSheets_CellFormatting(sender, e, taoSheets));
      // Resize "works" once the data is painted to the control
      taoSheets.AutoResizeColumns();
      // Register a method to filter the data grid view by setting the data view selectors (internal)
      comboDgvFilter.SelectedValueChanged += new System.EventHandler((sender, e) => changeDgvFilter(sender, e, comboDgvFilter.Items[comboDgvFilter.SelectedIndex].ToString(), taoSheets));

      // Buttons ...
      System.Windows.Forms.Button buttonTaoSuiteReportPerc = new System.Windows.Forms.Button();
      buttonTaoSuiteReportPerc.Image = global::taoGUI.Properties.Resources.Percent;
      buttonTaoSuiteReportPerc.Location = new System.Drawing.Point(414, -1);
      buttonTaoSuiteReportPerc.Name = "buttonTaoSuiteReportPerc";
      buttonTaoSuiteReportPerc.Size = new System.Drawing.Size(24, 23);
      buttonTaoSuiteReportPerc.TabIndex = 0;
      buttonTaoSuiteReportPerc.Top = -1;
      buttonTaoSuiteReportPerc.Left = 414;
      buttonTaoSuiteReportPerc.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonTaoSuiteReportPerc.UseVisualStyleBackColor = true;
      buttonTaoSuiteReportPerc.Click += new EventHandler((sender, e) => button_ShowTaoSuiteHistory(sender, e, SeriesChartType.StackedColumn100, projectRootFolder, taoSheets, comboDbConnection.Items[comboDbConnection.SelectedIndex].ToString()));
      tabPageContent.Controls.Add(buttonTaoSuiteReportPerc);

      System.Windows.Forms.Button buttonTaoSuiteReportAct = new System.Windows.Forms.Button();
      buttonTaoSuiteReportAct.Image = global::taoGUI.Properties.Resources.Stats2;
      buttonTaoSuiteReportAct.Location = new System.Drawing.Point(441, -1);
      buttonTaoSuiteReportAct.Name = "buttonTaoSuiteReportAct";
      buttonTaoSuiteReportAct.Size = new System.Drawing.Size(24, 23);
      buttonTaoSuiteReportAct.TabIndex = 0;
      buttonTaoSuiteReportAct.Top = -1;
      buttonTaoSuiteReportAct.Left = 441;
      buttonTaoSuiteReportAct.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonTaoSuiteReportAct.UseVisualStyleBackColor = true;
      buttonTaoSuiteReportAct.Click += new EventHandler((sender, e) => button_ShowTaoSuiteHistory(sender, e, SeriesChartType.StackedArea, projectRootFolder, taoSheets, comboDbConnection.Items[comboDbConnection.SelectedIndex].ToString()));
      tabPageContent.Controls.Add(buttonTaoSuiteReportAct);

      System.Windows.Forms.Button buttonTaoSuiteReportPoll = new System.Windows.Forms.Button();
      buttonTaoSuiteReportPoll.Image = global::taoGUI.Properties.Resources.Poll;
      buttonTaoSuiteReportPoll.Location = new System.Drawing.Point(468, -1);
      buttonTaoSuiteReportPoll.Name = "buttonTaoSuiteReportPoll";
      buttonTaoSuiteReportPoll.Size = new System.Drawing.Size(24, 23);
      buttonTaoSuiteReportPoll.TabIndex = 0;
      buttonTaoSuiteReportPoll.Top = -1;
      buttonTaoSuiteReportPoll.Left = 468;
      buttonTaoSuiteReportPoll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonTaoSuiteReportPoll.UseVisualStyleBackColor = true;
      buttonTaoSuiteReportPoll.Click += new EventHandler((sender, e) => button_ShowTaoSuiteStatistics(sender, e, projectRootFolder, taoSheets, comboDbConnection.Items[comboDbConnection.SelectedIndex].ToString()));
      tabPageContent.Controls.Add(buttonTaoSuiteReportPoll);

      System.Windows.Forms.Button buttonTaoSuiteReportHist = new System.Windows.Forms.Button();
      buttonTaoSuiteReportHist.Image = global::taoGUI.Properties.Resources.Dots_Up;
      buttonTaoSuiteReportHist.Location = new System.Drawing.Point(495, -1);
      buttonTaoSuiteReportHist.Name = "buttonTaoSuiteReportHist";
      buttonTaoSuiteReportHist.Size = new System.Drawing.Size(24, 23);
      buttonTaoSuiteReportHist.TabIndex = 0;
      buttonTaoSuiteReportHist.Top = -1;
      buttonTaoSuiteReportHist.Left = 495;
      buttonTaoSuiteReportHist.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonTaoSuiteReportHist.UseVisualStyleBackColor = true;
      buttonTaoSuiteReportHist.Click += new EventHandler((sender, e) => button_ShowTaoSuiteHistogram(sender, e, projectRootFolder, taoSheets, comboDbConnection.Items[comboDbConnection.SelectedIndex].ToString()));
      tabPageContent.Controls.Add(buttonTaoSuiteReportHist);

      System.Windows.Forms.Button buttonOpenTaoSheet = new System.Windows.Forms.Button();
      buttonOpenTaoSheet.Image = global::taoGUI.Properties.Resources.Table;
      buttonOpenTaoSheet.Location = new System.Drawing.Point(524, -1);
      buttonOpenTaoSheet.Name = "buttonOpenTaoSheet";
      buttonOpenTaoSheet.Size = new System.Drawing.Size(24, 23);
      buttonOpenTaoSheet.TabIndex = 0;
      buttonOpenTaoSheet.Top = -1;
      buttonOpenTaoSheet.Left = 524;
      buttonOpenTaoSheet.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonOpenTaoSheet.UseVisualStyleBackColor = true;
      buttonOpenTaoSheet.Click += new EventHandler((sender, e) => button_OpenTaoSheet(sender, e));
      tabPageContent.Controls.Add(buttonOpenTaoSheet);

      System.Windows.Forms.Button buttonOpenTaoSuiteReport = new System.Windows.Forms.Button();
      buttonOpenTaoSuiteReport.Image = global::taoGUI.Properties.Resources.Document2;
      buttonOpenTaoSuiteReport.Location = new System.Drawing.Point(551, -1);
      buttonOpenTaoSuiteReport.Name = "buttonOpenTaoSuiteReport";
      buttonOpenTaoSuiteReport.Size = new System.Drawing.Size(24, 23);
      buttonOpenTaoSuiteReport.TabIndex = 0;
      buttonOpenTaoSuiteReport.Top = -1;
      buttonOpenTaoSuiteReport.Left = 551;
      buttonOpenTaoSuiteReport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonOpenTaoSuiteReport.UseVisualStyleBackColor = true;
      // buttonOpenTaoSuiteReport.Click += new EventHandler((sender, e) => button_OpenTaoSuiteReport(sender, e));
      tabPageContent.Controls.Add(buttonOpenTaoSuiteReport);

      System.Windows.Forms.Button buttonExportTaoSuiteSummary = new System.Windows.Forms.Button();
      buttonExportTaoSuiteSummary.Image = global::taoGUI.Properties.Resources.Go_Out;
      buttonExportTaoSuiteSummary.Location = new System.Drawing.Point(578, -1);
      buttonExportTaoSuiteSummary.Name = "buttonExportTaoSuiteSummary";
      buttonExportTaoSuiteSummary.Size = new System.Drawing.Size(24, 23);
      buttonExportTaoSuiteSummary.TabIndex = 0;
      buttonExportTaoSuiteSummary.Top = -1;
      buttonExportTaoSuiteSummary.Left = 578;
      buttonExportTaoSuiteSummary.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonExportTaoSuiteSummary.UseVisualStyleBackColor = true;
      // buttonExportTaoSuiteSummary.Click += new EventHandler((sender, e) => button_ExportTaoSuiteSummary(sender, e));
      tabPageContent.Controls.Add(buttonExportTaoSuiteSummary);

      // Finally set up button tool-tips...
      ToolTip toolTip1 = new ToolTip();
      // Set up the delays for the ToolTip.
      toolTip1.AutoPopDelay = 5000;
      toolTip1.InitialDelay = 1000;
      toolTip1.ReshowDelay = 500;
      // Force the ToolTip text to be displayed whether or not the form is active.
      toolTip1.ShowAlways = true;
      // Set up the ToolTip text for the Combobox and Buttons.
      toolTip1.SetToolTip(comboDbConnection, "Select the database instance relevant to the Tao Suite Reports");
      toolTip1.SetToolTip(comboDgvFilter, "Filter Tao Suite Reports");
      toolTip1.SetToolTip(buttonTaoSuiteReportPerc, "Display the history of pass rates (as a stacked-column percentage chart).");
      toolTip1.SetToolTip(buttonTaoSuiteReportAct, "Display the history of pass rates (as stacked area chart)");
      toolTip1.SetToolTip(buttonTaoSuiteReportPoll, "Display pass rates, with mean, standard deviation and volatiltiy bands");
      toolTip1.SetToolTip(buttonTaoSuiteReportHist, "Display pass rates histogram (about the pass rate mean) with standard deviation");
      toolTip1.SetToolTip(buttonOpenTaoSheet, "Open Tao Suite");
      toolTip1.SetToolTip(buttonOpenTaoSuiteReport, "Open last known Tao Suite Report (latest test results)");
      toolTip1.SetToolTip(buttonExportTaoSuiteSummary, "Export the Tao Suite Reports view to Microsoft Excel");
    }

    private void addTabContent(string appId, string tabReportName, TabPage tabPageContent) {
      switch (tabReportName) {
        case "Tao Suite Reports":
          addTabContentTaoSuiteReports(appId, tabPageContent);
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
