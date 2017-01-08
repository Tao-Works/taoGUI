namespace taoGUI {
  partial class TaoSuiteReportChart {
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
      System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
      System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
      System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaoSuiteReportChart));
      this.passRateChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
      ((System.ComponentModel.ISupportInitialize)(this.passRateChart)).BeginInit();
      this.SuspendLayout();
      // 
      // passRateChart
      // 
      chartArea1.Name = "ChartArea1";
      this.passRateChart.ChartAreas.Add(chartArea1);
      this.passRateChart.Dock = System.Windows.Forms.DockStyle.Fill;
      legend1.Name = "Legend1";
      this.passRateChart.Legends.Add(legend1);
      this.passRateChart.Location = new System.Drawing.Point(0, 0);
      this.passRateChart.Name = "passRateChart";
      series1.ChartArea = "ChartArea1";
      series1.Legend = "Legend1";
      series1.Name = "Series1";
      this.passRateChart.Series.Add(series1);
      this.passRateChart.Size = new System.Drawing.Size(624, 441);
      this.passRateChart.TabIndex = 0;
      this.passRateChart.Text = "chart1";
      // 
      // TaoSuiteReportChart
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(624, 441);
      this.Controls.Add(this.passRateChart);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "TaoSuiteReportChart";
      this.Text = "TaoSuiteReportChart";
      ((System.ComponentModel.ISupportInitialize)(this.passRateChart)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.DataVisualization.Charting.Chart passRateChart;
  }
}