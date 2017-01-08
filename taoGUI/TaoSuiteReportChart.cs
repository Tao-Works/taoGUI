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

    public TaoSuiteReportChart(string projectRootFolder, string taoSuite, string dbInstance) {
      InitializeComponent();
      this.Text = "Pass Rate History - " + taoSuite;
      passRateChart.Series.Clear();
      var seriesPass = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "Pass",
        Color = System.Drawing.Color.LightSteelBlue,
        BorderColor = System.Drawing.Color.DarkSlateBlue,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.StackedColumn100
      };
      var seriesFail = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "Fail",
        Color = System.Drawing.Color.LemonChiffon,
        BorderColor = System.Drawing.Color.DarkOrange,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.StackedColumn100
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
      passRateChart.Series.Add(seriesTrendLine);
      string taoSuiteOutputFolder = projectRootFolder + @"\taoSuite_Report";
      string filePattern = taoSuite.Substring(0, taoSuite.IndexOf(".")) + "*" + dbInstance + ".xls";
      string[] taoResults = System.IO.Directory.GetFiles(taoSuiteOutputFolder, filePattern);
      var orderedResults = taoResults.OrderBy(f => f.ToString());
      string taoGenerated = ""; // YYYY-MM-DD_HHMM format
      foreach ( string orderedResult in orderedResults) {
        taoGenerated = orderedResult.Substring(orderedResult.LastIndexOf("\\") + 1);
        taoGenerated = taoGenerated.Substring(taoGenerated.IndexOf(".") + 1);
        taoGenerated = taoGenerated.Substring(0, taoGenerated.IndexOf("."));
        int _year = Convert.ToInt32(taoGenerated.Substring(0, 4));
        int _month = Convert.ToInt32(taoGenerated.Substring(5, 2));
        int _day = Convert.ToInt32(taoGenerated.Substring(8, 2));
        int _hour = Convert.ToInt32(taoGenerated.Substring(11, 2));
        int _minute = Convert.ToInt32(taoGenerated.Substring(13, 2));
        System.DateTime x = new System.DateTime(_year, _month, _day, _hour, _minute, 0);
        taoReportReader taoSuiteResults = new taoReportReader(orderedResult);
        int taoTotalTests = taoSuiteResults.getTotalTests();
        int taoTestPass = taoSuiteResults.getPairsThatAreEqual();
        double taoTrendLine = (double)Decimal.Round((Decimal)taoSuiteResults.getOverallPassRate(),2);
        DataPoint pointPass = new DataPoint();
        pointPass.SetValueXY(x.ToOADate(), taoTestPass);
        pointPass.ToolTip = string.Format("{0}, {1}", x.ToShortDateString() + " " + x.ToShortTimeString(), taoTestPass.ToString() + " tests");
        seriesPass.Points.Add(pointPass);
        DataPoint pointFail = new DataPoint();
        pointFail.SetValueXY(x.ToOADate(), (taoTotalTests - taoTestPass));
        pointFail.ToolTip = string.Format("{0}, {1}", x.ToShortDateString() + " " + x.ToShortTimeString(), (taoTotalTests - taoTestPass).ToString() + " tests");
        seriesFail.Points.Add(pointFail);
        DataPoint pointTrend = new DataPoint();
        pointTrend.SetValueXY(x.ToOADate(), taoTrendLine);
        pointTrend.ToolTip = string.Format("{0}, {1}", x.ToShortDateString() + " " + x.ToShortTimeString(), taoTrendLine.ToString() + " % pass");
        seriesTrendLine.Points.Add(pointTrend);
        passRateChart.Invalidate();
      }
    }
  }
}
