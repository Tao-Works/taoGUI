using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace taoGUI {
  class taoReportCache {

    private string _projectRootFolder;
    private string _appId;
    private string _dbInstance;
    private string _cacheLocation;
    private string _taoSuiteInputFolder;
    private string _taoSuiteOutputFolder;
    private DataTable _cacheResults;
    private DataTable _actualResults;

    private DataTable initialiseCacheTable() {
      DataTable _tmpTable = new DataTable();
      _tmpTable.Columns.Add("TaoSuite", typeof(string));
      _tmpTable.Columns.Add("FirstRun", typeof(string));
      _tmpTable.Columns.Add("LastRun", typeof(string));
      _tmpTable.Columns.Add("Iterations", typeof(int));
      _tmpTable.Columns.Add("PassRate", typeof(double));
      _tmpTable.Columns.Add("PassDelta", typeof(double));
      // Strategy changes: get all data, perform stats, save results...
      // TO DO: add mean, standard deviation, normalised std. dev., future-implied volatility, moving average, MA std dev., upper & lower Bollinger bands.
      _tmpTable.Columns.Add("Volatility", typeof(double));
      return _tmpTable;
    }

    private void getCacheResults() {
      _cacheResults = initialiseCacheTable();
      if (System.IO.File.Exists(_cacheLocation)) {
        using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(_cacheLocation)) {
          parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
          parser.SetDelimiters(";");
          string[] fields = parser.ReadFields(); // Skip header...
          while (!parser.EndOfData) {
            fields = parser.ReadFields();
            _cacheResults.Rows.Add(fields[0], fields[1], fields[2], Convert.ToInt32(fields[3]), Convert.ToDouble(fields[4]), Convert.ToDouble(fields[5]), Convert.ToDouble(fields[6]));
          }
        }
      }
    }

    private void getTaoResults(string projectRootFolder, string suiteName, string dbInstance, DataTable tableTaoSuiteReports, out string outFirstKnownTaoGenerated, out string outLastKnownTaoGenerated, out int outTaoIterations, out double outPassRate, out double outPassRateDelta, out double outTaoSheetVolatility, out string outPassRateLocation, out string outPassDeltaLocation) {
      string taoSuiteOutputFolder = projectRootFolder + @"\taoSuite_Report";
      string filePattern = suiteName.Substring(0, suiteName.IndexOf(".")) + "*" + dbInstance + ".xls";
      string[] taoResults = System.IO.Directory.GetFiles(taoSuiteOutputFolder, filePattern);
      string firstKnownTaoGenerated = "9999-99-99_2359";
      string lastKnownTaoGenerated = "-";
      int taoIterations = 0;
      double taoPassRate = 0.0;
      double taoPassRateDelta = 0.0;
      double taoSheetVolatility = 0.0;
      string taoGenerated = "";
      string filenameForPassRateCalc = "";
      string filenameForPassRateDeltaCalc = "";
      foreach (string taoResult in taoResults) {
        taoGenerated = taoResult.Substring(taoResult.LastIndexOf("\\") + 1);
        taoGenerated = taoGenerated.Substring(taoGenerated.IndexOf(".") + 1);
        taoGenerated = taoGenerated.Substring(0, taoGenerated.IndexOf("."));
        if (String.Compare(taoGenerated, lastKnownTaoGenerated) > 0) {
          lastKnownTaoGenerated = taoGenerated;
          filenameForPassRateDeltaCalc = filenameForPassRateCalc;
          filenameForPassRateCalc = taoResult;
        }
        if (String.Compare(taoGenerated, firstKnownTaoGenerated) < 0) {
          firstKnownTaoGenerated = taoGenerated;
        }
        taoIterations++;
      }
      // Return values...
      if (firstKnownTaoGenerated.Equals("9999-99-99_2359")) {
        outFirstKnownTaoGenerated = "-";
      } else {
        firstKnownTaoGenerated = firstKnownTaoGenerated.Substring(0, firstKnownTaoGenerated.Length - 2) + ":" + firstKnownTaoGenerated.Substring(lastKnownTaoGenerated.Length - 2);
        outFirstKnownTaoGenerated = firstKnownTaoGenerated.Replace("_", " ");
      }
      if (!lastKnownTaoGenerated.Equals("-")) {
        lastKnownTaoGenerated = lastKnownTaoGenerated.Substring(0, lastKnownTaoGenerated.Length - 2) + ":" + lastKnownTaoGenerated.Substring(lastKnownTaoGenerated.Length - 2);
        lastKnownTaoGenerated = lastKnownTaoGenerated.Replace("_", " ");
        // Defer calculation of the pass-fail rates and volatility (standard deviation) until later...
      }
      outLastKnownTaoGenerated = lastKnownTaoGenerated;
      outTaoIterations = taoIterations;
      outPassRate = taoPassRate;
      outPassRateDelta = taoPassRateDelta;
      outTaoSheetVolatility = taoSheetVolatility;
      outPassRateLocation = filenameForPassRateCalc;
      outPassDeltaLocation = filenameForPassRateDeltaCalc;
    }

    private void getActualResults() {
      _actualResults = initialiseCacheTable();
      _actualResults.Columns.Add("PassRateLocation", typeof(string));
      _actualResults.Columns.Add("PassDeltaLocation", typeof(string));
      if (System.IO.Directory.Exists(_taoSuiteInputFolder)) {
        string[] fileEntries = System.IO.Directory.GetFiles(_taoSuiteInputFolder);
        foreach (string fileName in fileEntries) {
          string suiteName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
          string outFirstKnownTaoGenerated = "-";
          string outLastKnownTaoGenerated = "-";
          int outTaoIterations = 0;
          double outTaoPassRate = 0.0;
          double outTaoPassRateDelta = 0.0;
          double outTaoSheetVolatility = 0.0;
          string outPassRateLocation;
          string outPassDeltaLocation;
          getTaoResults(_projectRootFolder, suiteName, _dbInstance, _actualResults, out outFirstKnownTaoGenerated, out outLastKnownTaoGenerated, out outTaoIterations, out outTaoPassRate, out outTaoPassRateDelta, out outTaoSheetVolatility, out outPassRateLocation, out outPassDeltaLocation);
          _actualResults.Rows.Add(suiteName, outFirstKnownTaoGenerated, outLastKnownTaoGenerated, outTaoIterations, outTaoPassRate, outTaoPassRateDelta, outTaoSheetVolatility, outPassRateLocation, outPassDeltaLocation);
        }
      }
    }

    private void initialisedInternal(string projectRootFolder, string appId, string dbInstance) {
      _projectRootFolder = projectRootFolder;
      _appId = appId;
      _dbInstance = dbInstance;
      _cacheLocation = Application.StartupPath + @"\taoGUI.resources\tsrc_" + appId + "_" + dbInstance + ".tao";
      _taoSuiteInputFolder = _projectRootFolder + @"\taoSuite_Input";
      _taoSuiteOutputFolder = _projectRootFolder + @"\taoSuite_Report";
      getCacheResults();
      getActualResults();
    }

    public taoReportCache (string projectRootFolder, string appId, string dbInstance) {
      initialisedInternal(projectRootFolder, appId, dbInstance);
    }

    public bool isCacheCurrent() {
      bool cacheStatus = false;
      int cacheRowCount = _cacheResults.Rows.Count;
      int actualRowCount = _actualResults.Rows.Count;
      if (cacheRowCount == actualRowCount) {
        DataRow[] foundRows;
        int i = 0;
        bool allMatch = true;
        while (i < cacheRowCount && allMatch) {
          // Users could at any stage change the order of "cache" relative to "actual" hence nested search logic...
          foundRows = _actualResults.Select("TaoSuite = '" + _cacheResults.Rows[i]["TaoSuite"].ToString() + "'");
          if (foundRows.Length == 1) {
            if (!_cacheResults.Rows[i]["LastRun"].ToString().Equals(foundRows[0]["LastRun"]) || Convert.ToInt32(_cacheResults.Rows[i]["Iterations"].ToString()) != Convert.ToInt32(foundRows[0]["Iterations"])) {
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
      return _cacheResults;
    }

    private void persistCacheDataTable() {
      using (var sw = new System.IO.StreamWriter(_cacheLocation)) {
        string line;
        sw.WriteLine("TaoSuite;FirstRun;LastRun;Iterations;PassRate;PassDelta;Volatility");
        int totalRows = _cacheResults.Rows.Count;
        for (int i = 0; i < totalRows; i++) {
          line = _cacheResults.Rows[i]["TaoSuite"].ToString() + ";" +
                 _cacheResults.Rows[i]["FirstRun"].ToString() + ";" +
                 _cacheResults.Rows[i]["LastRun"].ToString() + ";" +
                 _cacheResults.Rows[i]["Iterations"].ToString() + ";" +
                 _cacheResults.Rows[i]["PassRate"].ToString() + ";" +
                 _cacheResults.Rows[i]["PassDelta"].ToString() + ";" +
                 _cacheResults.Rows[i]["Volatility"].ToString();
          sw.WriteLine(line);
        }
        sw.Flush();
        sw.Close();
      }
    }

    public void updateActualResults() {
      Cursor.Current = Cursors.WaitCursor;
      // Clear cache...
      _cacheResults.Dispose();
      _cacheResults = initialiseCacheTable();
      // Update actual values...
      taoProgressBar calcProgress = new taoProgressBar();
      calcProgress.Show();
      int actualRowCount = _actualResults.Rows.Count;
      int _upperLimit = calcProgress.getProgressUpperLimit();
      int _progressStep = 1;
      int _progressMax = actualRowCount * 2;
      for (int i = 0; i < actualRowCount; i++) {
        double taoPassRate = 0.0;
        double previousTaoPassRate = 0.0;
        calcProgress.setProgressDescription("Processing Tao Suite: " + _actualResults.Rows[i]["TaoSuite"].ToString() + " (" + (i+1).ToString() + " out of " + actualRowCount.ToString() + " total)" );
        int progressMeter = (int)((double)_progressStep / (double)_progressMax * (double)_upperLimit);
        calcProgress.setProgress(progressMeter);
        _progressStep++;
        // Pass rate calculation...
        calcProgress.setProgressAction(1, "1) Calculating pass rate ...");
        calcProgress.setProgressAction(2, "");
        calcProgress.Refresh();
        string targetFilename = _actualResults.Rows[i]["PassRateLocation"].ToString();
        if (targetFilename.Length > 0 && System.IO.File.Exists(targetFilename)) {
          taoReportReader lastKnownTao = new taoReportReader(targetFilename);
          taoPassRate = lastKnownTao.getOverallPassRate();
        }
        _actualResults.Rows[i]["PassRate"] = taoPassRate;
        calcProgress.setProgressAction(1, "1) Calculating pass rate ... DONE");
        progressMeter = (int)((double)_progressStep / (double)_progressMax * (double)_upperLimit);
        calcProgress.setProgress(progressMeter);
        _progressStep++;
        // Pass rate delta calculation...
        calcProgress.setProgressAction(2, "2) Calculating pass rate delta ...");
        calcProgress.Refresh();
        targetFilename = _actualResults.Rows[i]["PassDeltaLocation"].ToString();
        if (targetFilename.Length > 0 && System.IO.File.Exists(targetFilename)) {
          taoReportReader previousKnownTao = new taoReportReader(targetFilename);
          previousTaoPassRate = previousKnownTao.getOverallPassRate();
        }
        _actualResults.Rows[i]["PassDelta"] = taoPassRate - previousTaoPassRate;
        // Pass rate delta calculation...
        calcProgress.setProgressAction(2, "2) Calculating pass rate delta ... DONE");
        calcProgress.Refresh();
        // Update cache...
        _cacheResults.Rows.Add(
          _actualResults.Rows[i]["TaoSuite"].ToString(),
          _actualResults.Rows[i]["FirstRun"].ToString(),
          _actualResults.Rows[i]["LastRun"].ToString(),
          Convert.ToInt32(_actualResults.Rows[i]["Iterations"].ToString()),
          Convert.ToDouble(_actualResults.Rows[i]["PassRate"].ToString()),
          Convert.ToDouble(_actualResults.Rows[i]["PassDelta"].ToString()),
          Convert.ToDouble(_actualResults.Rows[i]["Volatility"].ToString()));
      }
      // Persist cache data
      persistCacheDataTable();
      calcProgress.Hide();
      calcProgress.Dispose();
      GC.Collect();
      Cursor.Current = Cursors.Default;
    }

    ~taoReportCache() {
      // Clean up...
    }

  }
}
