using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace taoGUI {
  class taoReportReader {

    private int _sumTotalTests;
    private int _sumPairsThatAreEqual;
    private int _sumPairsThatAreNotEqual;
    private int _sumPairsCommentedOut;
    private double _overallPassRate;

    private static Excel.Application _taoApp = null;
    private static Excel.Workbook _taoBook = null;
    private static Excel.Worksheet _taoSheet = null;

    private void initialiseTaoSummaryValues() {
      _sumTotalTests = 0;
      _sumPairsThatAreEqual = 0;
      _sumPairsThatAreNotEqual = 0;
      _sumPairsCommentedOut = 0;
      _overallPassRate = 0.0;
    }

    public taoReportReader (string taoReportFilename) {
      initialiseTaoSummaryValues();
      if (System.IO.File.Exists(taoReportFilename)) {
        _taoApp = new Excel.Application();
        _taoApp.Visible = false;
        _taoBook = _taoApp.Workbooks.Open(taoReportFilename);

        // Tao Reports are very structured so, for now, assume:
        //    1. Summary results are here on the first Tab.
        //    2. Summary table starts row 16
        //    3. columns F - I contain summary values.
        _taoSheet = (Excel.Worksheet)_taoBook.Sheets[1];
        int totalRows = (int)_taoSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell).Row + 1;
        System.Array _tmpTotalTests = (System.Array)_taoSheet.get_Range("F16", "F" + totalRows.ToString()).Cells.Value;
        List<string> _totalTests = _tmpTotalTests.OfType<string>().ToList();
        _totalTests.RemoveAll(item => item == null);
        System.Array _tmpPairsThatAreEqual = (System.Array)_taoSheet.get_Range("G16", "G" + totalRows.ToString()).Cells.Value;
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
        // Close and clean up workbook objects.
        _taoBook.Close(0);
        _taoApp.Quit();
        System.Runtime.InteropServices.Marshal.ReleaseComObject(_taoApp);
        GC.Collect();
      }
    }

    public double getOverallPassRate() {
      return _overallPassRate;
    }

    ~taoReportReader() {
      // Clean up...
    }

  }
}
