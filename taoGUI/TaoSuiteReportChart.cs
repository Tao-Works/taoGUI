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

namespace taoGUI {
  public partial class TaoSuiteReportChart : Form {

    public TaoSuiteReportChart(SeriesChartType targetChartType, string projectRootFolder, string taoSuite, string dbInstance) {
      InitializeComponent();
      int _opacityPass = 255;
      int _opacityFail = 255;
      if (targetChartType == SeriesChartType.StackedColumn100) {
        this.Text = "Pass Rate History (percentage) - " + taoSuite + " @" + dbInstance;
      } else if (targetChartType == SeriesChartType.StackedArea) {
        this.Text = "Pass Rate History (actual) - " + taoSuite + " @" + dbInstance;
        _opacityPass = 132;
        _opacityFail = 196;
      }
      passRateChart.Series.Clear();
      var seriesPass = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "Pass",
        Color = Color.FromArgb(_opacityPass, System.Drawing.Color.LightSteelBlue ),
        BorderColor = System.Drawing.Color.DarkSlateBlue,
        BorderWidth = 2,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = targetChartType
      };
      var seriesFail = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "Fail",
        Color = Color.FromArgb(_opacityFail, System.Drawing.Color.LemonChiffon ),
        BorderColor = System.Drawing.Color.DarkOrange,
        BorderWidth = 2,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = targetChartType
      };
      var seriesTrendLine = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "TrendLine",
        Color = System.Drawing.Color.CornflowerBlue,
        BorderWidth = 4,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.Line
      };
      passRateChart.Series.Add(seriesPass);
      passRateChart.Series.Add(seriesFail);
      if (targetChartType == SeriesChartType.StackedColumn100) {
        passRateChart.Series.Add(seriesTrendLine);
      }
      string chartLocation = Application.StartupPath + @"\taoGUI.resources\chart_" + projectRootFolder.Substring(projectRootFolder.LastIndexOf("\\") + 1) + "_" + dbInstance + "_" + taoSuite.Substring(0, taoSuite.IndexOf(".")) + ".tao";
      if (System.IO.File.Exists(chartLocation)) {
        using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(chartLocation)) {
          parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
          parser.SetDelimiters(";");
          string[] fields = parser.ReadFields();    // Skip header...
          while (!parser.EndOfData) {
            fields = parser.ReadFields();
            int _year = Convert.ToInt32(fields[0]);
            int _month = Convert.ToInt32(fields[1]);
            int _day = Convert.ToInt32(fields[2]);
            int _hour = Convert.ToInt32(fields[3]);
            int _minute = Convert.ToInt32(fields[4]);
            System.DateTime x = new System.DateTime(_year, _month, _day, _hour, _minute, 0);
            int taoTotalTests = Convert.ToInt32(fields[5]);
            int taoTestPass = Convert.ToInt32(fields[6]);
            double taoTrendLine = (double)Math.Round(Convert.ToDouble(fields[7]),2);
            if (taoTotalTests > 0) {
              DataPoint pointPass = new DataPoint();
              pointPass.SetValueXY(x.ToOADate(), taoTestPass);
              pointPass.ToolTip = string.Format("{0}, {1}", x.ToShortDateString() + " " + x.ToShortTimeString(), taoTestPass.ToString() + " PASS");
              seriesPass.Points.Add(pointPass);
              DataPoint pointFail = new DataPoint();
              pointFail.SetValueXY(x.ToOADate(), (taoTotalTests - taoTestPass));
              pointFail.ToolTip = string.Format("{0}, {1}", x.ToShortDateString() + " " + x.ToShortTimeString(), (taoTotalTests - taoTestPass).ToString() + " FAIL");
              seriesFail.Points.Add(pointFail);
              if (targetChartType == SeriesChartType.StackedColumn100) {
                DataPoint pointTrend = new DataPoint();
                pointTrend.SetValueXY(x.ToOADate(), taoTrendLine);
                pointTrend.ToolTip = string.Format("{0}, {1}", x.ToShortDateString() + " " + x.ToShortTimeString(), taoTrendLine.ToString() + " % pass");
                seriesTrendLine.Points.Add(pointTrend);
              }
            }
          }
        }
      }
      passRateChart.Invalidate();
    }

  }
}
