﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using taoGUI.TaoBeanReader;

namespace taoGUI {
  public class TaoReportReader {
    private int _sumTotalTests;
    private int _sumPairsThatAreEqual;
    private double _overallPassRate;

    private void initialiseTaoSummaryValues() {
      _sumTotalTests = 0;
      _sumPairsThatAreEqual = 0;
      _overallPassRate = 0.0;
    }

    public static TaoReportReader parseFile(string taoReportFilename) {
      var taoReportFi = new FileInfo(taoReportFilename);
      return parseFile(taoReportFi);
    }

    public static TaoReportReader parseFile(FileInfo taoReportFi) {
      return new TaoReportReader(taoReportFi);
    }

    private TaoReportReader(FileInfo taoReportFi) {
      initialiseTaoSummaryValues();
      string result = string.Empty;
      Exception exception = null;
      OleDbCommand oComm = null;
      OleDbDataReader oRdr = null;
      DataTable oTbl = null;
      string sConnString = string.Empty;
      string sCommand = string.Empty;
      OleDbConnection oConn = new OleDbConnection();
      if (taoReportFi.Exists) {
        try {
          // Read Excel via Jet OLEB
          string connectVersion = taoReportFi.Extension.EndsWith("xls") ? "Excel 8.0" : "Excel 12.0";
          sConnString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + taoReportFi.FullName + ";Extended Properties='" + connectVersion + ";HDR=No;READONLY=TRUE';";
          oConn.ConnectionString = sConnString;
          oConn.Open();
          oComm = new OleDbCommand(@"SELECT * FROM [Summary$]", oConn);
          oRdr = oComm.ExecuteReader();
          oTbl = new DataTable();
          oTbl.Load(oRdr);
          oTbl.Select();

          // Read TaoBean 
          ToaBeanReader taoBeanReader = new ToaBeanReader(oTbl);
          DataTable taoBeanTable = taoBeanReader.getTeoBeanTable("TaoSuiteSummary");

          // Calculate pass rates
          _sumTotalTests = taoBeanTable.AsEnumerable().Sum(c => Convert.ToInt32(c.Field<string>("Total_Tests")));
          _sumPairsThatAreEqual = taoBeanTable.AsEnumerable().Sum(c => Convert.ToInt32(c.Field<string>("Pairs_that_are_Equal")));
          if (_sumTotalTests > 0) {
            _overallPassRate = ((double)_sumPairsThatAreEqual / (double)_sumTotalTests) * 100.0;
          }
        } finally {
          if (oRdr != null) {
            oRdr.Close();
          }
          if (oTbl != null) {
            oTbl.Dispose();
          }
          if (oComm != null) {
            oComm.Dispose();
          }
          if (oConn != null) {
            oConn.Close();
          }
        }
        if (exception != null) {
          throw exception;
        }
      }
    }

    public double getOverallPassRate() {
      return _overallPassRate;
    }

    public int getTotalTests() {
      return _sumTotalTests;
    }

    public int getTotalPass() {
      return _sumPairsThatAreEqual;
    }

    ~TaoReportReader() {
      // Clean up...
    }

  }
}
