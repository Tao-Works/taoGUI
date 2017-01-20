using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace taoGUI {
  class TaoReportCache {

    private struct TaoApplicationFolder {
      public string projectRootFolder;              // This is the project root folder containing all Tao applications and command scripts
      public string appId;                          // The Tao application reference (e.g. tao.baer.conf.emir for all EMIR related)
      public string dbInstance;                     // DB instance (like Oracle, H2, or some reference to a remote / cloud service)
      public string cacheLocation;                  // Location of the Tao Suite Report Cache (where summary statistics are cached)
      public string chartDataFolderPrefix;           // Folder prefix of the pass / fail bar chart cache (contains individual sample points as percentages)
      public string taoSuiteInputFolder;            // Location of the Tao Suites
      public string taoSuiteOutputFolder;           // Location of the Tao Suite Reports
    };

    private struct TaoSamplePoint {

      // To be used later e.g. System.DateTime x = new System.DateTime(_year, _month, _day, _hour, _minute, 0);
      public int sampleYear;
      public int sampleMonth;
      public int sampleDay;
      public int sampleHour;
      public int sampleMinute;

      // To be used in calculations of mean, standard deviation, implied volatility and Bollinger bands
      public int totalTests;
      public int totalPass;
      public double overallPassRate;

      // Helper methods (getters, setters, etc.)
      public TaoSamplePoint( string sampleResultFile ) {

        string tmpTaoGenerated = sampleResultFile.Substring(sampleResultFile.LastIndexOf("\\") + 1);
        tmpTaoGenerated = tmpTaoGenerated.Substring(tmpTaoGenerated.IndexOf(".") + 1);
        tmpTaoGenerated = tmpTaoGenerated.Substring(0, tmpTaoGenerated.IndexOf("."));

        sampleYear = Convert.ToInt32(tmpTaoGenerated.Substring(0, 4));
        sampleMonth = Convert.ToInt32(tmpTaoGenerated.Substring(5, 2));
        sampleDay = Convert.ToInt32(tmpTaoGenerated.Substring(8, 2));
        sampleHour = Convert.ToInt32(tmpTaoGenerated.Substring(11, 2));
        sampleMinute = Convert.ToInt32(tmpTaoGenerated.Substring(13, 2));

        TaoReportReader taoSuiteResults = new TaoReportReader(sampleResultFile);

        totalTests = taoSuiteResults.getTotalTests();
        totalPass = taoSuiteResults.getPairsThatAreEqual();
        overallPassRate = taoSuiteResults.getOverallPassRate();

      }

    }

    private struct TaoStatistic {

      // Main attributes (summary) ...
      public string taoSuiteName;                   // Name of the project's Tao Suite
      public string taoSuiteFirstRun;               // Date and time of the first Tao Suite execution at given DB instance
      public string taoSuiteLastRun;                // Date and time of the last known Tao Suite execution at given DB instance
      public int    taoSuiteIterations;             // Number of times this Tao Suite has executed at given DB instance  
      public double passRate;                       // Defined as number of passes / total tests * 100% (note, failed and ignored tests are implied)
      public double passRateDelta;                  // Defined as the absolute change from the previous pass rate to the current pass rate (absolute percentage)
      public double passRateMean;                   // Defined as the arithmetric mean of population of pass results /Tao Suite per DB instance)
      public double passRateStdDev;                 // Defined as the population standard deviation of pass rates (to arithmetric mean)
      public double lowerBollingerBand;             // Defined as maximum( passRate - ( 2 * passRateStdDev ), 0 )
      public double upperBollingerBand;             // Defined as minimum( passRate + ( 2 * passRateStdDev ), 100 )
      public double impliedVolatility;              // Normalised standard deviation of the pass rate population (expected best guess future volatility)

      // Helper attributes (samples) ...
      public string passRateLocation;               // Location of the latest Tao Suite Report (containing data for the pass rate calculation)
      public string passRateDeltaLocation;          // Location of the previous "last known" Tao Suite Report (so to calculate the pass rate delta)
      public List<string> allTaoSuiteReports;       // Collection of all related Tao Suite Reports (containing raw sample points)

      // Helper methods...
      public void initialise(string suiteName) {
        taoSuiteName = suiteName;
        taoSuiteFirstRun = "-";
        taoSuiteLastRun = "-";
        taoSuiteIterations = 0;
        passRate = 0.0;
        passRateDelta = 0.0;
        passRateMean = 0.0;
        passRateStdDev = 0.0;
        lowerBollingerBand = 0.0;
        upperBollingerBand = 0.0;
        impliedVolatility = 0.0;
      }

    };

    private TaoApplicationFolder taoFolders;        // Using the parameters passed, this holds the complete Tao folder locations (Tao suite input, output, cache)
    private DataTable cacheResults;                 // This represents the content of the current Tao Sute Report Cache
    private DataTable actualResults;                // This is how the actual Tao Suite Report statistics look (so to compare with cache)

    private DataTable initialiseCacheTable() {
      DataTable tmpTable = new DataTable();
      tmpTable.Columns.Add("taoSuiteName", typeof(string));
      tmpTable.Columns.Add("taoSuiteFirstRun", typeof(string));
      tmpTable.Columns.Add("taoSuiteLastRun", typeof(string));
      tmpTable.Columns.Add("taoSuiteIterations", typeof(int));
      tmpTable.Columns.Add("passRate", typeof(double));
      tmpTable.Columns.Add("passRateDelta", typeof(double));
      tmpTable.Columns.Add("passRateMean", typeof(double));
      tmpTable.Columns.Add("passRateStdDev", typeof(double));
      tmpTable.Columns.Add("lowerBollingerBand", typeof(double));
      tmpTable.Columns.Add("upperBollingerBand", typeof(double));
      tmpTable.Columns.Add("impliedVolatility", typeof(double));
      return tmpTable;
    }

    private void getCacheResults() {
      cacheResults = initialiseCacheTable();
      if (File.Exists(taoFolders.cacheLocation)) {
        using (TextFieldParser parser = new TextFieldParser(taoFolders.cacheLocation)) {
          parser.TextFieldType = FieldType.Delimited;
          parser.SetDelimiters(";");
          string[] fields = parser.ReadFields();    // Skip header...
          while (!parser.EndOfData) {
            fields = parser.ReadFields();
            cacheResults.Rows.Add(
              fields[0],                            // taoSuiteName
              fields[1],                            // taoSuiteFirstRun
              fields[2],                            // taoSuiteLastRun
              Convert.ToInt32(fields[3]),           // taoSuiteIterations
              Convert.ToDouble(fields[4]),          // passRate
              Convert.ToDouble(fields[5]),          // passRateDelta
              Convert.ToDouble(fields[6]),          // passRateMean
              Convert.ToDouble(fields[7]),          // passRateStdDev
              Convert.ToDouble(fields[8]),          // lowerBollingerBand
              Convert.ToDouble(fields[9]),          // upperBollingerBand
              Convert.ToDouble(fields[10]));        // impliedVolatility
          }
        }
      }
    }

    private void getTaoResults(TaoApplicationFolder taoFolders, string suiteName, out TaoStatistic taoStatistics) {

      TaoStatistic tmpStatistics = new TaoStatistic();
      tmpStatistics.initialise(suiteName);

      string filePattern = suiteName.Substring(0, suiteName.IndexOf(".")) + "*" + taoFolders.dbInstance + ".xls";
      string[] taoResults = Directory.GetFiles(taoFolders.taoSuiteOutputFolder, filePattern);

      string tmptaoSuiteReport = "";
      string tmpTaoSuiteFirstRun = "9999-99-99_2359";
      string tmpTaoSuiteLastRun = "-";
      int    tmpTaoSuiteIterations = 0;
      string tmpPassRateLocation = "";
      string tmpPassRateDeltaLocation = "";
      List<string> tmpAllTaoSuiteReports = new List<string>();

      foreach (string taoResult in taoResults) {
        tmpAllTaoSuiteReports.Add(taoResult);
        tmptaoSuiteReport = taoResult.Substring(taoResult.LastIndexOf("\\") + 1);
        tmptaoSuiteReport = tmptaoSuiteReport.Substring(tmptaoSuiteReport.IndexOf(".") + 1);
        tmptaoSuiteReport = tmptaoSuiteReport.Substring(0, tmptaoSuiteReport.IndexOf("."));
        if (String.Compare(tmptaoSuiteReport, tmpTaoSuiteLastRun) > 0) {
          tmpTaoSuiteLastRun = tmptaoSuiteReport;
          tmpPassRateDeltaLocation = tmpPassRateLocation;
          tmpPassRateLocation = taoResult;
        }
        if (String.Compare(tmptaoSuiteReport, tmpTaoSuiteFirstRun) < 0) {
          tmpTaoSuiteFirstRun = tmptaoSuiteReport;
        }
        tmpTaoSuiteIterations++;
      }

      if (tmpTaoSuiteFirstRun.Equals("9999-99-99_2359")) {
        tmpStatistics.taoSuiteFirstRun = "-";
      } else {
        tmpTaoSuiteFirstRun = tmpTaoSuiteFirstRun.Substring(0, tmpTaoSuiteFirstRun.Length - 2) + ":" + tmpTaoSuiteFirstRun.Substring(tmpTaoSuiteLastRun.Length - 2);
        tmpStatistics.taoSuiteFirstRun = tmpTaoSuiteFirstRun.Replace("_", " ");
      }
      if (!tmpTaoSuiteLastRun.Equals("-")) {
        tmpTaoSuiteLastRun = tmpTaoSuiteLastRun.Substring(0, tmpTaoSuiteLastRun.Length - 2) + ":" + tmpTaoSuiteLastRun.Substring(tmpTaoSuiteLastRun.Length - 2);
        tmpTaoSuiteLastRun = tmpTaoSuiteLastRun.Replace("_", " ");
      }
      tmpStatistics.taoSuiteLastRun = tmpTaoSuiteLastRun;
      tmpStatistics.taoSuiteIterations = tmpTaoSuiteIterations;
      tmpStatistics.passRateLocation = tmpPassRateLocation;
      tmpStatistics.passRateDeltaLocation = tmpPassRateDeltaLocation;
      tmpStatistics.allTaoSuiteReports = tmpAllTaoSuiteReports;
      taoStatistics = tmpStatistics;
    }

    private void getActualResults() {
      actualResults = initialiseCacheTable();
      // Extensions the Tao Suite Report Cache structure (used in method updateActualResults)
      actualResults.Columns.Add("passRateLocation", typeof(string));
      actualResults.Columns.Add("passRateDeltaLocation", typeof(string));
      actualResults.Columns.Add("allTaoSuiteReports", typeof(List<string>));
      if (Directory.Exists(taoFolders.taoSuiteInputFolder)) {
        string[] fileEntries = Directory.GetFiles(taoFolders.taoSuiteInputFolder);
        TaoStatistic taoStatistics = new TaoStatistic();
        foreach (string fileName in fileEntries) {
          getTaoResults(taoFolders, fileName.Substring(fileName.LastIndexOf("\\") + 1), out taoStatistics);
          actualResults.Rows.Add(
            taoStatistics.taoSuiteName,
            taoStatistics.taoSuiteFirstRun,
            taoStatistics.taoSuiteLastRun,
            taoStatistics.taoSuiteIterations,
            taoStatistics.passRate,
            taoStatistics.passRateDelta,
            taoStatistics.passRateMean,
            taoStatistics.passRateStdDev,
            taoStatistics.lowerBollingerBand,
            taoStatistics.upperBollingerBand,
            taoStatistics.impliedVolatility,
            taoStatistics.passRateLocation,
            taoStatistics.passRateDeltaLocation,
            taoStatistics.allTaoSuiteReports);
        }
      }
    }

    private void initialisedInternal(string projectRootFolder, string appId, string dbInstance) {
      taoFolders = new TaoApplicationFolder();
      taoFolders.projectRootFolder = projectRootFolder;
      taoFolders.appId = appId;
      taoFolders.dbInstance = dbInstance;
      taoFolders.cacheLocation = Application.StartupPath + @"\taoGUI.resources\tsrc_" + appId + "_" + dbInstance + ".tao"; // (t)ao (s)uite (r)eport (c)ache
      taoFolders.chartDataFolderPrefix = Application.StartupPath + @"\taoGUI.resources\chart_" + appId + "_" + dbInstance + "_";
      taoFolders.taoSuiteInputFolder = taoFolders.projectRootFolder + @"\taoSuite_Input";
      taoFolders.taoSuiteOutputFolder = taoFolders.projectRootFolder + @"\taoSuite_Report";
      getCacheResults();
      getActualResults();
    }

    public TaoReportCache (string projectRootFolder, string appId, string dbInstance) {
      initialisedInternal(projectRootFolder, appId, dbInstance);
    }

    public bool isCacheCurrent() {
      bool cacheStatus = false;
      int cacheRowCount = cacheResults.Rows.Count;
      int actualRowCount = actualResults.Rows.Count;
      if (cacheRowCount == actualRowCount) {
        DataRow[] foundRows;
        int i = 0;
        bool allMatch = true;
        while (i < cacheRowCount && allMatch) {
          // Users could at any stage change the order of "cache" relative to "actual" hence nested search logic...
          foundRows = actualResults.Select("taoSuiteName = '" + cacheResults.Rows[i]["taoSuiteName"].ToString() + "'");
          if (foundRows.Length == 1) {
            // Due to potential discrepancies between non-zero sample points and file counts -- do not include iterations as part of the calculation...
            if (!cacheResults.Rows[i]["taoSuiteLastRun"].ToString().Equals(foundRows[0]["taoSuiteLastRun"])) {
              allMatch = false;
            } else {
              i++;
            }
          } else {
            allMatch = false;
          }
        }
        cacheStatus = allMatch;
      }
      return cacheStatus;
    }

    public DataTable getCacheDataTable() {
      return cacheResults;
    }

    private void persistCacheDataTable() {
      using (var sw = new StreamWriter(taoFolders.cacheLocation)) {
        string line;
        sw.WriteLine("taoSuiteName;taoSuiteFirstRun;taoSuiteLastRun;taoSuiteIterations;passRate;passRateDelta;passRateMean;passRateStdDev;lowerBollingerBand;upperBollingerBand;impliedVolatility");
        int totalRows = cacheResults.Rows.Count;
        for (int i = 0; i < totalRows; i++) {
         var r = cacheResults.Rows[i];
          line = r["taoSuiteName"].ToString() + ";" +
                 r["taoSuiteFirstRun"].ToString() + ";" +
                 r["taoSuiteLastRun"].ToString() + ";" +
                 r["taoSuiteIterations"].ToString() + ";" +
                 r["passRate"].ToString() + ";" +
                 r["passRateDelta"].ToString() + ";" +
                 r["passRateMean"].ToString() + ";" +
                 r["passRateStdDev"].ToString() + ";" +
                 r["lowerBollingerBand"].ToString() + ";" +
                 r["upperBollingerBand"].ToString() + ";" +
                 r["impliedVolatility"].ToString();
          sw.WriteLine(line);
        }
        sw.Flush();
        sw.Close();
      }
    }

    private void persistSampleData(string taoSuiteName, List<TaoSamplePoint> allSamplePoints) {
      string chartLocation = taoFolders.chartDataFolderPrefix + taoSuiteName.Substring(0, taoSuiteName.IndexOf(".")) + ".tao";
      using (var sw = new StreamWriter(chartLocation)) {
        string line;
        sw.WriteLine("sampleYear;sampleMonth;sampleDay;sampleHour;sampleMinute;totalTests;totalPass;overallPassRate");
        foreach (TaoSamplePoint tmpSamplePoint in allSamplePoints) {
          line = tmpSamplePoint.sampleYear.ToString() + ";" +
                 tmpSamplePoint.sampleMonth.ToString() + ";" +
                 tmpSamplePoint.sampleDay.ToString() + ";" +
                 tmpSamplePoint.sampleHour.ToString() + ";" +
                 tmpSamplePoint.sampleMinute.ToString() + ";" +
                 tmpSamplePoint.totalTests.ToString() + ";" +
                 tmpSamplePoint.totalPass.ToString() + ";" +
                 tmpSamplePoint.overallPassRate.ToString();
          sw.WriteLine(line);
        }
        sw.Flush();
        sw.Close();
      }
    }

    // This method will clear the current cache (in memory), loop through the actualResults and re-build the currentCache (persisting once updated).
    public void updateCacheResults() {
      Cursor.Current = Cursors.WaitCursor;
      cacheResults.Dispose();
      cacheResults = initialiseCacheTable();
      taoProgressBar calcProgress = new taoProgressBar();
      calcProgress.Show();
      int actualRowCount = actualResults.Rows.Count;
      int progressUpperLimit = calcProgress.getProgressUpperLimit();
      int progressStep = 1;
      int progressMaxSteps = actualRowCount * 3;
      for (int i = 0; i < actualRowCount; i++) {
        var r = actualResults.Rows[i];
        double taoPassRate = 0.0;
        double previousTaoPassRate = 0.0;
        calcProgress.setProgressDescription("Processing Tao Suite: " + r["taoSuiteName"].ToString() + " (" + (i+1).ToString() + " out of " + actualRowCount.ToString() + " total)" );
        int progressSoFar = (int)((double)progressStep / (double)progressMaxSteps * (double)progressUpperLimit);
        calcProgress.setProgress(progressSoFar);
        progressStep++;
        calcProgress.setProgressAction(1, "1) Calculating pass rate ...");
        calcProgress.setProgressAction(2, "");
        calcProgress.setProgressAction(3, "");
        calcProgress.Refresh();
        string targetFilename = r["passRateLocation"].ToString();
        if (targetFilename.Length > 0 && File.Exists(targetFilename)) {
          TaoReportReader lastKnownTao = new TaoReportReader(targetFilename);
          taoPassRate = lastKnownTao.getOverallPassRate();
        }
        r["passRate"] = taoPassRate;
        calcProgress.setProgressAction(1, "1) Calculating pass rate ... DONE");
        progressSoFar = (int)((double)progressStep / (double)progressMaxSteps * (double)progressUpperLimit);
        calcProgress.setProgress(progressSoFar);
        progressStep++;
        calcProgress.setProgressAction(2, "2) Calculating pass rate delta ...");
        calcProgress.Refresh();
        targetFilename = r["passRateDeltaLocation"].ToString();
        if (targetFilename.Length > 0 && File.Exists(targetFilename)) {
          TaoReportReader previousKnownTao = new TaoReportReader(targetFilename);
          previousTaoPassRate = previousKnownTao.getOverallPassRate();
        }
        r["passRateDelta"] = taoPassRate - previousTaoPassRate;
        calcProgress.setProgressAction(2, "2) Calculating pass rate delta ... DONE");
        progressSoFar = (int)((double)progressStep / (double)progressMaxSteps * (double)progressUpperLimit);
        calcProgress.setProgress(progressSoFar);
        progressStep++;
        calcProgress.setProgressAction(3, "3) Calculating statistics ...");
        calcProgress.Refresh();
        List <string> tmpAllTaoSuiteReports = (List<string>)r["allTaoSuiteReports"];
        List<TaoSamplePoint> allSamplePoints = new List<TaoSamplePoint>();
        allSamplePoints.Clear();
        int actualSamplePointsUsed = 0;
        foreach (string taoReport in tmpAllTaoSuiteReports) {
          calcProgress.setProgressAction(3, "3) Calculating statistics ... sample point " + (actualSamplePointsUsed+1).ToString() + " ...");
          calcProgress.Refresh();
          TaoSamplePoint tmpSamplePoint = new TaoSamplePoint(taoReport);
          if (tmpSamplePoint.totalTests > 0) {
            allSamplePoints.Add(tmpSamplePoint);
            actualSamplePointsUsed++;
          }
        }
        if (actualSamplePointsUsed > 0 ) {
          r["passRateMean"] = allSamplePoints.Sum(Item => Item.overallPassRate) / (double)actualSamplePointsUsed;
          r["passRateStdDev"] = Math.Sqrt(allSamplePoints.Sum(Item => Math.Pow((Item.overallPassRate - Convert.ToDouble(r["passRateMean"].ToString())), 2)) / (double)actualSamplePointsUsed);
          double lowerBollingerBand = Convert.ToDouble(r["passRateMean"].ToString()) - (2.0 * Convert.ToDouble(r["passRateStdDev"].ToString()));
          if (lowerBollingerBand < 0.0) {
            lowerBollingerBand = 0.0;
          }
          r["lowerBollingerBand"] = lowerBollingerBand;
          double upperBollingerBand = Convert.ToDouble(r["passRateMean"].ToString()) + (2.0 * Convert.ToDouble(r["passRateStdDev"].ToString()));
          if (upperBollingerBand > 100.0) {
            upperBollingerBand = 100.0;
          }
          r["upperBollingerBand"] = upperBollingerBand;
          // Define volatiltiy as FILTERED standard deviation of log returns, appropriately adjusted to "10 iterations forward" and converted to percentage (Black-Scholes style volatility)
          // TODO: Apply linear regression and Cooks Distance to correctly FILTER outliers (for now filter the samples using Bollinger bands)
          if (actualSamplePointsUsed > 1) {
            double previousSample = 0.0;
            double currentSample = 0.0;
            double logReturn = 0.0;
            double logMean = 0.0;
            int logSamples = 0;
            foreach (TaoSamplePoint tmpSamplePoint in allSamplePoints) {
              if (tmpSamplePoint.overallPassRate > 0.0 && lowerBollingerBand <= tmpSamplePoint.overallPassRate && tmpSamplePoint.overallPassRate <= upperBollingerBand) {
                previousSample = currentSample;
                currentSample = tmpSamplePoint.overallPassRate;
              }
              if (previousSample > 0.0) {
                logReturn = logReturn + Math.Log(currentSample / previousSample); // Basis of volatility are the "relative returns" ...
                logSamples++;
              }
            }
            // Assume the results have been filtered and therefore standard deviation is of a SAMPLE (not the FULL POPULATION) set.
            logSamples--;
            if (logSamples > 0) {
              logMean = logReturn / ((double)logSamples+1); // Still needs to be the arithmetic mean of log( returns ) ...
              previousSample = 0.0;
              currentSample = 0.0;
              logReturn = 0.0;
              foreach (TaoSamplePoint tmpSamplePoint in allSamplePoints) {
                if (tmpSamplePoint.overallPassRate > 0.0 && lowerBollingerBand <= tmpSamplePoint.overallPassRate && tmpSamplePoint.overallPassRate <= upperBollingerBand) {
                  previousSample = currentSample;
                  currentSample = tmpSamplePoint.overallPassRate;
                }
                if (previousSample > 0.0) {
                  logReturn = logReturn + Math.Pow(Math.Log(currentSample / previousSample) - logMean, 2);
                }
              }
              // Assumption: adjust the future implied to reflect 10 iterations forward (key iteration points 10, 20, 50, 100, 200).
              r["impliedVolatility"] = Math.Sqrt(logReturn / (double)logSamples) * Math.Sqrt(10.0) * 100.0;
            }
          }
        }
        // The calcualtion of taoSuiteIterations is based on file count and not the actual number of tests. It can be therefore, that
        // the actual number of tests performed is less than the file count.  In this case, we need to correct the taoSuiteIterations
        // from earlier assumptions.
        r["taoSuiteIterations"] = actualSamplePointsUsed;
        calcProgress.setProgressAction(3, "3) Calculating statistics ... DONE");
        calcProgress.Refresh();
        cacheResults.Rows.Add(
          r["taoSuiteName"].ToString(),
          r["taoSuiteFirstRun"].ToString(),
          r["taoSuiteLastRun"].ToString(),
          Convert.ToInt32(r["taoSuiteIterations"].ToString()),
          Convert.ToDouble(r["passRate"].ToString()),
          Convert.ToDouble(r["passRateDelta"].ToString()),
          Convert.ToDouble(r["passRateMean"].ToString()),
          Convert.ToDouble(r["passRateStdDev"].ToString()),
          Convert.ToDouble(r["lowerBollingerBand"].ToString()),
          Convert.ToDouble(r["upperBollingerBand"].ToString()),
          Convert.ToDouble(r["impliedVolatility"].ToString()));
        persistSampleData(r["taoSuiteName"].ToString(), allSamplePoints);
      }
      // Persist cache data
      persistCacheDataTable();
      calcProgress.Hide();
      calcProgress.Dispose();
      GC.Collect();
      Cursor.Current = Cursors.Default;
    }

  }
}
