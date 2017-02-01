using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using taoGUI.Caching;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

namespace taoGUI_UnitTest.Caching {

  [TestClass]
  public class DataTableReaderWriter_Test { 
    private static string TEST_TimeStr = "2017-07-17_1707";
    private static string TEST_ReportFileStr_NoExt = "generate_Foo_01." + TEST_TimeStr + ".FooDb";
    private static string TEST_ReportFileStr = TEST_ReportFileStr_NoExt+".xls";

    [TestMethod]
    public void Test_TaoSamplePoint_New() {
      DateTime timeStamp = DateTime.MinValue;
      DateTime.TryParseExact(TEST_TimeStr, ReportFileNameParser.FORMAT_YYYYMMDD_HHMM, null, DateTimeStyles.None, out timeStamp);
      var p = new TaoSamplePoint("test", timeStamp, 90, 100);
      Assert.IsNotNull(p);
    }

    [TestMethod]
    public void Test_RegEx() {
      Match match = Regex.Match(TEST_ReportFileStr, ReportFileNameParser.REGEX_ofReportFile, RegexOptions.IgnoreCase);

      Assert.IsTrue(match.Success, "RegEx failed to parse '" + TEST_ReportFileStr + "' with '" + ReportFileNameParser.REGEX_ofReportFile + "'");
      Assert.AreEqual("generate_Foo_01",match.Groups[1].Value);
      Assert.AreEqual(TEST_TimeStr, match.Groups[2].Value);
      Assert.AreEqual("FooDb", match.Groups[3].Value);
    }

    [TestMethod]
    public void Test_ReportFileNameParser() {
      DateTime timeStamp = DateTime.MinValue;
      DateTime.TryParseExact(TEST_TimeStr, ReportFileNameParser.FORMAT_YYYYMMDD_HHMM, null, DateTimeStyles.None, out timeStamp);

      var r = ReportFileNameParser.parseFile(TEST_ReportFileStr);
      Assert.IsNotNull(r);
      Assert.AreEqual("generate_Foo_01", r.suiteName);
      Assert.AreEqual(timeStamp, r.dateTimeOfReport);
      Assert.AreEqual("FooDb", r.dbInstance);
    }

    [TestMethod]
    public void Test_TaoSamplePoint_Persit() {
      string fileLocation = @".\~cache\" + TEST_ReportFileStr_NoExt + ".txt";
      var fileInfo = new FileInfo(fileLocation);
      DateTime timeStamp = DateTime.Now;
      TaoSamplePoint spToPersis = new TaoSamplePoint("generate_Foo_01", timeStamp, 100, 80);
      Assert.IsNotNull(spToPersis);
      TaoSamplePoint.persist(fileLocation, spToPersis);
      TaoSamplePoint spLoaded =  TaoSamplePoint.loadOrNull(fileInfo);
      Assert.IsNotNull(spLoaded);
      Assert.AreEqual(spToPersis, spLoaded);
    }

  }
}
