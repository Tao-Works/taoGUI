﻿using System;
using System.Globalization;
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
using Excel = Microsoft.Office.Interop.Excel;

namespace taoGUI {
  public partial class Form1 : Form {

    public static string TAO_PROJECT_FILE = Application.StartupPath + @"\taoGUI.resources\projects.tao";
    public static string TAO_DIMENSIONS_FILE = Application.StartupPath + @"\taoGUI.resources\dimensions.tao";

    public struct DimensionAttributeMap {
      public string dimension { get; set; }
      public List<string> dimensionAttributes { get; set; }
    }

    public struct TaoSuiteDimensionMap {
      public string taoSuiteName { get; set; }
      public List<DimensionAttributeMap> taoGroupByAttributes { get; set; }
    }

    // Used for the drop-down ComboBoxes where "label" != "value" ...
    public class ComboBoxItem {
      public string Text { get; set; }
      public object Value { get; set; }
      public override string ToString() {
        return Text.ToString();
      }
    }

    // Represents the "chart" cell of a Summary of Done matrix
    public class TaoSummaryChartMatrixCell {
      public string chartTitle { get; set; }
      public double testsPass { get; set; }
      public double totalTests { get; set; }
      public string rowDimension { get; set; }
      public string rowAttribute { get; set; }
      public string colDimension { get; set; }
      public string colAttribute { get; set; }
      public string ctxDimension { get; set; }
      public string ctxAttribute { get; set; }
      public List<string> relatedTaoSuites = new List<string>();

      private void tagRelevantTaoSuites(List<TaoSuiteDimensionMap> userMap) {

        List<string> xDimension = new List<string>();
        List<string> yDimension = new List<string>();
        List<string> zDimension = new List<string>();

        foreach (TaoSuiteDimensionMap userDim in userMap) {

          xDimension.Clear();
          yDimension.Clear();
          zDimension.Clear();

          foreach (DimensionAttributeMap userAttr in userDim.taoGroupByAttributes) {

            bool xValid = false;
            bool yValid = false;
            bool zValid = false;

            // Row dimension / attributes...
            if (String.IsNullOrEmpty(rowDimension)) {
              xValid = true;
            } else if (userAttr.dimension.Equals(rowDimension)) {
              if (String.IsNullOrEmpty(rowAttribute)) {
                xValid = true;
              } else if (userAttr.dimensionAttributes.Contains(rowAttribute)) {
                xValid = true;
              }
            }

            // Column dimension / attributes...
            if (String.IsNullOrEmpty(colDimension)) {
              yValid = true;
            } else if (userAttr.dimension.Equals(colDimension)) {
              if (String.IsNullOrEmpty(colAttribute)) {
                yValid = true;
              } else if (userAttr.dimensionAttributes.Contains(colAttribute)) {
                yValid = true;
              }
            }

            // Context dimension / attributes...
            if (String.IsNullOrEmpty(ctxDimension)) {
              zValid = true;
            } else if (userAttr.dimension.Equals(ctxDimension)) {
              if (String.IsNullOrEmpty(ctxAttribute)) {
                zValid = true;
              } else if (userAttr.dimensionAttributes.Contains(ctxAttribute)) {
                zValid = true;
              }
            }

            // Tag?
            if (xValid && !xDimension.Contains(userDim.taoSuiteName)) {
              xDimension.Add(userDim.taoSuiteName);
            }
            if (yValid && !yDimension.Contains(userDim.taoSuiteName)) {
              yDimension.Add(userDim.taoSuiteName);
            }
            if (zValid && !zDimension.Contains(userDim.taoSuiteName)) {
              zDimension.Add(userDim.taoSuiteName);
            }

          }

          // Tao Suite crosses all dimensions -- therefore tag as "related" Tao Suite (for statistics)
          if (xDimension.Contains(userDim.taoSuiteName) && yDimension.Contains(userDim.taoSuiteName) && zDimension.Contains(userDim.taoSuiteName)) {
            if (!relatedTaoSuites.Contains(userDim.taoSuiteName)) {
              relatedTaoSuites.Add(userDim.taoSuiteName);
            }
          }

        }
      }

      public void setMatrixCell(List<string> dim1Tokens, List<string> dim2Tokens, List<string> dim3Tokens, string rowText, string colText, List<TaoSuiteDimensionMap> userMap) {

        StringBuilder strapLine = new StringBuilder();

        // 2 = row; 1 = column;
        switch (dim2Tokens[0] + dim1Tokens[0]) {
          case "00":
            rowDimension = String.Empty;
            rowAttribute = String.Empty;
            colDimension = String.Empty;
            colAttribute = String.Empty;
            strapLine.Append(rowText);
            break;
          case "01":
          case "02":
            rowDimension = String.Empty;
            rowAttribute = String.Empty;
            colDimension = dim1Tokens[1];
            colAttribute = colText;
            strapLine.Append(colText);
            break;
          case "10":
          case "20":
            rowDimension = dim2Tokens[1];
            rowAttribute = rowText;
            colDimension = String.Empty;
            colAttribute = String.Empty;
            strapLine.Append(rowText);
            break;
          case "11":
          case "12":
          case "21":
          case "22":
            rowDimension = dim2Tokens[1];
            rowAttribute = rowText;
            colDimension = dim1Tokens[1];
            colAttribute = colText;
            if (rowText.Equals(colText)) {
              strapLine.Append(rowText);
            } else {
              strapLine.Append(rowText + " / " + colText);
            }
            break;
          default:
            break;
        }

        // 3 = context;
        switch (dim3Tokens[0]) {
          case "0":
            ctxDimension = String.Empty;
            ctxAttribute = String.Empty;
            break;
          case "1":
            strapLine.Append(" (" + dim3Tokens[1] + ")");
            ctxDimension = dim3Tokens[1];
            ctxAttribute = String.Empty;
            break;
          case "2":
            ctxDimension = dim3Tokens[1];
            ctxAttribute = dim3Tokens[2];
            strapLine.Append(" (" + dim3Tokens[2] + ")");
            break;
          default:
            break;
        }

        chartTitle = strapLine.ToString();

        testsPass = 0.0;
        totalTests = 0.0;
        relatedTaoSuites = new List<string>();

        // Populate (X,Y,Z) cell location with all applicable Tao Suites...
        tagRelevantTaoSuites(userMap);

      }

    }

    // Represents a "column" of matrix cells
    public class TaoSummaryChartMatrixColumn {
      public string rowId = String.Empty;
      public List<TaoSummaryChartMatrixCell> matrixColumns = new List<TaoSummaryChartMatrixCell>();
    }

    // Represents the chart matrix as "rows" of "matrix columns" and statistic data
    public class TaoSummaryChartMatrix {

      public string matrixId = String.Empty;
      public DataTable summaryOfDoneDataTable;
      public List<TaoSummaryChartMatrixColumn> matrixRows = new List<TaoSummaryChartMatrixColumn>();

      public void setSummaryOfDoneCache(string projectRootFolder, string appId, string dbInstance) {
        TaoReportCache tmpCacheStatus = new TaoReportCache(projectRootFolder, appId, dbInstance);
        if (!tmpCacheStatus.isCacheCurrent()) {
          MessageBox.Show("The statistics for the Tao Suite Reports need re-calculating.  This will take a short while (please be patient).", "Re-calculating Statistics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          tmpCacheStatus.updateCacheResults();
        }
        summaryOfDoneDataTable = tmpCacheStatus.getCacheDataTable();
      }

    }

    private TabControl tabCtrlAppStatus;                                            // This is the main "tab control" container for status reports
    private List<TabPage> appStatusTabPages = new List<TabPage>();                  // This is the tab pages, each represent one specific Tao App.
    private List<TabControl> statusReportsTabControl = new List<TabControl>();      // This is a sub "tab control" contained within each Tao App.
    private List<TabPage> statusReportsTabPages = new List<TabPage>();              // This is the sub tab pages per report per Tao Application.
    private List<ComboBox> comboDimensionFilter = new List<ComboBox>();             // This is the group-by dimensions for the summary tabs.

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
          l1_node.Nodes.Add(applicationId + "|status|summary", "Summary of Done");
          l1_node.Nodes.Add(applicationId + "|status|velocity", "Velocity of Alignment");
          l1_node.Nodes.Add(applicationId + "|status|stability", "Tao Application Stability");
          prjNode.Nodes.Add(applicationId + "|tao", "Tao Sheets");
          TreeNode fileStrctNode = prjNode.Nodes.Add(applicationId + "|file", "File Structure");
          string appFolderName = ctx.getAppFolder();
          if (System.IO.Directory.Exists(appFolderName)) {
            var di = new DirectoryInfo(appFolderName);
            walkDirectoryStructure(di, fileStrctNode, applicationId + "|file");
          }
        }
      }
    }

    public void addProjectFile(string appName, string appDesc, string appFolder) {
      string fileLocation = TAO_PROJECT_FILE;
      if (!File.Exists(fileLocation)) {
        // Create a file to write to.
        using (StreamWriter sw = File.CreateText(fileLocation)) {
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
        var lines = File.ReadAllLines(fileLocation);
        File.WriteAllLines(fileLocation, lines.Take(lines.Length - 3).ToArray());

        // This text is always added, making the file longer over time (if it is not deleted).
        using (StreamWriter sw = File.AppendText(fileLocation)) {
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
      using (var sw = new StreamWriter(tempFile)) {
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
      File.Delete(fileLocation);
      File.Move(tempFile, fileLocation);
    }

    private void alwaysOn_DrawNode(object sender, DrawTreeNodeEventArgs e) {
      if (e.Node != null) {
        // if treeview's HideSelection property is "True", 
        // this will always returns "False" on unfocused treeview
        var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
        var unfocused = !e.Node.TreeView.Focused;

        // we need to do owner drawing only on a selected node
        // and when the treeview is unfocused, else let the OS do it for us
        if (selected && unfocused) {
          var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
          e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
          TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
        } else {
          e.DrawDefault = true;
        }
      }
    }

    public Form1() {
      InitializeComponent();
      showProjectsInTreeView();
      taoProjectView.DrawMode = TreeViewDrawMode.OwnerDrawText;
      taoProjectView.HideSelection = false;
      taoProjectView.DrawNode += new DrawTreeNodeEventHandler(alwaysOn_DrawNode);
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
          FileInfo keyInfo = new FileInfo(importLicenceKey);
          FileInfo textInfo = new FileInfo(importLicenceText);
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
        showStatusReprtTabPage(appId, "Summary of Done", "|status|summary");
      }
    }

    private void summaryToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Weather - Current", "|weather|actual");
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
          var totalTabForm = appStatusTabPages[totalTabs];
          tabCtrlAppStatus.Controls.Add(totalTabForm);
          totalTabForm.Location = new System.Drawing.Point(4, 4);
          totalTabForm.Name = "appStatusTabPages_" + totalTabs.ToString();
          totalTabForm.Padding = new System.Windows.Forms.Padding(3);
          totalTabForm.Size = new System.Drawing.Size(528, 335);
          totalTabForm.TabIndex = totalTabs;
          totalTabForm.Text = appId;
          totalTabForm.UseVisualStyleBackColor = true;
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

    private void button_ExportTaoSuiteSummary(object sender, EventArgs e, DataGridView taoSheets) {

      Excel.Application xlApp = null;
      xlApp = new Excel.Application();
      xlApp.ScreenUpdating = true;
      xlApp.Visible = true;

      Excel.Workbook xlWorkbook = xlApp.Workbooks.Add();
      xlWorkbook.Sheets[1].Name = "Tao Suite Reports";

      // Setup column widths
      xlWorkbook.Sheets[1].Columns["A:A"].ColumnWidth = 2.14;     // approx  20 pixcels
      xlWorkbook.Sheets[1].Columns["B:B"].ColumnWidth = 67.14;    // approx 480 pixcels
      xlWorkbook.Sheets[1].Columns["C:L"].ColumnWidth = 17.14;    // approx 125 pixcels

      // Setup report headers
      xlWorkbook.Sheets[1].Range("B2").Value = "Tao Suite";
      xlWorkbook.Sheets[1].Range("C2").Value = "First Run";
      xlWorkbook.Sheets[1].Range("D2").Value = "Last Run";
      xlWorkbook.Sheets[1].Range("E2").Value = "Iterations";
      xlWorkbook.Sheets[1].Range("F2").Value = "Pass Rate";
      xlWorkbook.Sheets[1].Range("G2").Value = "Delta";
      xlWorkbook.Sheets[1].Range("H2").Value = "Mean";
      xlWorkbook.Sheets[1].Range("I2").Value = "Std. Dev.";
      xlWorkbook.Sheets[1].Range("J2").Value = "Lower Band";
      xlWorkbook.Sheets[1].Range("K2").Value = "Upper Band";
      xlWorkbook.Sheets[1].Range("L2").Value = "Volatility";

      // Format the header
      xlWorkbook.Sheets[1].Range("B2:L2").Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(204, 204, 255));  // dull blue
      xlWorkbook.Sheets[1].Range("B2:L2").Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
      xlWorkbook.Sheets[1].Range("B2:L2").Borders.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(64, 64, 64));      // dark grey
      xlWorkbook.Sheets[1].Range("B2:L2").Borders.Weight = Excel.XlBorderWeight.xlThin;
      xlWorkbook.Sheets[1].Range("B2:L2").Font.Bold = true;
      xlWorkbook.Sheets[1].Range("B2:L2").IndentLevel = 1;

      // Export the data
      int excelRowIndex = 3;
      foreach (DataGridViewRow row in taoSheets.Rows) {
        xlWorkbook.Sheets[1].Range("B" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[0]).Value;
        xlWorkbook.Sheets[1].Range("C" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[1]).Value;
        xlWorkbook.Sheets[1].Range("D" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[2]).Value;
        xlWorkbook.Sheets[1].Range("E" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[3]).Value;
        xlWorkbook.Sheets[1].Range("F" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[4]).Value;
        if (0.0 < Convert.ToDouble(((DataGridViewCell)row.Cells[4]).Value.ToString()) && Convert.ToDouble(((DataGridViewCell)row.Cells[4]).Value.ToString()) < 100.0) {
          xlWorkbook.Sheets[1].Range("F" + excelRowIndex.ToString()).Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LemonChiffon);
        }
        xlWorkbook.Sheets[1].Range("G" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[5]).Value;
        if (Convert.ToDouble(((DataGridViewCell)row.Cells[5]).Value.ToString()) < 0.0) {
          xlWorkbook.Sheets[1].Range("G" + excelRowIndex.ToString()).Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightPink);
        }
        if (Convert.ToDouble(((DataGridViewCell)row.Cells[5]).Value.ToString()) > 0.0) {
          xlWorkbook.Sheets[1].Range("G" + excelRowIndex.ToString()).Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGreen);
        }
        xlWorkbook.Sheets[1].Range("H" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[6]).Value;
        xlWorkbook.Sheets[1].Range("I" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[7]).Value;
        xlWorkbook.Sheets[1].Range("J" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[8]).Value;
        xlWorkbook.Sheets[1].Range("K" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[9]).Value;
        xlWorkbook.Sheets[1].Range("L" + excelRowIndex.ToString()).Value = ((DataGridViewCell)row.Cells[10]).Value;
        excelRowIndex++;
      }
      if (excelRowIndex > 3) {
        excelRowIndex--;
      }

      // Format exported cells
      xlWorkbook.Sheets[1].Range("B3:L" + excelRowIndex.ToString()).Borders.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
      xlWorkbook.Sheets[1].Range("C3:D" + excelRowIndex.ToString()).Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
      xlWorkbook.Sheets[1].Range("E3:E" + excelRowIndex.ToString()).NumberFormat = @"_(* #,##0_);_(* -#,##0_);_(* "" - ""??_);_(@_)";
      xlWorkbook.Sheets[1].Range("F3:L" + excelRowIndex.ToString()).NumberFormat = @"_(* #,##0.0000_);_(* -#,##0.0000_);_(* "" - ""??_);_(@_)";

      // Freeze panes
      xlApp.ActiveWindow.SplitRow = 2;
      xlApp.ActiveWindow.FreezePanes = true;
      Excel.Range firstRow = (Excel.Range)xlWorkbook.Sheets[1].Rows[2];
      firstRow.AutoFilter(1, Type.Missing, Excel.XlAutoFilterOperator.xlAnd, Type.Missing, true);

      // Now zoom out
      xlApp.ActiveWindow.Zoom = 80;

    }

    private void button_OpenTaoSuiteReport(object sender, EventArgs e, string projectRootFolder, DataGridView taoSheetData, string dbInstance) {
      Excel.Application xlApp = null;
      Excel.Workbook xlWorkbook = null;

      xlApp = new Excel.Application();
      xlApp.ScreenUpdating = true;
      xlApp.Visible = true;

      string taoReportFolders = projectRootFolder + @"\taoSuite_Report\"; // TODO: I am missing an object that takes care of these values...
      List<string> taoSheets = new List<string>();
      string taoSheet = string.Empty;
      string timeOfLastRun = string.Empty;

      DataGridViewSelectedRowCollection rows = taoSheetData.SelectedRows;
      taoSheets.Clear();

      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          // TODO: Extend the data grid view to include "hidden" attributes: Tao Suite filename and latest Tao Suite Report filename.
          //       This would prevent us having to re-construct (possibly with false extension) the filenames...
          timeOfLastRun = myRow.ItemArray[2].ToString();
          timeOfLastRun = timeOfLastRun.Replace(" ", "_").Replace(":", "");
          taoSheet = myRow.ItemArray[0].ToString();
          taoSheet = taoReportFolders + taoSheet.Substring(0, taoSheet.IndexOf(".")) + "." + timeOfLastRun + "." + dbInstance + ".xls"; // TODO: Refactor this assumption...
          if (!taoSheets.Contains(taoSheet)) {
            taoSheets.Add(taoSheet);
          }
        }
      } else {
        Int32 selectedCellCount = taoSheetData.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = taoSheetData.SelectedCells[i].RowIndex;
            timeOfLastRun = taoSheetData.Rows[rowIndex].Cells[2].Value.ToString();
            timeOfLastRun = timeOfLastRun.Replace(" ", "_").Replace(":", "");
            taoSheet = taoSheetData.Rows[rowIndex].Cells[0].Value.ToString();
            taoSheet = taoReportFolders + taoSheet.Substring(0, taoSheet.IndexOf(".")) + "." + timeOfLastRun + "." + dbInstance + ".xls"; // TODO: Refactor this assumption...
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
            }
          }
        } else {
          MessageBox.Show("No Tao Suite Report was selected for opening.", "Pass Rate History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
      foreach (string ts in taoSheets) {
        if (File.Exists(ts)) {
          xlWorkbook = xlApp.Workbooks.Open(ts, true, false); // Update links (e.g. parameter files) and allow user to read and write as necessary
        }
      }
    }

    private void button_OpenTaoSheet(object sender, EventArgs e, string projectRootFolder, DataGridView taoSheetData) {

      Excel.Application xlApp = null;
      Excel.Workbook xlWorkbook = null;

      xlApp = new Excel.Application();
      xlApp.ScreenUpdating = true;
      xlApp.Visible = true;

      string taoInputFolders = projectRootFolder + @"\taoSuite_Input\"; // TODO: I am missing an object that takes care of these values...
      List<string> taoSheets = new List<string>();
      string taoSheet = string.Empty;

      DataGridViewSelectedRowCollection rows = taoSheetData.SelectedRows;
      taoSheets.Clear();

      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          taoSheet = taoInputFolders + myRow.ItemArray[0].ToString();
          if (!taoSheets.Contains(taoSheet)) {
            taoSheets.Add(taoSheet);
          }
        }
      } else {
        Int32 selectedCellCount = taoSheetData.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = taoSheetData.SelectedCells[i].RowIndex;
            taoSheet = taoInputFolders + taoSheetData.Rows[rowIndex].Cells[0].Value.ToString();
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
            }
          }
        } else {
          MessageBox.Show("No Tao Suite was selected for opening.", "Pass Rate History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
      foreach (string ts in taoSheets) {
        if (File.Exists(ts)) {
          xlWorkbook = xlApp.Workbooks.Open(ts, true, false); // Update links (e.g. parameter files) and allow user to read and write as necessary
        }
      }
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
      buttonOpenTaoSheet.Click += new EventHandler((sender, e) => button_OpenTaoSheet(sender, e, projectRootFolder, taoSheets));
      tabPageContent.Controls.Add(buttonOpenTaoSheet);

      System.Windows.Forms.Button buttonOpenTaoSuiteReport = new System.Windows.Forms.Button();
      buttonOpenTaoSuiteReport.Image = global::taoGUI.Properties.Resources.Ok;
      buttonOpenTaoSuiteReport.Location = new System.Drawing.Point(551, -1);
      buttonOpenTaoSuiteReport.Name = "buttonOpenTaoSuiteReport";
      buttonOpenTaoSuiteReport.Size = new System.Drawing.Size(24, 23);
      buttonOpenTaoSuiteReport.TabIndex = 0;
      buttonOpenTaoSuiteReport.Top = -1;
      buttonOpenTaoSuiteReport.Left = 551;
      buttonOpenTaoSuiteReport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonOpenTaoSuiteReport.UseVisualStyleBackColor = true;
      buttonOpenTaoSuiteReport.Click += new EventHandler((sender, e) => button_OpenTaoSuiteReport(sender, e, projectRootFolder, taoSheets, comboDbConnection.Items[comboDbConnection.SelectedIndex].ToString()));
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
      buttonExportTaoSuiteSummary.Click += new EventHandler((sender, e) => button_ExportTaoSuiteSummary(sender, e, taoSheets));
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

    private void button_SetSummaryDimensions(object sender, EventArgs e, string projectRootFolder, string appId) {
      formGroupByDimensions newProj = new formGroupByDimensions(this, projectRootFolder, appId); // Passing master form to trigger master-form refresh after sub-form commit
      newProj.ShowDialog();
    }

    public void refreshGroupByDimensions() {
      int comboIndex = 1;
      foreach (ComboBox comboDimFilter in comboDimensionFilter) {
        comboDimFilter.Items.Clear();

        ComboBoxItem dimAttribute = new ComboBoxItem();
        dimAttribute.Text = "Dimension " + comboIndex.ToString();
        dimAttribute.Value = "0";
        comboDimFilter.Items.Add(dimAttribute);

        if (File.Exists(TAO_DIMENSIONS_FILE)) {
          Dictionary<string, TaoJsonGroupByDimension> userDimensions = TaoJsonConfigReader.getTaoGroupByDimensionMap(TAO_DIMENSIONS_FILE);
          foreach (TaoJsonGroupByDimension userDimension in userDimensions.Values) {

            dimAttribute = new ComboBoxItem();
            dimAttribute.Text = String.Empty;
            dimAttribute.Value = "0";
            comboDimFilter.Items.Add(dimAttribute);

            dimAttribute = new ComboBoxItem();
            dimAttribute.Text = userDimension.dimension.ToString();
            dimAttribute.Value = "1|" + userDimension.dimension.ToString();
            comboDimFilter.Items.Add(dimAttribute);

            List<string> attributes = userDimension.attributes;
            foreach (string attr in attributes) {

              dimAttribute = new ComboBoxItem();
              dimAttribute.Text = "     " + attr.ToString();
              dimAttribute.Value = "2|" + userDimension.dimension.ToString() + "|" + attr.ToString();
              comboDimFilter.Items.Add(dimAttribute);

            }
          }
        }
        comboDimFilter.SelectedIndex = 0;
        comboIndex++;
        if (comboIndex > 3) {
          comboIndex = 1;
        }
      }
    }

    private void buildSummaryMatrix(string projectRootFolder, string appId, string dbInstance, string dim1Filter, string dim2Filter, string dim3Filter, Panel pieChartPanel) {

      string matrixName = "chartSummaryOfDoneMatrix." + appId + "." + dbInstance;
      string dimensionsLocation = Application.StartupPath + @"\taoGUI.resources\dimensions.tao";
      string dimensionsMapLocation = Application.StartupPath + @"\taoGUI.resources\dim_" + appId + ".tao";
      List<TaoJsonGroupByDimension> groupByDimensions = new List<TaoJsonGroupByDimension>();
      List<TaoSuiteDimensionMap> userTaoSuiteDimensionMap = new List<TaoSuiteDimensionMap>();
      List<string> dim1Tokens = dim1Filter.Split('|').ToList<string>();
      List<string> dim2Tokens = dim2Filter.Split('|').ToList<string>();
      List<string> dim3Tokens = dim3Filter.Split('|').ToList<string>();

      // Get all dimensions
      if (File.Exists(dimensionsLocation)) {
        Dictionary<string, TaoJsonGroupByDimension> userDimensions = TaoJsonConfigReader.getTaoGroupByDimensionMap(dimensionsLocation);
        foreach (TaoJsonGroupByDimension userDimension in userDimensions.Values) {
          groupByDimensions.Add(userDimension); // use this later for validation (e.g. prevent duplication) and persist to JSON file
        }
      }

      // Get the user defined mappings of Tao Suites to group-by dimensions
      if (File.Exists(dimensionsMapLocation)) {
        Dictionary<string, TaoJsonTaoSuiteDimensionMap> tmpDimension = TaoJsonConfigReader.getTaoSuiteDimensionMap(dimensionsMapLocation);
        foreach (TaoJsonTaoSuiteDimensionMap userDimension in tmpDimension.Values) {
          TaoSuiteDimensionMap tmpDimensionMap = new TaoSuiteDimensionMap();
          tmpDimensionMap.taoSuiteName = userDimension.taoSuiteName;
          tmpDimensionMap.taoGroupByAttributes = new List<DimensionAttributeMap>();
          foreach (TaoJsonGroupByDimension tmpMap in userDimension.groupByDimensions) {
            DimensionAttributeMap tmpDim = new DimensionAttributeMap();
            tmpDim.dimension = tmpMap.dimension;
            tmpDim.dimensionAttributes = new List<string>();
            foreach (string tmpAttr in tmpMap.attributes) {
              tmpDim.dimensionAttributes.Add(tmpAttr);
            }
            tmpDimensionMap.taoGroupByAttributes.Add(tmpDim);
          }
          userTaoSuiteDimensionMap.Add(tmpDimensionMap);
        }
      }

      // Create the matrix...
      TaoSummaryChartMatrix tmpMatrix = new TaoSummaryChartMatrix();
      tmpMatrix.matrixId = matrixName;
      tmpMatrix.matrixRows = new List<TaoSummaryChartMatrixColumn>();

      // Set up the statistics...
      tmpMatrix.summaryOfDoneDataTable = new DataTable();
      tmpMatrix.setSummaryOfDoneCache(projectRootFolder, appId, dbInstance);

      // Build matrix...
      if (dim2Tokens[0].Equals("0")) {

        TaoSummaryChartMatrixColumn tmpRow = new TaoSummaryChartMatrixColumn();
        tmpRow.rowId = "All Tao Suites";
        tmpRow.matrixColumns = new List<TaoSummaryChartMatrixCell>();

        if (dim1Tokens[0].Equals("0")) {

          TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
          tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, "All Tao Suites", String.Empty, userTaoSuiteDimensionMap);
          tmpRow.matrixColumns.Add(tmpCell);

        } else if (dim1Tokens[0].Equals("1")) {

          var idxDim = groupByDimensions.FirstOrDefault(x => x.dimension.Equals(dim1Tokens[1]));
          if (!String.IsNullOrEmpty(idxDim.dimension)) {
            foreach (string idxCol in idxDim.attributes) {
              TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
              tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, "All Tao Suites", idxCol, userTaoSuiteDimensionMap);
              tmpRow.matrixColumns.Add(tmpCell);
            }
          }

        } else if (dim1Tokens[0].Equals("2")) {

          TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
          tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, "All Tao Suites", dim1Tokens[2], userTaoSuiteDimensionMap);
          tmpRow.matrixColumns.Add(tmpCell);

        }
        tmpMatrix.matrixRows.Add(tmpRow);

      } else if (dim2Tokens[0].Equals("1")) {
        var idxDimension = groupByDimensions.FirstOrDefault(x => x.dimension.Equals(dim2Tokens[1]));
        if (!String.IsNullOrEmpty(idxDimension.dimension)) {
          foreach (string attr in idxDimension.attributes) {
            TaoSummaryChartMatrixColumn tmpRow = new TaoSummaryChartMatrixColumn();
            tmpRow.rowId = attr;
            tmpRow.matrixColumns = new List<TaoSummaryChartMatrixCell>();

            if (dim1Tokens[0].Equals("0")) {

              TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
              tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, attr, String.Empty, userTaoSuiteDimensionMap);
              tmpRow.matrixColumns.Add(tmpCell);

            } else if (dim1Tokens[0].Equals("1")) {

              var idxDim = groupByDimensions.FirstOrDefault(x => x.dimension.Equals(dim1Tokens[1]));
              if (!String.IsNullOrEmpty(idxDim.dimension)) {
                foreach (string idxCol in idxDim.attributes) {
                  TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
                  tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, attr, idxCol, userTaoSuiteDimensionMap);
                  tmpRow.matrixColumns.Add(tmpCell);
                }
              }

            } else if (dim1Tokens[0].Equals("2")) {

              TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
              tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, attr, dim1Tokens[2], userTaoSuiteDimensionMap);
              tmpRow.matrixColumns.Add(tmpCell);

            }
            tmpMatrix.matrixRows.Add(tmpRow);

          }
        }
      } else if (dim2Tokens[0].Equals("2")) {
        TaoSummaryChartMatrixColumn tmpRow = new TaoSummaryChartMatrixColumn();
        tmpRow.rowId = dim2Tokens[2];
        tmpRow.matrixColumns = new List<TaoSummaryChartMatrixCell>();

        if (dim1Tokens[0].Equals("0")) {

          TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
          tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, dim2Tokens[2], String.Empty, userTaoSuiteDimensionMap);
          tmpRow.matrixColumns.Add(tmpCell);

        } else if (dim1Tokens[0].Equals("1")) {

          var idxDim = groupByDimensions.FirstOrDefault(x => x.dimension.Equals(dim1Tokens[1]));
          if (!String.IsNullOrEmpty(idxDim.dimension)) {
            foreach (string idxCol in idxDim.attributes) {
              TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
              tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, dim2Tokens[2], idxCol, userTaoSuiteDimensionMap);
              tmpRow.matrixColumns.Add(tmpCell);
            }
          }

        } else if (dim1Tokens[0].Equals("2")) {

          TaoSummaryChartMatrixCell tmpCell = new TaoSummaryChartMatrixCell();
          tmpCell.setMatrixCell(dim1Tokens, dim2Tokens, dim3Tokens, dim2Tokens[2], dim1Tokens[2], userTaoSuiteDimensionMap);
          tmpRow.matrixColumns.Add(tmpCell);

        }

        tmpMatrix.matrixRows.Add(tmpRow);
      }

      // Group the statistics by dimension attributes using data table
      foreach (TaoSummaryChartMatrixColumn rowCell in tmpMatrix.matrixRows) {
        foreach (TaoSummaryChartMatrixCell colCell in rowCell.matrixColumns) {
          foreach (string tao in colCell.relatedTaoSuites) {
            DataRow[] foundRows = tmpMatrix.summaryOfDoneDataTable.Select("taoSuiteName = '" + tao + "'");
            if (foundRows.Length == 1) {
              colCell.testsPass += Convert.ToDouble(foundRows[0]["passRate"].ToString()); // Assume this means number range 0 to 100 ... not 0 to 1.
              colCell.totalTests += 100.0;
            }
          }
        }
      }

      // Find all previous charts (provided each chart has the same name as matrix...)
      var oldCharts = pieChartPanel.Controls.Find("pieChart", true);
      foreach (Chart oldChart in oldCharts) {
        pieChartPanel.Controls.Remove(oldChart);
      }

      // Create new charts (with 4 pixcel margin off-set) ...
      int yCoord = 4;
      foreach (TaoSummaryChartMatrixColumn rowCell in tmpMatrix.matrixRows) {
        int xCoord = 4;
        foreach (TaoSummaryChartMatrixCell colCell in rowCell.matrixColumns) {

          Chart pieChart = new Chart();
          pieChart.Name = "pieChart";
          pieChart.Location = new System.Drawing.Point(xCoord, yCoord);
          pieChart.Series.Clear();
          pieChart.Palette = ChartColorPalette.SemiTransparent;
          pieChart.BackColor = Color.FromArgb(132, System.Drawing.Color.GhostWhite);
          pieChart.Titles.Add(colCell.chartTitle);
          Font legendTitleFont = new Font(pieChart.Titles[0].Font, FontStyle.Regular);
          NumberFormatInfo nfi = CultureInfo.CreateSpecificCulture("en-US").NumberFormat;
          Legend pieChartLegend = new Legend() { BackColor = System.Drawing.Color.White, ForeColor = Color.Black, Title = "Tao Suites: " + (colCell.totalTests/100d).ToString("N0",nfi), TitleFont = legendTitleFont };
          pieChart.Legends.Add(pieChartLegend);

          ChartArea pieChartArea = new ChartArea();
          pieChart.ChartAreas.Add(pieChartArea);
          pieChart.ChartAreas[0].BackColor = Color.Transparent;

          Series pieChartSeries = new Series {
            IsVisibleInLegend = true,
            Color = System.Drawing.Color.Green,
            ChartType = SeriesChartType.Pie
          };

          pieChartSeries.Points.Add(colCell.totalTests - colCell.testsPass); // Fail
          pieChartSeries.Points.Add(colCell.testsPass);                      // Pass

          pieChartSeries.Points[0].AxisLabel = (100d * (colCell.totalTests - colCell.testsPass) / colCell.totalTests).ToString("0.00") + "%";
          pieChartSeries.Points[0].LegendText = "Fail";
          pieChartSeries.Points[1].AxisLabel = (100d * colCell.testsPass / colCell.totalTests).ToString("0.00") + "%";
          pieChartSeries.Points[1].LegendText = "Pass";

          pieChart.Series.Add(pieChartSeries);
          pieChart.Invalidate();

          pieChartPanel.Controls.Add(pieChart);

          xCoord += 304;

        }
        yCoord += 304;
      }

    }

    private void changeDimensionFilter(object sender, EventArgs e, string projectRootFolder, string appId, string dbInstance, string dim1Filter, string dim2Filter, string dim3Filter, Panel pieChartPanel) {
      buildSummaryMatrix(projectRootFolder, appId, dbInstance, dim1Filter, dim2Filter, dim3Filter, pieChartPanel);
    }

    private void changeDbConnectionSummaryOfDone(object sender, EventArgs e, string projectRootFolder, string appId, string dbInstance, string dim1Filter, string dim2Filter, string dim3Filter, Panel pieChartPanel) {
      buildSummaryMatrix(projectRootFolder, appId, dbInstance, dim1Filter, dim2Filter, dim3Filter, pieChartPanel);
    }

    private void addTabContentSummary(string appId, TabPage tabPageContent) {

      string projectRootFolder = getProjectFolderName(appId) + @"\" + appId;

      // Create a "ribbon" effect for various control items (like filters and search)
      tabPageContent.Padding = new Padding(0, 24, 0, 0);

      // Create a database selector so that users can switch between instances and compare results
      System.Windows.Forms.ComboBox comboDbConnSummary = new System.Windows.Forms.ComboBox();
      comboDbConnSummary.FormattingEnabled = true;
      comboDbConnSummary.Location = new System.Drawing.Point(4, 28);
      comboDbConnSummary.Name = "comboDbConnSummary." + appId;
      comboDbConnSummary.Size = new System.Drawing.Size(200, 21);
      comboDbConnSummary.TabIndex = 0;
      comboDbConnSummary.Top = 0;
      comboDbConnSummary.Left = 0;
      comboDbConnSummary.Items.Add("All Database Connections");
      comboDbConnSummary.SelectedIndex = 0;
      // Read the connection files in the project config area
      string dirLocOfDbConnection = projectRootFolder + @"\conf";
      if (System.IO.Directory.Exists(dirLocOfDbConnection)) {
        string[] dbConnections = System.IO.Directory.GetFiles(dirLocOfDbConnection);
        foreach (string fileName in dbConnections) {
          string ConnectionName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          if (ConnectionName.Contains("Connection.")) {
            string dbConnection = ConnectionName.Substring(11);
            comboDbConnSummary.Items.Add(dbConnection.Substring(0, dbConnection.IndexOf(".")));
            comboDbConnSummary.SelectedIndex++;
          }
        }
      }
      string dbInstance = comboDbConnSummary.Items[comboDbConnSummary.SelectedIndex].ToString();

      // Create some filters so that user can focus on specific Summary "group by" constellations
      System.Windows.Forms.ComboBox comboDim1Filter = new System.Windows.Forms.ComboBox();
      comboDim1Filter.FormattingEnabled = true;
      comboDim1Filter.Location = new System.Drawing.Point(204, 28);
      comboDim1Filter.Name = "comboDim1Filter." + appId;
      comboDim1Filter.Size = new System.Drawing.Size(200, 21);
      comboDim1Filter.TabIndex = 0;
      comboDim1Filter.Top = 0;
      comboDim1Filter.Left = 204;
      comboDimensionFilter.Add(comboDim1Filter);

      // Create some filters so that user can focus on specific Summary "group by" constellations
      System.Windows.Forms.ComboBox comboDim2Filter = new System.Windows.Forms.ComboBox();
      comboDim2Filter.FormattingEnabled = true;
      comboDim2Filter.Location = new System.Drawing.Point(408, 28);
      comboDim2Filter.Name = "comboDim2Filter." + appId;
      comboDim2Filter.Size = new System.Drawing.Size(200, 21);
      comboDim2Filter.TabIndex = 0;
      comboDim2Filter.Top = 0;
      comboDim2Filter.Left = 408;
      comboDimensionFilter.Add(comboDim2Filter);

      // Create some filters so that user can focus on specific Summary "group by" constellations
      System.Windows.Forms.ComboBox comboDim3Filter = new System.Windows.Forms.ComboBox();
      comboDim3Filter.FormattingEnabled = true;
      comboDim3Filter.Location = new System.Drawing.Point(612, 28);
      comboDim3Filter.Name = "comboDim3Filter." + appId;
      comboDim3Filter.Size = new System.Drawing.Size(200, 21);
      comboDim3Filter.TabIndex = 0;
      comboDim3Filter.Top = 0;
      comboDim3Filter.Left = 612;
      comboDimensionFilter.Add(comboDim3Filter);

      // Create a panel (scrollable content
      Panel pieChartPanel = new Panel();
      pieChartPanel.Name = "pieChartPanel";
      pieChartPanel.Location = new System.Drawing.Point(0, 0);
      pieChartPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      pieChartPanel.AutoScroll = true;
      pieChartPanel.TabIndex = 0;

      // Populate each drop down list with user dimensions
      refreshGroupByDimensions();

      // Build the N dimensional matrix
      buildSummaryMatrix(projectRootFolder, appId, dbInstance, (comboDim1Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim2Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim3Filter.SelectedItem as ComboBoxItem).Value.ToString(), pieChartPanel);

      // Define triggers on the Dimension Filters to change the N dimensional matrix
      comboDbConnSummary.SelectedValueChanged += new System.EventHandler((sender, e) => changeDbConnectionSummaryOfDone(sender, e, projectRootFolder, appId, comboDbConnSummary.Items[comboDbConnSummary.SelectedIndex].ToString(), (comboDim1Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim2Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim3Filter.SelectedItem as ComboBoxItem).Value.ToString(), pieChartPanel));
      comboDim1Filter.SelectedValueChanged += new System.EventHandler((sender, e) => changeDimensionFilter(sender, e, projectRootFolder, appId, comboDbConnSummary.Items[comboDbConnSummary.SelectedIndex].ToString(), (comboDim1Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim2Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim3Filter.SelectedItem as ComboBoxItem).Value.ToString(), pieChartPanel));
      comboDim2Filter.SelectedValueChanged += new System.EventHandler((sender, e) => changeDimensionFilter(sender, e, projectRootFolder, appId, comboDbConnSummary.Items[comboDbConnSummary.SelectedIndex].ToString(), (comboDim1Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim2Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim3Filter.SelectedItem as ComboBoxItem).Value.ToString(), pieChartPanel));
      comboDim3Filter.SelectedValueChanged += new System.EventHandler((sender, e) => changeDimensionFilter(sender, e, projectRootFolder, appId, comboDbConnSummary.Items[comboDbConnSummary.SelectedIndex].ToString(), (comboDim1Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim2Filter.SelectedItem as ComboBoxItem).Value.ToString(), (comboDim3Filter.SelectedItem as ComboBoxItem).Value.ToString(), pieChartPanel));

      // Buttons ...
      System.Windows.Forms.Button buttonSetSummaryDimensions = new System.Windows.Forms.Button();
      buttonSetSummaryDimensions.Image = global::taoGUI.Properties.Resources.Dots;
      buttonSetSummaryDimensions.Location = new System.Drawing.Point(818, -1);
      buttonSetSummaryDimensions.Name = "buttonSetSummaryDimensions";
      buttonSetSummaryDimensions.Size = new System.Drawing.Size(24, 23);
      buttonSetSummaryDimensions.TabIndex = 0;
      buttonSetSummaryDimensions.Top = -1;
      buttonSetSummaryDimensions.Left = 818;
      buttonSetSummaryDimensions.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
      buttonSetSummaryDimensions.UseVisualStyleBackColor = true;
      buttonSetSummaryDimensions.Click += new EventHandler((sender, e) => button_SetSummaryDimensions(sender, e, projectRootFolder, appId));

      // Attach the data grid view to a container, add pading top to the container (24px) to give room to some controls (e.g. drop down list and search)
      tabPageContent.Controls.Add(comboDbConnSummary);
      tabPageContent.Controls.Add(comboDim1Filter);
      tabPageContent.Controls.Add(comboDim2Filter);
      tabPageContent.Controls.Add(comboDim3Filter);
      tabPageContent.Controls.Add(buttonSetSummaryDimensions);
      tabPageContent.Controls.Add(pieChartPanel);

      // Finally set up button tool-tips...
      ToolTip toolTip1 = new ToolTip();
      // Set up the delays for the ToolTip.
      toolTip1.AutoPopDelay = 5000;
      toolTip1.InitialDelay = 1000;
      toolTip1.ReshowDelay = 500;
      // Force the ToolTip text to be displayed whether or not the form is active.
      toolTip1.ShowAlways = true;
      // Set up the ToolTip text for the Combobox and Buttons.
      toolTip1.SetToolTip(comboDbConnSummary, "Select the database instance relevant to the Tao Suite Reports");
      toolTip1.SetToolTip(comboDim1Filter, "Group the project summary by the first dimension");
      toolTip1.SetToolTip(comboDim2Filter, "Group the project summary by the second dimension");
      toolTip1.SetToolTip(comboDim2Filter, "Group the project summary by the third dimension");
      toolTip1.SetToolTip(buttonSetSummaryDimensions, "Define the group-by dimensions for the project summary");

    }

    private void addTabContent(string appId, string tabReportName, TabPage tabPageContent) {
      switch (tabReportName) {
        case "Tao Suite Reports":
          addTabContentTaoSuiteReports(appId, tabPageContent);
          break;
        case "Summary of Done":
          addTabContentSummary(appId, tabPageContent);
          break;
        case "Velocity of Alignment":
          break;
        case "Tao Application Stability":
          break;
        case "Weather - Current":
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
        showStatusReprtTabPage(appId, "Velocity of Alignment", "|status|velocity");
      }
    }

    private void taoApplicationStatisticsToolStripMenuItem_Click(object sender, EventArgs e) {
      string appId = getProjectViewAppId();
      if (appId != null) {
        showAppStatusTabPage(appId);
        showStatusReprtTabPage(appId, "Tao Application Stability", "|status|stability");
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
