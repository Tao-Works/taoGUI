using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

[assembly: InternalsVisibleTo("taoGUI_UnitTest")]


namespace taoGUI.Caching {
  /******************************************************************************** 
   */
  public interface FileCachableI {
    string[] getHeaderNames();
    Type[] getHeaderTypes();
  }

  /********************************************************************************
  */
  public class DataTableReaderWriter {
    internal static string CSV_Seperator = ";";
    private FileCachableI classRepresentive;

    internal DataTableReaderWriter(FileCachableI classRepresentive) {
      this.classRepresentive = classRepresentive;
    }

    public DataTable loadDataTable(FileInfo fileInfo) {
      DataTable resultTable = createEmptyCacheTable();
      if (!fileInfo.Exists) {
        throw new Exception("Unable to find file: " + fileInfo.FullName);
      }
      TextFieldParser parser = new TextFieldParser(fileInfo.FullName);
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
        DataRow newRow = resultTable.NewRow();
        for (int i = 0; i < headerNames.Length; i++) {
          string columnName = headerNames[i];
          Type toType = headerTypes[i];
          string value = csvStringFields[i];
          newRow[columnName] = Convert.ChangeType(value, toType);
        }
        resultTable.Rows.Add(newRow);
      }
      return resultTable;
    }

    public void persistDataTable(string dtFilepath, DataTable dtToPersist) {
      var txt = new StringBuilder();
      IEnumerable<string> columnNames = dtToPersist.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
      txt.AppendLine(string.Join(";", columnNames));
      foreach (DataRow row in dtToPersist.Rows) {
        IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
        txt.AppendLine(string.Join(";", fields));
      }
      string path = Path.GetDirectoryName(dtFilepath);
      if (path.Length > 0) {
        Directory.CreateDirectory(path);
      }
      StreamWriter sw = new StreamWriter(dtFilepath);
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

}
