using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace taoGUI.TaoReader {


  /**************************************************
   * 
   */
  public class TaoBeanReader_TEST {

    static void Main(string[] args) {
      // Snippet4ExcelAccess.test();
      printDir(@"c:\@Cloud\TeamDrive\TeamMotivation\DeveloperArea\TaoOnLocalPc\TaoApp.bear\tao.conf.baer.gdrp", "g.", "Lecce");
    }

    private static void printDir(string projectRootFolder, string taoSuite, string dbInstance) {
      OleDbConnection oConn = null;
      string taoSuiteOutputFolder = projectRootFolder + @"\taoSuite_Report";
      string filePattern = taoSuite.Substring(0, taoSuite.IndexOf(".")) + "*" + dbInstance + ".xls";

      string[] taoResults = System.IO.Directory.GetFiles(taoSuiteOutputFolder, filePattern);
      var orderedResults = taoResults.OrderBy(f => f.ToString());
      try {
        foreach (string orderedResult in orderedResults) {
          printSummery(orderedResult);
        }
        Console.WriteLine("--- size " + orderedResults.Count());
        Console.ReadKey();
      } finally {
        if (oConn != null) {
          oConn.Close();
          oConn.Dispose();
        }
      }
    }

    private static string printSummery(string filepathToExcel) {
      string result = string.Empty;
      Exception exception = null;
      OleDbCommand oComm = null;
      OleDbDataReader oRdr = null;
      DataTable oTbl = null;
      string sConnString = string.Empty;
      string sCommand = string.Empty;
      OleDbConnection oConn = new OleDbConnection();
      try {
        // Read Excel via Jet OLEB
        string connectVersion = filepathToExcel.EndsWith("xls") ? "Excel 8.0" : "Excel 12.0";
        sConnString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filepathToExcel + ";Extended Properties='" + connectVersion + ";HDR=No;READONLY=TRUE';";
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

        // DataTable => CSV
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("+----------------------------------------------------");
        sb.AppendLine("|  " + filepathToExcel);
        sb.AppendLine("+----------------------------------------------------");
        IEnumerable<string> columnNames = taoBeanTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
        sb.AppendLine(string.Join(";", columnNames));
        foreach (DataRow row in taoBeanTable.Rows) {
          IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
          sb.AppendLine(string.Join(";", fields));
        }
        result = sb.ToString();
        Console.WriteLine(result);
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        exception = ex;
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
      return result;
    }

    // ====================================================================================
    // Snippet TEST CODE
    // ====================================================================================
    private static void test() {
      OleDbConnection oConn = null;
      OleDbCommand oComm = null;
      OleDbDataReader oRdr = null;
      DataTable oTbl = null;
      String CON_XLS = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=<filepathToExcel>.xlsx;Extended Properties='Excel 12.0 Xml;HDR=YES;'"; // XLSX
      String CON_XLSX = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=<filepath2excel>.xls;Extended Properties='Excel 8.0;HDR=YES;'";    // XLS
      String sFirstFile = String.Empty;
      String sSecondFile = String.Empty;
      String sConnString = String.Empty;
      String sCommand = String.Empty;
      try {
        sFirstFile = @"C:\Temp\Fruits1.xls";
        sSecondFile = @"C:\Temp\Fruits2.xls";
        sConnString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sFirstFile + ";Extended Properties='Excel 8.0;HDR=Yes';";
        oConn = new OleDbConnection(sConnString);
        oConn.Open();
        sCommand = @"SELECT NameOfFruit FROM [Fruits$]" + Environment.NewLine +
                    "UNION ALL" + Environment.NewLine +
                    "SELECT NameOfFruit FROM [Fruits$] IN '" + sSecondFile + "' 'Excel 8.0;';";
        oComm = new OleDbCommand(sCommand, oConn);
        oRdr = oComm.ExecuteReader();
        oTbl = new DataTable();
        oTbl.Load(oRdr);

        foreach (DataRow row in oTbl.Rows) {
          Console.WriteLine(row["NameOfFruit"].ToString());
        }

        Console.ReadKey();
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
        Console.ReadKey();
      } finally {
        if (oRdr != null)
          oRdr.Close();
        oRdr = null;
        if (oTbl != null)
          oTbl.Dispose();
        oComm.Dispose();
        oConn.Close();
        oConn.Dispose();
      }
    }

  }
}
