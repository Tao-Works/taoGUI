using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: InternalsVisibleTo("taoGUI_UnitTest")]

namespace taoGUI.Caching{

  /********************************************************************************
  */
  public interface FileCachableI {
    string[] getHeaderNames();
    Type[] getHeaderTypes();
  }

  /********************************************************************************
  */
  public class TaoSamplePoint : FileCachableI, IComparable {
    internal string suiteName = "";
    internal DateTime timeStamp = DateTime.Now;
    internal int totalTests = 0;
    internal int totalPass = 0;
    internal double passRate =0.0;

    private static string[] CSV_HeaderNames = { //
        "suiteName", "timeStamp", //
        "totalTests","totalPass" //
      };

    private static TaoSamplePoint s = new TaoSamplePoint();
    private static Type[] CSV_HeaderTypes = { //
          s.suiteName.GetType(), s.timeStamp.GetType(), //
          s.totalTests.GetType(), s.totalPass.GetType() //
        };

    internal static TaoSamplePoint create(string taoReportFileLocation) {
      // Read the date from the report file
      var fileNameData = ReportFileNameParser.parseReportFile(taoReportFileLocation);
      var taoSuiteResults = new TaoReportReader(taoReportFileLocation);
      return new TaoSamplePoint(//
        fileNameData.suiteName //
        , fileNameData.dateTimeOfReport //
        , taoSuiteResults.getTotalTests() //
        , taoSuiteResults.getTotalPass() //
      );
    }

    internal TaoSamplePoint(string suiteName, DateTime timeStamp, int totalTests, int totalPass) {
      this.suiteName = suiteName;
      this.timeStamp = timeStamp;
      this.totalTests = totalTests;
      this.totalPass = totalPass;
      if (totalTests > 0) {
        passRate = ((double)totalPass / (double)totalTests);
      }
    }

    private TaoSamplePoint() {
    }

    public string[] getHeaderNames() {
      return CSV_HeaderNames;
    }

    public Type[] getHeaderTypes() {
      return CSV_HeaderTypes;
    }

    private static TaoCacheReaderWriter CACHE_ReadWriter = new TaoCacheReaderWriter(s);

    public static void persist(string fileLocation, TaoSamplePoint v) {
      DataTable dataTable = CACHE_ReadWriter.createEmptyCacheTable();
      dataTable.Rows.Add(//
        v.suiteName, v.timeStamp, //
        v.totalTests, v.totalPass //
      );
      CACHE_ReadWriter.persistCacheDataTable(fileLocation, dataTable);
    }

    public static void persist(string fileLocation, List<TaoSamplePoint> list) {
      DataTable dataTable = CACHE_ReadWriter.createEmptyCacheTable();
      foreach (var v in list) {
        dataTable.Rows.Add(//
          v.suiteName, v.timeStamp, //
          v.totalTests, v.totalPass //
        );
      }
      CACHE_ReadWriter.persistCacheDataTable(fileLocation, dataTable);
    }

    public static List<TaoSamplePoint> loadList(string fileLocation) {
      List<TaoSamplePoint> result = new List<TaoSamplePoint>();
      DataTable dataTable = CACHE_ReadWriter.loadCachedResults(fileLocation);
      foreach (DataRow v in dataTable.Rows) {
        TaoSamplePoint t = new TaoSamplePoint(v["suiteName"].ToString(), Convert.ToDateTime(v["timeStamp"]), Convert.ToInt32(v["totalTests"]), Convert.ToInt32(v["pairsThatAreEqual"]));
        result.Add(t);
      }
      return result;
    }

    public int CompareTo(object obj) {
      TaoSamplePoint other = (TaoSamplePoint)obj;
      return this.timeStamp.CompareTo(other.timeStamp);
    }

    public override bool Equals(object obj) {
      TaoSamplePoint other = (TaoSamplePoint)obj;
      return ((obj != null) //
        && (obj is TaoSamplePoint) //
        && (this.suiteName.Equals(other.suiteName)) //
        && (this.totalTests.Equals(other.totalTests)) //
        && (this.totalPass.Equals(other.totalPass)) //
        );
    }

    public override int GetHashCode() {
      return this.suiteName.GetHashCode() //
        + this.timeStamp.GetHashCode()
        + this.totalTests.GetHashCode()
        + this.totalPass.GetHashCode()
        ;
    }
  }

  /********************************************************************************
 */
  public class TaoCacheReaderWriter {
    internal static string CSV_Seperator = ";";
    private FileCachableI classRepresentive;

    internal TaoCacheReaderWriter(FileCachableI classRepresentive) {
      this.classRepresentive = classRepresentive;
    }

    public DataTable loadCachedResults(string cacheFileLocation) {
      DataTable cacheResults = createEmptyCacheTable();
      if (!File.Exists(cacheFileLocation)) {
        throw new Exception("Unable to find Cache-Dir: " + cacheFileLocation);
      }
      TextFieldParser parser = new TextFieldParser(cacheFileLocation);
      parser.TextFieldType = FieldType.Delimited;
      parser.SetDelimiters(CSV_Seperator);
      string[] headerToCheck = parser.ReadFields();    // Look at header...
      if (!isCsvHeaderValid(headerToCheck)) {
        throw new Exception("Cache File has unexpected header. @Dave: Need to recreate the cache at this point");
      }
      string[] headerNames = classRepresentive.getHeaderNames();
      Type[] headerTypes = classRepresentive.getHeaderTypes();
      string[] csvStringFields;
      while (!parser.EndOfData) {
        var typedfields = new List<object>();
        csvStringFields = parser.ReadFields();
        for (int i = 0; i < headerNames.Length; i++) {
          Type toType = headerTypes[i];
          if (toType.Equals(typeof(int))) {
            typedfields.Add(Convert.ToInt32(csvStringFields[i]));
          } else if (toType.Equals(typeof(double))) {
            typedfields.Add(Convert.ToDouble(csvStringFields[i]));
          } else if (toType.Equals(typeof(DateTime))) {
            typedfields.Add(Convert.ToDateTime(csvStringFields[i]));
          } else {
            typedfields.Add(csvStringFields[i]);
          }
        }
        cacheResults.Rows.Add(typedfields);
      }
      return cacheResults;
    }

    public void persistCacheDataTable(string cacheFileLocation, DataTable cacheResults) {
      var txt = new StringBuilder();
      IEnumerable<string> columnNames = cacheResults.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
      txt.AppendLine(string.Join(";", columnNames));
      foreach (DataRow row in cacheResults.Rows) {
        IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
        txt.AppendLine(string.Join(";", fields));
      }
      StreamWriter sw = new StreamWriter(cacheFileLocation);
      sw.Write(txt.ToString());
      sw.Flush();
      sw.Close();
    }

    internal string getCsvHeader() {
      return string.Join(CSV_Seperator, classRepresentive.getHeaderNames());
    }

    internal bool isCsvHeaderValid(string[] headerToCheck) {
      string expectedHeaderCsv = string.Join(CSV_Seperator, classRepresentive.getHeaderNames());
      string fromCacheHeaderCsv = string.Join(CSV_Seperator, headerToCheck);
      return expectedHeaderCsv.Equals(fromCacheHeaderCsv);
    }

    internal DataTable createEmptyCacheTable() {
      DataTable tmpTable = new DataTable();
      string[] headerNames = classRepresentive.getHeaderNames();
      Type[] headerTypes = classRepresentive.getHeaderTypes();
      for (int i = 0; i < headerNames.Length; i++) {
        tmpTable.Columns.Add(headerNames[i], headerTypes[i]);
      }
      return tmpTable;
    }
  }


  /********************************************************************************
  * A Value Object (Vo) 
  */
  class TaoStatisticVo {
    // Main attributes (summary) ...
    public string suiteName;                      // Name of the project's Tao Suite
    public string taoSuiteFirstRun = "-";         // Date and time of the first Tao Suite execution at given DB instance
    public string taoSuiteLastRun = "-";          // Date and time of the last known Tao Suite execution at given DB instance
    public int taoSuiteIterations = 0;            // Number of times this Tao Suite has executed at given DB instance  
    public double passRate = 0.0;                 // Defined as number of passes / total tests * 100% (note, failed and ignored tests are implied)
    public double passRateDelta = 0.0;            // Defined as the absolute change from the previous pass rate to the current pass rate (absolute percentage)
    public double passRateMean = 0.0;             // Defined as the arithmetric mean of population of pass results /Tao Suite per DB instance)
    public double passRateStdDev = 0.0;           // Defined as the population standard deviation of pass rates (to arithmetric mean)
    public double lowerBollingerBand = 0.0;       // Defined as maximum( passRate - ( 2 * passRateStdDev ), 0 )
    public double upperBollingerBand = 0.0;       // Defined as minimum( passRate + ( 2 * passRateStdDev ), 100 )
    public double impliedVolatility = 0.0;        // Normalised standard deviation of the pass rate population (expected best guess future volatility)
                                                  // TODO: Dave add mean success rate = number of iterations where pass rate = 100% / total iterations
                                                  // TODO: Dave add mean success duration = average length of continuous success (100%)
                                                  // TODO: Dave add mean time to failure = when success, compare the "length of success" to the mean success duration,
                                                  //       combined with the mean succes rate to report probability of failure at next N number of iterations.


    // Helper attributes (samples) ...
    public string passRateLocation;               // Location of the latest Tao Suite Report (containing data for the pass rate calculation)
    public string passRateDeltaLocation;          // Location of the previous "last known" Tao Suite Report (so to calculate the pass rate delta)
    private List<TaoSamplePoint> filteredPoints;

    public TaoStatisticVo(string suiteName, List<TaoSamplePoint> rowSamplePoints) {
      this.suiteName = suiteName;
      rowSamplePoints.Sort();
      var filteredPoints = new List<TaoSamplePoint>();
      // filer empty tests
      foreach (var samplePoint in rowSamplePoints) {
        if (samplePoint.totalTests > 0) {
          this.filteredPoints.Add(samplePoint);
        }
      }
      this.filteredPoints = filteredPoints;
      this.taoSuiteIterations = filteredPoints.Count;
      TaoSamplePoint lastPoint = this.filteredPoints.Last();
      if (taoSuiteIterations > 0) {
        this.taoSuiteFirstRun = this.filteredPoints.First().timeStamp.ToString();
        this.taoSuiteLastRun = lastPoint.timeStamp.ToString();
      }
      this.passRate = getPassrate(lastPoint);
      if (taoSuiteIterations > 1) { // Calc deltas
        TaoSamplePoint beforeLastPoint = this.filteredPoints[taoSuiteIterations - 2];
        this.passRateDelta = passRate - getPassrate(beforeLastPoint);
      }
      double meanTemp = 0.0;
      foreach (var p in filteredPoints) {
        meanTemp += getPassrate(p);
      }

      this.passRateMean = meanTemp / (double)taoSuiteIterations;

      this.passRateStdDev = Math.Pow(this.passRate - this.passRateMean, 2) / (double)this.taoSuiteIterations;

      this.lowerBollingerBand = this.passRateMean - (2.0 * this.passRateStdDev);
      if (this.lowerBollingerBand < 0.0) {
        this.lowerBollingerBand = 0.0;
      }

      this.upperBollingerBand = this.passRateMean + (2.0 * this.passRateStdDev);
      if (this.upperBollingerBand > 100.0) {
        this.upperBollingerBand = 100.0;
      }
    }

    private double getPassrate(TaoSamplePoint p) {
      return (double)p.totalPass / (double)p.totalTests;
    }


    // !!! START - Caching , Sam: I don't think we want to cache the statistics as calculation is fast 
    /*
    private static string[] CSV_HeaderNames = { //
      "taoSuiteName", //
      "taoSuiteFirstRun", "taoSuiteLastRun", "taoSuiteIterations", //
      "passRate", "passRateDelta", "passRateMean", "passRateStdDev", //
      "lowerBollingerBand", "upperBollingerBand", //
      "impliedVolatility" //
    };

    private static TaoStatisticVo s = new TaoStatisticVo();
    private static Type[] CSV_HeaderTypes = { //
        s.suiteName.GetType(), //
        s.taoSuiteFirstRun.GetType(), s.taoSuiteLastRun.GetType(), s.taoSuiteIterations.GetType(), //
        s.passRate.GetType(), s.passRateDelta.GetType(), s.passRateMean.GetType(), s.passRateStdDev.GetType(), //
        s.lowerBollingerBand.GetType(), s.upperBollingerBand.GetType(), //
        s.impliedVolatility.GetType() //
      };

    public string[] getHeaderNames() {
      return CSV_HeaderNames;
    }

    public Type[] getHeaderTypes() {
      return CSV_HeaderTypes;
    }

    private TaoStatisticVo() {
    }

    private static TaoCacheReaderWriter CACHE_ReadWriter = new TaoCacheReaderWriter(s);

    public static void persist(string fileLocation, TaoStatisticVo v) {
      DataTable dataTable = CACHE_ReadWriter.createEmptyCacheTable();
      dataTable.Rows.Add(//
        v.suiteName, // 
        v.taoSuiteFirstRun, v.taoSuiteLastRun, v.taoSuiteIterations, //
        v.passRate, v.passRateDelta, v.passRateMean, v.passRateStdDev, //
        v.lowerBollingerBand, v.upperBollingerBand, //
        v.impliedVolatility //
        );
      CACHE_ReadWriter.persistCacheDataTable(fileLocation, dataTable);
    }
    */
    // !!! END - Caching , Sam: I don't think we want to cache the statistics as calculation is fast 
  }

  /********************************************************************************
   * A Value Object (Vo) 
   * Handle file name of format: 'generate_BOM_Output_01.2016-05-16_2310.Lecce.xls
   */
  internal class ReportFileNameParser {
    internal static string REGEX_ofReportFile = @"([^\.]*)\.(\d*-\d*-\d*_\d*)\.([^\.]*)\.([^\.]*)";
    internal static string FORMAT_YYYYMMDD_HHMM = "yyyy-MM-dd_HHmm";
    internal string taoReportFileLocation;
    internal string taoReportPath;
    internal string taoReportFileName;
    internal string suiteName;
    internal DateTime dateTimeOfReport;
    internal string dbInstance;
    internal string extention;

    internal static ReportFileNameParser parseReportFile(string taoReportFileLocation) {
      return new ReportFileNameParser(taoReportFileLocation);
    }

    private ReportFileNameParser(string taoReportFileLocation) {
      // Splitt file and path
      this.taoReportFileLocation = taoReportFileLocation;
      int startPos = taoReportFileLocation.LastIndexOf("\\");
      if (startPos > 0) {
        this.taoReportPath = taoReportFileLocation.Substring(0, startPos);
        this.taoReportFileName = taoReportFileLocation.Substring(startPos + 1);
      } else {
        this.taoReportPath = "";
        this.taoReportFileName = taoReportFileLocation;
      }

      // Analyze file name
      Match match = Regex.Match(taoReportFileName, REGEX_ofReportFile, RegexOptions.IgnoreCase);
      this.suiteName = match.Groups[1].Value;
      string dateValue = match.Groups[2].Value;
      if (!DateTime.TryParseExact(dateValue, FORMAT_YYYYMMDD_HHMM, null, DateTimeStyles.None, out this.dateTimeOfReport)) {
        throw new Exception("Unable to convert to a date and time: " + dateValue);
      }
      this.dbInstance = match.Groups[3].Value;
      this.extention = match.Groups[4].Value.ToLower();
    }

    public override string ToString() {
      string result = string.Format("[suiteName={0}, dateTimeOfReport={1}, dbInstance={2}, extention={3}]", suiteName, dateTimeOfReport.ToString("dd.MM.yyyy HH:mm"), dbInstance, extention);
      return result.ToLower();
    }
  }


  public class TaoCache {
    private TaoApplicationFolder taoFolders;        // Using the parameters passed, this holds the complete Tao folder locations (Tao suite input, output, cache)
                                                    //private DataTable cacheResults;                 // This represents the content of the current Tao Sute Report Cache
    private DataTable actualResults;                // This is how the actual Tao Suite Report statistics look (so to compare with cache)

    private struct TaoApplicationFolder {
      public string projectRootFolder;              // This is the project root folder containing all Tao applications and command scripts
      public string appId;                          // The Tao application reference (e.g. tao.baer.conf.emir for all EMIR related)
      public string dbInstance;                     // DB instance (like Oracle, H2, or some reference to a remote / cloud service)
      public string cacheLocation;                  // Location of the Tao Suite Report Cache (where summary statistics are cached)
      public string chartDataFolderPrefix;          // Folder prefix of the pass / fail bar chart cache (contains individual sample points as percentages)
      public string taoSuiteInputFolder;            // Location of the Tao Suites
      public string taoSuiteOutputFolder;           // Location of the Tao Suite Reports
    };


    /*
    private TaoStatisticVo createStatisticsFor(string suiteName) {
      string filePattern = suiteName.Substring(0, suiteName.IndexOf(".")) + "*" + taoFolders.dbInstance + ".xls";
      string[] taoResultFileNames = Directory.GetFiles(taoFolders.taoSuiteOutputFolder, filePattern);
      List<TaoSamplePoint> suiteaSamplePoints = new List<TaoSamplePoint>();
      foreach (string taoResultFileName in taoResultFileNames) {
        TaoSamplePoint samplePoint = TaoSamplePoint.create(taoResultFileName);
        suiteaSamplePoints.Add(samplePoint);
      }
      var result = new TaoStatisticVo(suiteName, suiteaSamplePoints);
      return result;
    }
    */


    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

/*
    private void getActualResults() {
      actualResults = createEmptyCacheTable();
      // Extensions the Tao Suite Report Cache structure (used in method updateActualResults)
      actualResults.Columns.Add("passRateLocation", typeof(string));
      actualResults.Columns.Add("passRateDeltaLocation", typeof(string));
      actualResults.Columns.Add("allTaoSuiteReports", typeof(List<string>));
      if (Directory.Exists(taoFolders.taoSuiteInputFolder)) {
        string[] fileEntries = Directory.GetFiles(taoFolders.taoSuiteInputFolder);
        TaoStatisticVo taoStatistics;
        foreach (string fileName in fileEntries) {
          taoStatistics = getTaoResults(taoFolders, fileName.Substring(fileName.LastIndexOf("\\") + 1));
          actualResults.Rows.Add(
            taoStatistics.suiteName,
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
      loadCachedResults(taoFolders.cacheLocation);
      getActualResults();
    }

    public TaoReportCache(string projectRootFolder, string appId, string dbInstance) {
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


    // This method will clear the current cache (in memory), loop through the actualResults and re-build the currentCache (persisting once updated).
    public void updateCacheResults() {
      Cursor.Current = Cursors.WaitCursor;
      cacheResults.Dispose();
      cacheResults = createEmptyCacheTable();
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
        calcProgress.setProgressDescription("Processing Tao Suite: " + r["taoSuiteName"].ToString() + " (" + (i + 1).ToString() + " out of " + actualRowCount.ToString() + " total)");
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
        List<string> tmpAllTaoSuiteReports = (List<string>)r["allTaoSuiteReports"];
        List<TaoSamplePoint> allSamplePoints = new List<TaoSamplePoint>();
        allSamplePoints.Clear();
        int actualSamplePointsUsed = 0;
        foreach (string taoReport in tmpAllTaoSuiteReports) {
          calcProgress.setProgressAction(3, "3) Calculating statistics ... sample point " + (actualSamplePointsUsed + 1).ToString() + " ...");
          calcProgress.Refresh();
          TaoSamplePoint tmpSamplePoint = new TaoSamplePoint(taoReport);
          if (tmpSamplePoint.totalTests > 0) {
            allSamplePoints.Add(tmpSamplePoint);
            actualSamplePointsUsed++;
          }
        }
        if (actualSamplePointsUsed > 0) {
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
              logMean = logReturn / ((double)logSamples + 1); // Still needs to be the arithmetic mean of log( returns ) ...
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
  */
  }
}
