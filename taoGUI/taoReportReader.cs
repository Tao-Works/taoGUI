using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace taoGUI {
  class TaoReportReader {
    private int _sumTotalTests;
    private int _sumPairsThatAreEqual;
    private double _overallPassRate;

    private void initialiseTaoSummaryValues() {
      _sumTotalTests = 0;
      _sumPairsThatAreEqual = 0;
      _overallPassRate = 0.0;
    }

    public TaoReportReader(string taoReportFilename) {
      Excel.Workbook workbook = null;
      Excel.Worksheet sheet = null;
      try {
        initialiseTaoSummaryValues();
        if (System.IO.File.Exists(taoReportFilename)) {
          // open in readonly mode (may add performance)
          workbook = Form1._taoApp.Workbooks.Open(taoReportFilename, false, true);
          Form1._taoApp.Calculation = Excel.XlCalculation.xlCalculationManual; // For efficiency and performance

          /*
           * Tao Reports are very structured so, for now, assume:
           * 1. Summary results are here on the first Tab.
           * 2. Summary table starts row 16
           * 3. columns F - I contain summary values.
           */
          sheet = workbook.Sheets[1];
          int totalRows = (int)sheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell).Row + 1;
          System.Array _tmpTotalTests = (System.Array)sheet.get_Range("F16", "F" + totalRows.ToString()).Cells.Value;
          List<string> _totalTests = _tmpTotalTests.OfType<string>().ToList();
          _totalTests.RemoveAll(item => item == null);
          System.Array _tmpPairsThatAreEqual = (System.Array)sheet.get_Range("G16", "G" + totalRows.ToString()).Cells.Value;
          List<string> _pairsThatAreEqual = _tmpPairsThatAreEqual.OfType<string>().ToList();
          _pairsThatAreEqual.RemoveAll(item => item == null);
          totalRows = _totalTests.Count;
          for (int i = 0; i < totalRows; i++) {
            _sumTotalTests += Convert.ToInt32(_totalTests[i]);
            _sumPairsThatAreEqual += Convert.ToInt32(_pairsThatAreEqual[i]);
          }
          if (_sumTotalTests > 0) {
            _overallPassRate = ((double)_sumPairsThatAreEqual / (double)_sumTotalTests) * 100.0;
          }
        }
      } catch (Exception ex) {
        // TODO Sam: handle exceptips in C#   
      } finally { // Close and clean up workbook objects. 
        if (workbook != null) {
          workbook.Close(false);
        }
      }
    }

    public double getOverallPassRate() {
      return _overallPassRate;
    }

    public int getTotalTests() {
      return _sumTotalTests;
    }

    public int getPairsThatAreEqual() {
      return _sumPairsThatAreEqual;
    }

    ~TaoReportReader() {
      // Clean up...
    }

  }
}
