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

    // Histogram...
    public TaoSuiteReportChart(string projectRootFolder, string taoSuite, string dbInstance, double passRateMean, double passRateStdDev, double lowerBollingerBand) {
      InitializeComponent();
      this.Text = "Pass Rate Histogram - " + taoSuite + " @" + dbInstance;
      passRateChart.Series.Clear();
      var seriesPassRateHistogram = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "passRateHistogram",
        Color = Color.FromArgb(132, System.Drawing.Color.LightSteelBlue),
        BorderColor = System.Drawing.Color.DarkSlateBlue,
        BorderWidth = 1,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.Int32,
        ChartType = SeriesChartType.Area
      };
      var seriesPassRateMean = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "passRateMean",
        Color = Color.FromArgb(220, System.Drawing.Color.DarkOrange),
        BorderColor = System.Drawing.Color.DarkOrange,
        BorderWidth = 1,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.Double,
        ChartType = SeriesChartType.Column
      };
      var seriesPassRateStdDev = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "passRateStdDev",
        Color = Color.FromArgb(164, System.Drawing.Color.Salmon),
        BorderColor = System.Drawing.Color.Salmon,
        BorderWidth = 1,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.Double,
        ChartType = SeriesChartType.Column
      };
      var seriesLowerBollingerBand = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "lowerBollingerBand",
        Color = Color.FromArgb(132, System.Drawing.Color.LightSalmon),
        BorderColor = System.Drawing.Color.LightSalmon,
        BorderWidth = 1,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.Double,
        ChartType = SeriesChartType.Column
      };
      passRateChart.Series.Add(seriesPassRateHistogram);
      passRateChart.Series.Add(seriesLowerBollingerBand);
      passRateChart.Series.Add(seriesPassRateStdDev);
      passRateChart.Series.Add(seriesPassRateMean);
      int[] buckets = new int[20];
      for (int i = 0; i < 20; i++) {
        buckets[i] = 0;
      }
      string chartLocation = Application.StartupPath + @"\taoGUI.resources\chart_" + projectRootFolder.Substring(projectRootFolder.LastIndexOf("\\") + 1) + "_" + dbInstance + "_" + taoSuite.Substring(0, taoSuite.IndexOf(".")) + ".tao";
      if (System.IO.File.Exists(chartLocation)) {
        using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(chartLocation)) {
          parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
          parser.SetDelimiters(";");
          string[] fields = parser.ReadFields();    // Skip header...
          while (!parser.EndOfData) {
            fields = parser.ReadFields();
            double taoTrendLine = (double)Math.Round(Convert.ToDouble(fields[7]), 2);
            for (int i = 0; i < 20; i++) {
              if ((double)(i*5) < taoTrendLine && taoTrendLine <= (double)((i+1)*5)) {
                buckets[i]++;
              }
            }
          }
        }
      }
      int maxBucket = 0;
      for (int i = 0; i < 20; i++) {
        if (buckets[i] > maxBucket) {
          maxBucket = buckets[i];
        }
      }
      maxBucket = Convert.ToInt32(Math.Ceiling((double)maxBucket / 10.0) * 10.0);
      for (int i = 0; i < 20; i++) {
        for (int j = (i*5); j < (i*5) + 5; j++) {
          DataPoint pointHistogram = new DataPoint();
          pointHistogram.SetValueXY(j, buckets[i]);
          pointHistogram.ToolTip = string.Format("Count " + buckets[i].ToString() + " pass rate between " + (i * 5).ToString() + " and " + ((i + 1) * 5).ToString());
          seriesPassRateHistogram.Points.Add(pointHistogram);

          DataPoint pointLowerBollingerBand = new DataPoint();
          if (j == Convert.ToInt32(Math.Round(lowerBollingerBand))) {
            pointLowerBollingerBand.SetValueXY(j, maxBucket);
            pointLowerBollingerBand.ToolTip = string.Format("Lower Bollinger Band = " + Math.Round(lowerBollingerBand, 4).ToString());
          } else {
            pointLowerBollingerBand.SetValueXY(j, 0);
            pointLowerBollingerBand.BorderColor = System.Drawing.Color.Transparent;
            pointLowerBollingerBand.Color = System.Drawing.Color.Transparent;
          }
          seriesLowerBollingerBand.Points.Add(pointLowerBollingerBand);


          DataPoint pointPassRateStdDev = new DataPoint();
          if (j == Convert.ToInt32(Math.Round(passRateMean-passRateStdDev))) {
            pointPassRateStdDev.SetValueXY(j, maxBucket);
            pointPassRateStdDev.ToolTip = string.Format("Standard Deviation = " + Math.Round(passRateStdDev, 4).ToString());
          } else {
            pointPassRateStdDev.SetValueXY(j, 0);
            pointPassRateStdDev.BorderColor = System.Drawing.Color.Transparent;
            pointPassRateStdDev.Color = System.Drawing.Color.Transparent;
          }
          seriesPassRateStdDev.Points.Add(pointPassRateStdDev);
          DataPoint pointPassRateMean = new DataPoint();
          if (j == Convert.ToInt32(Math.Round(passRateMean))) {
            pointPassRateMean.SetValueXY(j, maxBucket);
            pointPassRateMean.ToolTip = string.Format("Mean = " + Math.Round(passRateMean, 4).ToString());
          } else {
            pointPassRateMean.SetValueXY(j, 0);
            pointPassRateMean.BorderColor = System.Drawing.Color.Transparent;
            pointPassRateMean.Color = System.Drawing.Color.Transparent;
          }
          seriesPassRateMean.Points.Add(pointPassRateMean);
        }
      }
      passRateChart.ChartAreas[0].AxisX.Interval = 5;
      passRateChart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
      passRateChart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(80, System.Drawing.Color.LightGray);
      passRateChart.ChartAreas[0].AxisY.Minimum = 0;
      passRateChart.ChartAreas[0].AxisY.Maximum = maxBucket;
      passRateChart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDotDot;
      passRateChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(80, System.Drawing.Color.Gray);
      passRateChart.ChartAreas[0].AxisY.LabelStyle.Format = "N2";
      passRateChart.Invalidate();
    }

    // Volatility chart...
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
      var seriesImpliedVolatility = new System.Windows.Forms.DataVisualization.Charting.Series {
        Name = "impliedVolatility",
        Color = Color.FromArgb(80, System.Drawing.Color.LightSteelBlue),
        BorderColor = System.Drawing.Color.DarkSlateBlue,
        BorderWidth = 1,
        IsVisibleInLegend = false,
        IsXValueIndexed = true,
        XValueType = ChartValueType.DateTime,
        ChartType = SeriesChartType.RangeColumn
      };
      passRateChart.Series.Add(seriesLowerBollingerBand);
      passRateChart.Series.Add(seriesPassRateStdDev);
      passRateChart.Series.Add(seriesTrendLine);
      passRateChart.Series.Add(seriesPassRateMean);
      passRateChart.Series.Add(seriesImpliedVolatility);
      double chartMinimum = lowerBollingerBand;
      double chartMaximum = 0.0;
      string chartLocation = Application.StartupPath + @"\taoGUI.resources\chart_" + projectRootFolder.Substring(projectRootFolder.LastIndexOf("\\") + 1) + "_" + dbInstance + "_" + taoSuite.Substring(0, taoSuite.IndexOf(".")) + ".tao";
      if (System.IO.File.Exists(chartLocation)) {
        using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(chartLocation)) {
          System.DateTime xTime = new System.DateTime();
          double taoTrendLine = 0.0;
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
            xTime = new System.DateTime(_year, _month, _day, _hour, _minute, 0);
            int taoTotalTests = Convert.ToInt32(fields[5]);
            int taoTestPass = Convert.ToInt32(fields[6]);
            taoTrendLine = (double)Math.Round(Convert.ToDouble(fields[7]), 2);
            if (taoTotalTests > 0) {
              DataPoint pointLowerBollingerBand = new DataPoint();
              pointLowerBollingerBand.SetValueXY(xTime.ToOADate(), lowerBollingerBand, (passRateMean - passRateStdDev));
              pointLowerBollingerBand.ToolTip = string.Format("Lower Bollinger Band = " + Math.Round(lowerBollingerBand, 4).ToString());
              seriesLowerBollingerBand.Points.Add(pointLowerBollingerBand);
              DataPoint pointPassRateStdDev = new DataPoint();
              pointPassRateStdDev.SetValueXY(xTime.ToOADate(), (passRateMean - passRateStdDev), passRateMean);
              pointPassRateStdDev.ToolTip = string.Format("Standard Deviation = " + Math.Round(passRateStdDev, 4).ToString());
              seriesPassRateStdDev.Points.Add(pointPassRateStdDev);
              // Transparency is necessary to maintain continuity...
              DataPoint pointImpliedVolatility = new DataPoint();
              pointImpliedVolatility.SetValueXY(xTime.ToOADate(), taoTrendLine, taoTrendLine);
              pointImpliedVolatility.BorderColor = System.Drawing.Color.Transparent;
              pointImpliedVolatility.Color = System.Drawing.Color.Transparent;
              seriesImpliedVolatility.Points.Add(pointImpliedVolatility);
              DataPoint pointTrendLine = new DataPoint();
              pointTrendLine.SetValueXY(xTime.ToOADate(), taoTrendLine);
              pointTrendLine.ToolTip = string.Format("{0}, {1}", xTime.ToShortDateString() + " " + xTime.ToShortTimeString(), taoTrendLine.ToString() + " % pass");
              seriesTrendLine.Points.Add(pointTrendLine);
              DataPoint pointPassRateMean = new DataPoint();
              pointPassRateMean.SetValueXY(xTime.ToOADate(), passRateMean);
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
          double _dailyVolatility = impliedVolatility / Math.Sqrt(10.0);
          double sampleVolatility = _dailyVolatility;
          xTime = System.DateTime.Now;
          for (int i=1; i < 11; i++) {
            xTime = xTime.AddDays(1.0);
            sampleVolatility = _dailyVolatility * Math.Sqrt((double)i);
            double lowerVolBand = taoTrendLine * (1.0 - (sampleVolatility / 100.0));
            double upperVolBand = taoTrendLine * (1.0 + (sampleVolatility / 100.0));
            if (lowerVolBand < 0.0) {
              lowerVolBand = 0.0;
            }
            if (upperVolBand > 100.0) {
              upperVolBand = 100.0;
            }
            if (chartMaximum < upperVolBand) {
              chartMaximum = upperVolBand;
            }
            if (chartMinimum > lowerVolBand) {
              chartMinimum = lowerVolBand;
            }
            // Transparency is necessary to maintain continuity...
            DataPoint pointLowerBollingerBand = new DataPoint();
            pointLowerBollingerBand.SetValueXY(xTime.ToOADate(), lowerBollingerBand, (passRateMean - passRateStdDev));
            pointLowerBollingerBand.BorderColor = System.Drawing.Color.Transparent;
            pointLowerBollingerBand.Color = System.Drawing.Color.Transparent;
            seriesLowerBollingerBand.Points.Add(pointLowerBollingerBand);
            DataPoint pointPassRateStdDev = new DataPoint();
            pointPassRateStdDev.SetValueXY(xTime.ToOADate(), (passRateMean - passRateStdDev), passRateMean);
            pointPassRateStdDev.BorderColor = System.Drawing.Color.Transparent;
            pointPassRateStdDev.Color = System.Drawing.Color.Transparent;
            seriesPassRateStdDev.Points.Add(pointPassRateStdDev);
            DataPoint pointTrendLine = new DataPoint();
            pointTrendLine.SetValueXY(xTime.ToOADate(), taoTrendLine);
            pointTrendLine.BorderColor = System.Drawing.Color.Transparent;
            pointTrendLine.Color = System.Drawing.Color.Transparent;
            seriesTrendLine.Points.Add(pointTrendLine);
            DataPoint pointPassRateMean = new DataPoint();
            pointPassRateMean.SetValueXY(xTime.ToOADate(), passRateMean);
            pointPassRateMean.BorderColor = System.Drawing.Color.Transparent;
            pointPassRateMean.Color = System.Drawing.Color.Transparent;
            seriesPassRateMean.Points.Add(pointPassRateMean);
            // Now the volatility bands to be visible here ...
            DataPoint pointImpliedVolatility = new DataPoint();
            pointImpliedVolatility.SetValueXY(xTime.ToOADate(), lowerVolBand, upperVolBand);
            pointImpliedVolatility.ToolTip = string.Format("Volatility (" + i.ToString() + " day) = " + Math.Round(sampleVolatility, 4).ToString());
            seriesImpliedVolatility.Points.Add(pointImpliedVolatility);
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

    // Simple line / bar chart of pass rate...
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
