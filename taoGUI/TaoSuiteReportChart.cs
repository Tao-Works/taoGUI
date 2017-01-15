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

    public TaoSuiteReportChart(string projectRootFolder, string taoSuite, string dbInstance, double passRateMean, double passRateStdDev, double lowerBollingerBand, double upperBollingerBand, double impliedVolatility) {
      InitializeComponent();
      this.Text = "Pass Rate Statistics - " + taoSuite + " @" + dbInstance;
      passRateChart.Series.Clear();
      var seriesTrendLine = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "trendLine",
        Color = System.Drawing.Color.CornflowerBlue,
        BorderWidth = 4,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.Line
      };
      var seriesPassRateMean = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "passRateMean",
        Color = System.Drawing.Color.DarkOrange,
        BorderWidth = 2,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.Line
      };
      var seriesLowerBollingerBand = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "lowerBollingerBand",
        Color = Color.FromArgb(40, System.Drawing.Color.LightSalmon),
        BorderColor = System.Drawing.Color.DarkSalmon,
        BorderWidth = 1,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.Range
      };
      var seriesPassRateStdDev = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "passRateStdDev",
        Color = Color.FromArgb(132, System.Drawing.Color.LemonChiffon),
        BorderColor = System.Drawing.Color.SandyBrown,
        BorderWidth = 1,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.Range
      };
      // TODO: add future-implied volatility
      passRateChart.Series.Add(seriesLowerBollingerBand);
      passRateChart.Series.Add(seriesPassRateStdDev);
      passRateChart.Series.Add(seriesTrendLine);
      passRateChart.Series.Add(seriesPassRateMean);
      double chartMinimum = lowerBollingerBand;
      double chartMaximum = 0.0;
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
            double taoTrendLine = (double)Math.Round(Convert.ToDouble(fields[7]), 2);
            if (taoTotalTests > 0) {
              DataPoint pointLowerBollingerBand = new DataPoint();
              pointLowerBollingerBand.SetValueXY(x.ToOADate(), lowerBollingerBand, (passRateMean - passRateStdDev));
              pointLowerBollingerBand.ToolTip = string.Format("Lower Bollinger Band = " + Math.Round(lowerBollingerBand, 4).ToString());
              seriesLowerBollingerBand.Points.Add(pointLowerBollingerBand);
              DataPoint pointPassRateStdDev = new DataPoint();
              pointPassRateStdDev.SetValueXY(x.ToOADate(), (passRateMean - passRateStdDev), passRateMean);
              pointPassRateStdDev.ToolTip = string.Format("Standard Deviation = " + Math.Round(passRateStdDev, 4).ToString());
              seriesPassRateStdDev.Points.Add(pointPassRateStdDev);
              DataPoint pointTrendLine = new DataPoint();
              pointTrendLine.SetValueXY(x.ToOADate(), taoTrendLine);
              pointTrendLine.ToolTip = string.Format("{0}, {1}", x.ToShortDateString() + " " + x.ToShortTimeString(), taoTrendLine.ToString() + " % pass");
              seriesTrendLine.Points.Add(pointTrendLine);
              DataPoint pointPassRateMean = new DataPoint();
              pointPassRateMean.SetValueXY(x.ToOADate(), passRateMean);
              pointPassRateMean.ToolTip = string.Format("Mean = " + Math.Round(passRateMean,4).ToString());
              seriesPassRateMean.Points.Add(pointPassRateMean);
              if (chartMaximum < taoTrendLine) {
                chartMaximum = taoTrendLine;
              }
              if (chartMinimum > taoTrendLine) {
                chartMinimum = taoTrendLine;
              }
            }
          }
        }
      }
      chartMaximum = Math.Ceiling(chartMaximum / 10.0) * 10.0;
      if (chartMaximum > 100.0) {
        chartMaximum = 100.0;
      }
      chartMinimum = Math.Floor(chartMinimum / 10.0) * 10.0;
      if (chartMinimum < 0) {
        chartMinimum = 0.0;
      }
      passRateChart.ChartAreas[0].AxisY.Maximum = chartMaximum;
      passRateChart.ChartAreas[0].AxisY.Minimum = chartMinimum;
      passRateChart.Invalidate();
    }

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
