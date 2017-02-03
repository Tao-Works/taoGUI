using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: InternalsVisibleTo("taoGUI_UnitTest")]

namespace taoGUI.Caching {

  /******************************************************************************** 
  */
  public interface TaoStatAccessorI {
    TaoStatisticVo getStats(string suiteName, string dbInstance);
  }

  /******************************************************************************** 
  */
  public static class TaoAccessorFactory {
    public enum AccessType { WithCache, NoCache };

    public static TaoStatAccessorI getStatAccessor(AccessType accT, string projectRootDir, string appId) {
      TaoStatAccessorI result = null;
      switch (accT) {
        case AccessType.NoCache:
          result = new TaoStatAccessor(projectRootDir, appId);
          break;
        case AccessType.WithCache:
          var statAcc=  new TaoStatAccessor(projectRootDir, appId);
          result = TaoStatAccessor_WithCache.wrap(statAcc);
          break;
        default:
          throw new Exception("Unhandelde switch type :" + accT);
      }
      return result;
    }
  }

  /********************************************************************************
   * This is the normal Statistics Accessor WITHOUT any caching
   * 
   ********************************************************************************/
  public class TaoStatAccessor : TaoStatAccessorI {
    internal string projectRootFolder { get; }  // This is the project root folder containing all Tao applications and command scripts
    internal string taoSuiteReportDir { get; }    // Location of the Tao Suite Reports
    internal string appId { get; }    // The Tao application reference (e.g. tao.baer.conf.emir for all EMIR related)

    /**
     */
    internal TaoStatAccessor(string projectRootDir, string appId) {
      this.appId = appId;
      this.projectRootFolder = projectRootDir;
      this.taoSuiteReportDir = this.projectRootFolder + @"\taoSuite_Report\";
    }

    /**
     */
    public TaoStatisticVo getStats(string suiteName, string dbInstance) {
      // a) Fetch all XL-report files
      HashSet<FileInfo> allXlReportSet = getXlReports(suiteName, dbInstance);

      List<TaoSamplePoint> suiteSamplePoints = new List<TaoSamplePoint>();
      foreach (FileInfo reportFi in allXlReportSet) {
        TaoSamplePoint sp = TaoSamplePoint.create(reportFi);
        suiteSamplePoints.Add(sp);
      }

      var result = new TaoStatisticVo(suiteName, suiteSamplePoints);
      return result;
    }

    /**
     */
    internal HashSet<FileInfo> getXlReports(string suiteName, string dbInstance) {
      var result = new HashSet<FileInfo>();
      int suiteNameEnd = suiteName.IndexOf(".");
      string filePattern = suiteNameEnd > 0 ? suiteName.Substring(0, suiteNameEnd) : suiteName;
      filePattern += "*." + dbInstance + ".xls";
      var di = new DirectoryInfo(this.taoSuiteReportDir);
      foreach (FileInfo fi in di.GetFiles(filePattern)) {
        result.Add(fi);
      }
      return result;
    }

  }

  /********************************************************************************
   * This is the normal Statistics Accessor WITH caching
   * 
   ********************************************************************************/
  public class TaoStatAccessor_WithCache : TaoStatAccessorI {
    private static string EXT_NAME_Cache = @"~";
    private static string DIR_NAME_Cache = @"~taoGuiCache\";
    private TaoStatAccessor taoStatAccessor_NoCache;
    private string cacheDir;                  // Location of the Tao Suite Report Cache (where summary statistics are cached)


    public static TaoStatAccessorI wrap(TaoStatAccessor taoStatAccessor_NoCache) {
      return new TaoStatAccessor_WithCache(taoStatAccessor_NoCache);
    }

    private TaoStatAccessor_WithCache(TaoStatAccessor taoStatAccessor_NoCache) {
      this.taoStatAccessor_NoCache = taoStatAccessor_NoCache;
      this.cacheDir = taoStatAccessor_NoCache.taoSuiteReportDir + DIR_NAME_Cache;
    }

    public TaoStatisticVo getStats(string suiteName, string dbInstance) {
      // a) Fetch all XL-report files
      var foundXlReportSet = taoStatAccessor_NoCache.getXlReports(suiteName, dbInstance);
      // b) Fetch all existing report-chache files
      var reportCacheMap = new Dictionary<string, FileInfo>();
      var di = new DirectoryInfo(this.cacheDir);
      if (di.Exists) {
        foreach (FileInfo cacheFileInfo in di.GetFiles("*" + EXT_NAME_Cache)) {
          string cacheName = cacheFileInfo.Name;
          string orgName = cacheName.Substring(0, cacheName.Length - EXT_NAME_Cache.Length);
          reportCacheMap.Add(orgName, cacheFileInfo);
        }
      }
      // c) Put XL-files without (valid) cache in a Map and all valid caches in a Set
      var xLReportsWith_NO_cache = new HashSet<FileInfo>();
      var validCacheSet = new HashSet<FileInfo>();
      foreach (FileInfo xlFileInfo in foundXlReportSet) {
        if (reportCacheMap.ContainsKey(xlFileInfo.Name)) {
          FileInfo cacheFi = reportCacheMap[xlFileInfo.Name];
          // Cache is assumed to valid if it's younger then the corresponding XL-file
          if (xlFileInfo.LastWriteTime < cacheFi.LastWriteTime) {
            validCacheSet.Add(cacheFi);
          } else { // Cache is not valid any more
            xLReportsWith_NO_cache.Add(xlFileInfo);
          }
        } else { // No cache found at all
          xLReportsWith_NO_cache.Add(xlFileInfo);
        }
      }

      List<TaoSamplePoint> suiteaSamplePoints = new List<TaoSamplePoint>();
      foreach (FileInfo taoResultFileName in xLReportsWith_NO_cache) {
        TaoSamplePoint sp = TaoSamplePoint.create(taoResultFileName);
        string cacheFileLocation = this.cacheDir + taoResultFileName.Name + EXT_NAME_Cache;
        TaoSamplePoint.persist(cacheFileLocation, sp);
        suiteaSamplePoints.Add(sp);
      }

      foreach (FileInfo cacheFi in validCacheSet) {
        var sp = TaoSamplePoint.loadOrNull(cacheFi);
        suiteaSamplePoints.Add(sp);
      }

      var result = new TaoStatisticVo(suiteName, suiteaSamplePoints);
      return result;
    }

  }

  /********************************************************************************
  * A Value Object (Vo) 
  */
  public class TaoStatisticVo {
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
    private List<TaoSamplePoint> filteredPoints = new List<TaoSamplePoint>();

    internal TaoStatisticVo(string suiteName, List<TaoSamplePoint> rowSamplePoints) {
      this.suiteName = suiteName;
      rowSamplePoints.Sort();
      // filer empty tests
      foreach (var samplePoint in rowSamplePoints) {
        if (samplePoint.totalTests > 0) {
          this.filteredPoints.Add(samplePoint);
        }
      }
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
  }

  /********************************************************************************
  */
  internal class TaoSamplePoint : FileCachableI, IComparable {
    internal string suiteName = "";
    internal DateTime timeStamp = DateTime.Now;
    internal int totalTests = 0;
    internal int totalPass = 0;
    internal double passRate = 0.0;

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
      var taoReportFi = new FileInfo(taoReportFileLocation);
      return create(taoReportFi);
    }

    internal static TaoSamplePoint create(FileInfo taoReportFi) {
      var fileNameData = ReportFileNameParser.parseFile(taoReportFi);
      var taoSuiteResults = TaoReportReader.parseFile(taoReportFi);
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

    private static DataTableReaderWriter CACHE_ReadWriter = new DataTableReaderWriter(s);

    public static void persist(string fileLocation, TaoSamplePoint v) {
      DataTable dataTable = CACHE_ReadWriter.createEmptyCacheTable();
      dataTable.Rows.Add(//
        v.suiteName, v.timeStamp, //
        v.totalTests, v.totalPass //
      );
      CACHE_ReadWriter.persistDataTable(fileLocation, dataTable);
    }

    public static void persist(string fileLocation, List<TaoSamplePoint> list) {
      DataTable dataTable = CACHE_ReadWriter.createEmptyCacheTable();
      foreach (var v in list) {
        dataTable.Rows.Add(//
          v.suiteName, v.timeStamp, //
          v.totalTests, v.totalPass //
        );
      }
      CACHE_ReadWriter.persistDataTable(fileLocation, dataTable);
    }

    public static TaoSamplePoint loadOrNull(FileInfo fileInfo) {
      List<TaoSamplePoint> rows = new List<TaoSamplePoint>();
      DataTable dataTable = CACHE_ReadWriter.loadDataTable(fileInfo);
      foreach (DataRow v in dataTable.Rows) {
        string suiteName = Convert.ToString(v["suiteName"]);
        DateTime timeStamp = Convert.ToDateTime(v["timeStamp"]);
        int totalTests = Convert.ToInt32(v["totalTests"]);
        int totalPass = Convert.ToInt32(v["totalPass"]);
        TaoSamplePoint t = new TaoSamplePoint(suiteName, timeStamp, totalTests, totalPass);
        rows.Add(t);
      }

      TaoSamplePoint result = null;
      if (rows.Count > 0) {
        result = rows[0];
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
   * A Value Object (Vo) 
   * Handle file name of format: 'generate_BOM_Output_01.2016-05-16_2310.Lecce.xls
   */
  internal class ReportFileNameParser {
    internal static string REGEX_ofReportFile = @"([^\.]*)\.(\d*-\d*-\d*_\d*)\.([^\.]*)\.";
    internal static string FORMAT_YYYYMMDD_HHMM = "yyyy-MM-dd_HHmm";
    internal FileInfo taoReportFi;
    internal string suiteName;
    internal DateTime dateTimeOfReport;
    internal string dbInstance;

    internal static ReportFileNameParser parseFile(string taoReportFileLocation) {
      var taoReportFi = new FileInfo(taoReportFileLocation);
      return new ReportFileNameParser(taoReportFi);
    }

    internal static ReportFileNameParser parseFile(FileInfo taoReportFi) {
      return new ReportFileNameParser(taoReportFi);
    }

    private ReportFileNameParser(FileInfo taoReportFi) {
      this.taoReportFi = taoReportFi;

      // Analyze file name
      Match match = Regex.Match(taoReportFi.Name, REGEX_ofReportFile, RegexOptions.IgnoreCase);
      this.suiteName = match.Groups[1].Value;
      string dateValue = match.Groups[2].Value;
      if (!DateTime.TryParseExact(dateValue, FORMAT_YYYYMMDD_HHMM, null, DateTimeStyles.None, out this.dateTimeOfReport)) {
        throw new Exception("Unable to convert to a date and time: " + dateValue);
      }
      this.dbInstance = match.Groups[3].Value;
    }

    public override string ToString() {
      string result = string.Format("[suiteName={0}, dateTimeOfReport={1}, dbInstance={2}]", suiteName, dateTimeOfReport.ToString("dd.MM.yyyy HH:mm"), dbInstance);
      return result.ToLower();
    }
  }
}
