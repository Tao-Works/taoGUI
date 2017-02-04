using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using taoGUI.Json;
using static taoGUI.Json.TaoJsonConfigReader;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace taoGUI {
  public partial class formGroupByDimensions : Form {

    private struct TaoApplicationFolder {
      public string projectRootFolder;              // This is the project root folder containing all Tao applications and command scripts
      public string appId;                          // The Tao application reference (e.g. tao.baer.conf.emir for all EMIR related)
      public string dimensionsLocation;             // Location of the global group-by dimensions
      public string dimensionsMapLocation;          // Location of the group-by dimensions for application MAP
      public string taoSuiteInputFolder;            // Location of the Tao Suites

      public TaoApplicationFolder(string rootFolder, string app) {
        projectRootFolder = rootFolder;
        appId = app;
        dimensionsLocation = Application.StartupPath + @"\taoGUI.resources\dimensions.tao";
        dimensionsMapLocation = Application.StartupPath + @"\taoGUI.resources\dim_" + app + ".tao";
        taoSuiteInputFolder = projectRootFolder + @"\taoSuite_Input";
      }

    };

    private struct DimensionAttributeMap {
      public string dimension { get; set; }
      public List<string> dimensionAttributes { get; set; }
    }

    private struct TaoSuiteDimensionMap {
      public string taoSuiteName { get; set; }
      public List<DimensionAttributeMap> taoGroupByAttributes { get; set; }
    }

    private Form1 _masterForm;
    private List<TaoJsonGroupByDimension> userDefinedDimensions = new List<TaoJsonGroupByDimension>();
    private List<TaoJsonTaoSuiteDimensionMap> controlTaoSuiteDimensionMap = new List<TaoJsonTaoSuiteDimensionMap>();
    private List<TaoSuiteDimensionMap> userTaoSuiteDimensionMap = new List<TaoSuiteDimensionMap>();
    private TaoApplicationFolder taoFolders;        // Using the parameters passed, this holds the complete Tao folder locations (Tao suite input, cache)
    bool uncommittedChanges = false;

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

    public formGroupByDimensions(Form1 masterForm, string projectRootFolder, string appId) {
      taoFolders = new TaoApplicationFolder(projectRootFolder, appId);
      InitializeComponent();
      this.FormClosing += formGroupByDimensions_FormClosing;
      setTreeViewUserDimensions();
      treeViewUserDimensions.DrawMode = TreeViewDrawMode.OwnerDrawText;
      treeViewUserDimensions.HideSelection = false;
      treeViewUserDimensions.DrawNode += new DrawTreeNodeEventHandler(alwaysOn_DrawNode);


      if (File.Exists(taoFolders.dimensionsMapLocation)) {
        Dictionary<string, TaoJsonTaoSuiteDimensionMap> tmpDimension = TaoJsonConfigReader.getTaoSuiteDimensionMap(taoFolders.dimensionsMapLocation);
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

      setDataGridView();
      _masterForm = masterForm;
    }

    private void setDataGridView() {
      DataTable tableTaoSuiteReports = new DataTable();
      tableTaoSuiteReports.Columns.Add("taoSuiteName", typeof(string));
      tableTaoSuiteReports.Columns.Add("taoSuiteDimensions", typeof(string));
      if (Directory.Exists(taoFolders.taoSuiteInputFolder)) {
        string[] fileEntries = Directory.GetFiles(taoFolders.taoSuiteInputFolder);
        foreach (string fileName in fileEntries) {
          string taoSuiteName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          StringBuilder taoSuiteDimensions = new StringBuilder();
          var idxTaoSuite = userTaoSuiteDimensionMap.FirstOrDefault(x => x.taoSuiteName.Equals(taoSuiteName));
          if (!String.IsNullOrEmpty(idxTaoSuite.taoSuiteName)) {
            foreach (DimensionAttributeMap dimMap in idxTaoSuite.taoGroupByAttributes) {
              taoSuiteDimensions.Append(dimMap.dimension + " : { ");
              taoSuiteDimensions.Append(String.Join(", ", dimMap.dimensionAttributes));
              if (dimMap.Equals(idxTaoSuite.taoGroupByAttributes.Last<DimensionAttributeMap>())) {
                taoSuiteDimensions.Append(" }");
              } else {
                taoSuiteDimensions.Append(" }; ");
              }
            }
          }
          tableTaoSuiteReports.Rows.Add(taoSuiteName, taoSuiteDimensions.ToString());
        }
      }
      dataGridViewUserGroups.DataSource = tableTaoSuiteReports;
      dataGridViewUserGroups.Columns["taoSuiteName"].HeaderText = "Tao Suite";
      dataGridViewUserGroups.Columns["taoSuiteName"].Width = 280;
      dataGridViewUserGroups.Columns["taoSuiteDimensions"].HeaderText = "Group-By Dimensions";
      dataGridViewUserGroups.Columns["taoSuiteDimensions"].Width = 280;
    }

    private void formGroupByDimensions_FormClosing(Object sender, FormClosingEventArgs e) {
      if (uncommittedChanges && MessageBox.Show("The group-by dimensions have changed. Do you want to save them?", "Save Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
        
        // Save the global dimension attributes...
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("# -- DATA START --");
        sb.AppendLine("[");
        foreach (TaoJsonGroupByDimension userDimension in userDefinedDimensions) {
          sb.AppendLine("   {");
          sb.AppendLine("      \"dimension\" : \"" + userDimension.dimension + "\",");
          List<string> attributes = userDimension.attributes;
          if (attributes.Count() > 0) {
            sb.Append("      \"attributes\" : [ ");
            foreach (string attr in attributes) {
              sb.Append("\"" + attr + "\"");
              if (attr.Equals(attributes.Last<string>())) {
                sb.AppendLine(" ]");
              } else {
                sb.Append(", ");
              }
            }
          } else {
            sb.AppendLine("      \"attributes\" : [ ]");
          }
          if (userDimension.Equals(userDefinedDimensions.Last<TaoJsonGroupByDimension>())) {
            sb.AppendLine("   }");
          } else {
            sb.AppendLine("   },");
          }
        }
        sb.AppendLine("]");
        sb.Append("# -- DATA END --");
        using (System.IO.StreamWriter sw = System.IO.File.CreateText(taoFolders.dimensionsLocation)) {
          sw.WriteLine(sb);
          sw.Flush();
          sw.Close();
        }

        // Persist the Tao Suite group by dimensions control file...
        StringBuilder dim = new StringBuilder();
        dim.AppendLine("# -- DATA START --");
        dim.AppendLine("[");
        foreach (TaoSuiteDimensionMap userMap in userTaoSuiteDimensionMap) {
          dim.AppendLine("   {");
          dim.AppendLine("      \"taoSuiteName\" : \"" + userMap.taoSuiteName + "\",");
          dim.AppendLine("      \"groupByDimensions\" : [");
          foreach (DimensionAttributeMap dimAttrMap in userMap.taoGroupByAttributes) {
            dim.AppendLine("         {");
            dim.AppendLine("            \"dimension\" : \"" + dimAttrMap.dimension + "\",");
            List<string> attributes = dimAttrMap.dimensionAttributes;
            if (attributes.Count() > 0) {
              dim.Append("            \"attributes\" : [ ");
              foreach (string attr in attributes) {
                dim.Append("\"" + attr + "\"");
                if (attr.Equals(attributes.Last<string>())) {
                  dim.AppendLine(" ]");
                } else {
                  dim.Append(", ");
                }
              }
            } else {
              dim.AppendLine("            \"attributes\" : [ ]");
            }
            if (dimAttrMap.Equals(userMap.taoGroupByAttributes.Last<DimensionAttributeMap>())) {
              dim.AppendLine("         }");
            } else {
              dim.AppendLine("         },");
            }
          }
          dim.AppendLine("      ]");
          if (userMap.Equals(userTaoSuiteDimensionMap.Last<TaoSuiteDimensionMap>())) {
            dim.AppendLine("   }");
          } else {
            dim.AppendLine("   },");
          }
        }
        dim.AppendLine("]");
        dim.Append("# -- DATA END --");
        using (System.IO.StreamWriter sw = System.IO.File.CreateText(taoFolders.dimensionsMapLocation)) {
          sw.WriteLine(dim);
          sw.Flush();
          sw.Close();
        }

        // Finally, update the master form's drop down lists..
        _masterForm.refreshGroupByDimensions();

      }
    }

    private void setTreeViewUserDimensions() {
      treeViewUserDimensions.Nodes.Clear();
      userDefinedDimensions.Clear();
      TreeNode rootNode = treeViewUserDimensions.Nodes.Add("0|userDimensions", "Group-By Dimensions");
      if (File.Exists(taoFolders.dimensionsLocation)) {
        Dictionary<string, TaoJsonGroupByDimension> userDimensions = TaoJsonConfigReader.getTaoGroupByDimensionMap(taoFolders.dimensionsLocation);
        foreach (TaoJsonGroupByDimension userDimension in userDimensions.Values) {
          userDefinedDimensions.Add(userDimension); // use this later for validation (e.g. prevent duplication) and persist to JSON file
          string dimension = userDimension.dimension;
          TreeNode dimNode = rootNode.Nodes.Add("1|" + dimension, dimension);
          List<string> attributes = userDimension.attributes;
          foreach (string attr in attributes) {
            TreeNode attrNode = dimNode.Nodes.Add("2|" + dimension + "|" + attr, attr);
          }
          dimNode.Expand();
        }
        rootNode.Expand();
      }
    }

    private void menuButtonAddDimension_Click(object sender, EventArgs e) {
      string userText = toolStripUserDimension.Text;
      if (userText.Length == 0) {
        MessageBox.Show("Please enter a name for the group-by dimension.", "Group-By Dimensions", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      } else {
        if (treeViewUserDimensions.SelectedNode != null) {
          string selectedNodeName = treeViewUserDimensions.SelectedNode.Name;
          List<string> treeNodeTokens = selectedNodeName.Split('|').ToList<string>();
          bool isValidName = true;
          if (treeNodeTokens[0].Equals("0")) {
            foreach (TaoJsonGroupByDimension userDefinedDimension in userDefinedDimensions) {
              if (userDefinedDimension.dimension.Equals(userText)) {
                isValidName = false;
              }
            }
            if (!isValidName ) {
              MessageBox.Show(@"Group-by dimension '" + userText + "' already exists.", "Duplicate Group-By Dimension", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            } else {
              // Add dimension...
              TaoJsonGroupByDimension userDefinedDimension = new TaoJsonGroupByDimension();
              userDefinedDimension.dimension = userText;
              userDefinedDimension.attributes = new List<string>();
              userDefinedDimensions.Add(userDefinedDimension);
            }
          } else {
            string impliedDimensionName = treeNodeTokens[1];
            foreach (TaoJsonGroupByDimension userDefinedDimension in userDefinedDimensions) {
              if (userDefinedDimension.dimension.Equals(impliedDimensionName)) {
                if (userDefinedDimension.attributes.Contains(userText)) {
                  isValidName = false;
                } else {
                  // Add attribute...
                  userDefinedDimension.attributes.Add(userText);
                }
              }
            }
            if (!isValidName) {
              MessageBox.Show(@"Group-by dimension '" + impliedDimensionName + "' already contains the attribute '" + userText + "'.", "Duplicate Dimension Attribute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
          }
          if (isValidName) {
            // Refresh tree view...
            treeViewUserDimensions.Nodes.Clear();
            TreeNode rootNode = treeViewUserDimensions.Nodes.Add("0|userDimensions", "Group-By Dimensions");
            foreach (TaoJsonGroupByDimension userDimension in userDefinedDimensions) {
              string dimension = userDimension.dimension;
              TreeNode dimNode = rootNode.Nodes.Add("1|" + dimension, dimension);
              List<string> attributes = userDimension.attributes;
              foreach (string attr in attributes) {
                TreeNode attrNode = dimNode.Nodes.Add("2|" + dimension + "|" + attr, attr);
              }
              dimNode.Expand();
            }
            rootNode.Expand();
            uncommittedChanges = true;
          }
        }
      }
    }

    private void menuButtonRemoveDimension_Click(object sender, EventArgs e) {
      bool foundDimensionToRemove = false;
      if (treeViewUserDimensions.SelectedNode != null) {
        string selectedNodeName = treeViewUserDimensions.SelectedNode.Name;
        List<string> treeNodeTokens = selectedNodeName.Split('|').ToList<string>();
        if (treeNodeTokens[0].Equals("0")) {
          MessageBox.Show(@"Please select a dimension or attribute to remove.", "Remove Dimension / Attribute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        } else if (treeNodeTokens[0].Equals("1") && MessageBox.Show("Are you sure you want to remove the group-by dimension '" + treeNodeTokens[1] + "'?", "Remove Dimension", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
          TaoJsonGroupByDimension removeDimension = new TaoJsonGroupByDimension();
          foreach (TaoJsonGroupByDimension userDefinedDimension in userDefinedDimensions) {
            if (userDefinedDimension.dimension.Equals(treeNodeTokens[1])) {
              removeDimension = userDefinedDimension;
              foundDimensionToRemove = true;
            }
          }
          if (foundDimensionToRemove) {
            userDefinedDimensions.Remove(removeDimension);
          }
        } else if (treeNodeTokens[0].Equals("2") && MessageBox.Show("Are you sure you want to remove the attribute '" + treeNodeTokens[2] + "' from the group-by dimension '" + treeNodeTokens[1] + "'?", "Remove Dimension", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
          foreach (TaoJsonGroupByDimension userDefinedDimension in userDefinedDimensions) {
            if (userDefinedDimension.dimension.Equals(treeNodeTokens[1])) {
              userDefinedDimension.attributes.Remove(treeNodeTokens[2]);
              foundDimensionToRemove = true;
            }
          }
        } else {
          MessageBox.Show(@"Please select a dimension or attribute to remove.", "Remove Dimension / Attribute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
      if (foundDimensionToRemove) {
        // Refresh tree view...
        treeViewUserDimensions.Nodes.Clear();
        TreeNode rootNode = treeViewUserDimensions.Nodes.Add("0|userDimensions", "Group-By Dimensions");
        foreach (TaoJsonGroupByDimension userDimension in userDefinedDimensions) {
          string dimension = userDimension.dimension;
          TreeNode dimNode = rootNode.Nodes.Add("1|" + dimension, dimension);
          List<string> attributes = userDimension.attributes;
          foreach (string attr in attributes) {
            TreeNode attrNode = dimNode.Nodes.Add("2|" + dimension + "|" + attr, attr);
          }
          dimNode.Expand();
        }
        rootNode.Expand();
        uncommittedChanges = true;
      }
    }

    private void menuButtonAddToTaoSuite_Click(object sender, EventArgs e) {

      // Buid a list of selected Tao Sheets...
      List<string> taoSheets = new List<string>();
      taoSheets.Clear();
      string taoSheet = string.Empty;
      DataGridViewSelectedRowCollection rows = dataGridViewUserGroups.SelectedRows;
      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          taoSheet = myRow.ItemArray[0].ToString();
          if (!taoSheets.Contains(taoSheet)) {
            taoSheets.Add(taoSheet);
          }
        }
      } else {
        Int32 selectedCellCount = dataGridViewUserGroups.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = dataGridViewUserGroups.SelectedCells[i].RowIndex;
            taoSheet = dataGridViewUserGroups.Rows[rowIndex].Cells[0].Value.ToString();
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
            }
          }
        } else {
          MessageBox.Show("No Tao Suite was selected to assign dimension attributes.", "Tao Suite Map", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }

      // Build a list of selected dimension attributes...
      string selectedDimension = string.Empty;
      string selectedAttribute = string.Empty;
      if (treeViewUserDimensions.SelectedNode != null) {
        string selectedNodeName = treeViewUserDimensions.SelectedNode.Name;
        List<string> treeNodeTokens = selectedNodeName.Split('|').ToList<string>();
        if (treeNodeTokens[0].Equals("2")) {
          selectedDimension = treeNodeTokens[1];
          selectedAttribute = treeNodeTokens[2];
        } else {
          MessageBox.Show("Please select a group-by dimension attribute to map to a Tao Suite.", "Tao Suite Map", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }

      // Find the mapping in the structure and update where necessary...
      if (selectedDimension != string.Empty && selectedAttribute != string.Empty) {
        foreach (string ts in taoSheets) {
          var idxTaoSuite = userTaoSuiteDimensionMap.FirstOrDefault(x => x.taoSuiteName.Equals(ts));
          if (String.IsNullOrEmpty(idxTaoSuite.taoSuiteName)) {
            idxTaoSuite = new TaoSuiteDimensionMap();
            idxTaoSuite.taoSuiteName = ts;
            idxTaoSuite.taoGroupByAttributes = new List<DimensionAttributeMap>();
            DimensionAttributeMap tmpDimensionAttribute = new DimensionAttributeMap();
            tmpDimensionAttribute.dimension = selectedDimension;
            tmpDimensionAttribute.dimensionAttributes = new List<string>();
            tmpDimensionAttribute.dimensionAttributes.Add(selectedAttribute);
            idxTaoSuite.taoGroupByAttributes.Add(tmpDimensionAttribute);
            userTaoSuiteDimensionMap.Add(idxTaoSuite);
            uncommittedChanges = true;
          } else {
            var idxDimension = idxTaoSuite.taoGroupByAttributes.FirstOrDefault(x => x.dimension.Equals(selectedDimension));
            if (String.IsNullOrEmpty(idxDimension.dimension)) {
              DimensionAttributeMap tmpDimensionAttribute = new DimensionAttributeMap();
              tmpDimensionAttribute.dimension = selectedDimension;
              tmpDimensionAttribute.dimensionAttributes = new List<string>();
              tmpDimensionAttribute.dimensionAttributes.Add(selectedAttribute);
              idxTaoSuite.taoGroupByAttributes.Add(tmpDimensionAttribute);
              uncommittedChanges = true;
            } else {
              if (!idxDimension.dimensionAttributes.Contains(selectedAttribute)) {
                idxDimension.dimensionAttributes.Add(selectedAttribute);
                uncommittedChanges = true;
              }
            }
          }
        }
      }

      // Refresh data grid view...
      DataTable tableTaoSuiteReports = new DataTable();
      tableTaoSuiteReports.Columns.Add("taoSuiteName", typeof(string));
      tableTaoSuiteReports.Columns.Add("taoSuiteDimensions", typeof(string));
      if (Directory.Exists(taoFolders.taoSuiteInputFolder)) {
        string[] fileEntries = Directory.GetFiles(taoFolders.taoSuiteInputFolder);
        foreach (string fileName in fileEntries) {
          string taoSuiteName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          StringBuilder taoSuiteDimensions = new StringBuilder();
          var idxTaoSuite = userTaoSuiteDimensionMap.FirstOrDefault(x => x.taoSuiteName.Equals(taoSuiteName));
          if (!String.IsNullOrEmpty(idxTaoSuite.taoSuiteName)) {
            foreach (DimensionAttributeMap dimMap in idxTaoSuite.taoGroupByAttributes) {
              taoSuiteDimensions.Append(dimMap.dimension + " : { ");
              taoSuiteDimensions.Append(String.Join(", ", dimMap.dimensionAttributes));
              if (dimMap.Equals(idxTaoSuite.taoGroupByAttributes.Last<DimensionAttributeMap>())) {
                taoSuiteDimensions.Append(" }");
              } else {
                taoSuiteDimensions.Append(" }; ");
              }
            }
          }
          tableTaoSuiteReports.Rows.Add(taoSuiteName, taoSuiteDimensions.ToString());
        }
      }
      dataGridViewUserGroups.DataSource = tableTaoSuiteReports;

    }

    private void menuButtonRemoveFromTaoSuite_Click(object sender, EventArgs e) {

      // Buid a list of selected Tao Sheets...
      List<string> taoSheets = new List<string>();
      taoSheets.Clear();
      string taoSheet = string.Empty;
      DataGridViewSelectedRowCollection rows = dataGridViewUserGroups.SelectedRows;
      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          taoSheet = myRow.ItemArray[0].ToString();
          if (!taoSheets.Contains(taoSheet)) {
            taoSheets.Add(taoSheet);
          }
        }
      } else {
        Int32 selectedCellCount = dataGridViewUserGroups.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = dataGridViewUserGroups.SelectedCells[i].RowIndex;
            taoSheet = dataGridViewUserGroups.Rows[rowIndex].Cells[0].Value.ToString();
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
            }
          }
        } else {
          MessageBox.Show("No Tao Suite was selected to remove dimension attributes.", "Tao Suite Map", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }

      // Build a list of selected dimension attributes...
      string selectedDimension = string.Empty;
      string selectedAttribute = string.Empty;
      if (treeViewUserDimensions.SelectedNode != null) {
        string selectedNodeName = treeViewUserDimensions.SelectedNode.Name;
        List<string> treeNodeTokens = selectedNodeName.Split('|').ToList<string>();
        if (treeNodeTokens[0].Equals("1")) {
          selectedDimension = treeNodeTokens[1];
        } else if (treeNodeTokens[0].Equals("2")) {
          selectedDimension = treeNodeTokens[1];
          selectedAttribute = treeNodeTokens[2];
        } else {
          MessageBox.Show("Please select a group-by dimension attribute to remove from a Tao Suite.", "Tao Suite Map", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }

      // Find the mapping in the structure and remove where necessary...
      if (selectedDimension != string.Empty && selectedAttribute == string.Empty) {
        foreach (string ts in taoSheets) {
          var idxTaoSuite = userTaoSuiteDimensionMap.FirstOrDefault(x => x.taoSuiteName.Equals(ts));
          if (!String.IsNullOrEmpty(idxTaoSuite.taoSuiteName)) {
            var idxDimension = idxTaoSuite.taoGroupByAttributes.FirstOrDefault(x => x.dimension.Equals(selectedDimension));
            if (!String.IsNullOrEmpty(idxDimension.dimension)) {
              idxTaoSuite.taoGroupByAttributes.Remove(idxDimension);
              if (idxTaoSuite.taoGroupByAttributes.Count == 0) {
                userTaoSuiteDimensionMap.Remove(idxTaoSuite);
              }
              uncommittedChanges = true;
            }
          }
        }
      } else if (selectedDimension != string.Empty && selectedAttribute != string.Empty) {
        foreach (string ts in taoSheets) {
          var idxTaoSuite = userTaoSuiteDimensionMap.FirstOrDefault(x => x.taoSuiteName.Equals(ts));
          if (!String.IsNullOrEmpty(idxTaoSuite.taoSuiteName)) {
            var idxDimension = idxTaoSuite.taoGroupByAttributes.FirstOrDefault(x => x.dimension.Equals(selectedDimension));
            if (!String.IsNullOrEmpty(idxDimension.dimension)) {
              if (idxDimension.dimensionAttributes.Contains(selectedAttribute)) {
                idxDimension.dimensionAttributes.Remove(selectedAttribute);
                if (idxDimension.dimensionAttributes.Count == 0) {
                  idxTaoSuite.taoGroupByAttributes.Remove(idxDimension);
                  if (idxTaoSuite.taoGroupByAttributes.Count == 0) {
                    userTaoSuiteDimensionMap.Remove(idxTaoSuite);
                  }
                }
                uncommittedChanges = true;
              }
            }
          }
        }
      }

      // Refresh data grid view...
      DataTable tableTaoSuiteReports = new DataTable();
      tableTaoSuiteReports.Columns.Add("taoSuiteName", typeof(string));
      tableTaoSuiteReports.Columns.Add("taoSuiteDimensions", typeof(string));
      if (Directory.Exists(taoFolders.taoSuiteInputFolder)) {
        string[] fileEntries = Directory.GetFiles(taoFolders.taoSuiteInputFolder);
        foreach (string fileName in fileEntries) {
          string taoSuiteName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          StringBuilder taoSuiteDimensions = new StringBuilder();
          var idxTaoSuite = userTaoSuiteDimensionMap.FirstOrDefault(x => x.taoSuiteName.Equals(taoSuiteName));
          if (!String.IsNullOrEmpty(idxTaoSuite.taoSuiteName)) {
            foreach (DimensionAttributeMap dimMap in idxTaoSuite.taoGroupByAttributes) {
              taoSuiteDimensions.Append(dimMap.dimension + " : { ");
              taoSuiteDimensions.Append(String.Join(", ", dimMap.dimensionAttributes));
              if (dimMap.Equals(idxTaoSuite.taoGroupByAttributes.Last<DimensionAttributeMap>())) {
                taoSuiteDimensions.Append(" }");
              } else {
                taoSuiteDimensions.Append(" }; ");
              }
            }
          }
          tableTaoSuiteReports.Rows.Add(taoSuiteName, taoSuiteDimensions.ToString());
        }
      }
      dataGridViewUserGroups.DataSource = tableTaoSuiteReports;

    }

    private void menuButtonShowTaoSuite_Click(object sender, EventArgs e) {

      Excel.Application xlApp = null;
      Excel.Workbook xlWorkbook = null;

      xlApp = new Excel.Application();
      xlApp.ScreenUpdating = true;
      xlApp.Visible = true;

      List<string> taoSheets = new List<string>();
      string taoSheet = string.Empty;

      DataGridViewSelectedRowCollection rows = dataGridViewUserGroups.SelectedRows;
      taoSheets.Clear();

      if (rows.Count > 0) {
        foreach (DataGridViewRow row in rows) {
          DataRow myRow = (row.DataBoundItem as DataRowView).Row;
          taoSheet = taoFolders.taoSuiteInputFolder + @"\" + myRow.ItemArray[0].ToString();
          if (!taoSheets.Contains(taoSheet)) {
            taoSheets.Add(taoSheet);
          }
        }
      } else {
        Int32 selectedCellCount = dataGridViewUserGroups.GetCellCount(DataGridViewElementStates.Selected);
        if (selectedCellCount > 0) {
          for (int i = 0; i < selectedCellCount; i++) {
            int rowIndex = dataGridViewUserGroups.SelectedCells[i].RowIndex;
            taoSheet = taoFolders.taoSuiteInputFolder + @"\" + dataGridViewUserGroups.Rows[rowIndex].Cells[0].Value.ToString();
            if (!taoSheets.Contains(taoSheet)) {
              taoSheets.Add(taoSheet);
            }
          }
        } else {
          MessageBox.Show("No Tao Suite was selected for opening.", "Open Tao Suite", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }

      foreach (string ts in taoSheets) {
        if (System.IO.File.Exists(ts)) {
          xlWorkbook = xlApp.Workbooks.Open(ts, true, false); // Update links (e.g. parameter files) and allow user to read and write as necessary
        }
      }

    }
  }
}
