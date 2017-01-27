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

namespace taoGUI {
  public partial class formGroupByDimensions : Form {

    public static string TAO_DIMENSIONS_FILE = Application.StartupPath + @"\taoGUI.resources\dimensions.tao";
    private Form1 _masterForm;

    public formGroupByDimensions(Form1 masterForm) {                                                        
      InitializeComponent();
      setTreeViewUserDimensions();
      _masterForm = masterForm;
    }

    private void setTreeViewUserDimensions() {
      treeViewUserDimensions.Nodes.Clear();
      if (File.Exists(TAO_DIMENSIONS_FILE)) {
        Dictionary<string, TaoJsonGroupByDimension> userDimensions = TaoJsonConfigReader.getTaoGroupByDimensionMap(TAO_DIMENSIONS_FILE);
        foreach (TaoJsonGroupByDimension userDimension in userDimensions.Values) {
          string dimension = userDimension.dimension;
          TreeNode dimNode = treeViewUserDimensions.Nodes.Add(dimension, dimension);
          List<string> attributes = userDimension.attributes;
          foreach (string attr in attributes) {
            TreeNode attrNode = dimNode.Nodes.Add(dimension + "." + attr, attr);
          } 
        }
      }
    }

    private void menuButtonAddDimension_Click(object sender, EventArgs e) {

    }

    private void menuButtonRemoveDimension_Click(object sender, EventArgs e) {

    }

    private void menuButtonAddToTaoSuite_Click(object sender, EventArgs e) {

    }

    private void menuButtonRemoveFromTaoSuite_Click(object sender, EventArgs e) {

    }
  }
}
