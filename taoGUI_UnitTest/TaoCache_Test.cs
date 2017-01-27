using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using taoGUI.Caching;
using System.Globalization;
using System.Text.RegularExpressions;

namespace taoGUI_UnitTest.Caching {
  [TestClass]
  public class TaoCache_Test { 
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
      Assert.AreEqual("xls", match.Groups[4].Value);
    }

    [TestMethod]
    public void Test_ReportFileNameParser() {
      DateTime timeStamp = DateTime.MinValue;
      DateTime.TryParseExact(TEST_TimeStr, ReportFileNameParser.FORMAT_YYYYMMDD_HHMM, null, DateTimeStyles.None, out timeStamp);

      var r = ReportFileNameParser.parseReportFile(TEST_ReportFileStr);
      Assert.IsNotNull(r);
      Assert.AreEqual("generate_Foo_01", r.suiteName);
      Assert.AreEqual(timeStamp, r.dateTimeOfReport);
      Assert.AreEqual("FooDb", r.dbInstance);
      Assert.AreEqual("xls", r.extention);
    }

    [TestMethod]
    public void Test_TaoSamplePoint_Persit() {
      DateTime timeStamp = DateTime.Now;
      TaoSamplePoint sp = new TaoSamplePoint("generate_Foo_01", timeStamp, 100, 80);
      TaoSamplePoint.persist(@".\~cache\"+ TEST_ReportFileStr_NoExt+".txt", sp);
      Assert.IsNotNull(sp);
    }

  }
}
