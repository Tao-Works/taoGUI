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
    }

    public void setProgressDescription(string description) {
      this.progressDescription.Text = description;
    }

    public void setProgressAction(string description) {
      this.taoSuiteAction.Text = description;
    }

    public void setProgress(int completion) {
      this.progressBar.Value = completion;
    }

  }
}
