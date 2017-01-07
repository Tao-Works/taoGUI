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
  public partial class taoProgressBar : Form {

    public taoProgressBar() {
      InitializeComponent();
      setProgressDescription("");
      setProgressAction(1, "");
      setProgressAction(2, "");
      setProgressAction(3, "");
      setProgress(0);
    }

    public void setProgressDescription(string description) {
      this.progressDescription.Text = description;
    }

    public void setProgressAction(int actionIndex, string description) {
      switch (actionIndex) {
        case 1:
          this.taoSuiteAction_1.Text = description;
          break;
        case 2:
          this.taoSuiteAction_2.Text = description;
          break;
        case 3:
          this.taoSuiteAction_3.Text = description;
          break;
        default:
          break;
      }
    }

    public void setProgress(int completion) {
      this.progressBar.Value = completion;
    }

    public int getProgressUpperLimit() {
      return this.progressBar.Maximum;
    }

  }
}
