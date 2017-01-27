using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace taoGUI.TaoBeanReader {

  /**************************************************
   * 
   */
  public class ToaBeanReader {
      int maxX = 0, maxY = 0;
      DataTable oTbl;
      Dictionary<string, Coordinate> allTaoBeansMap;

      public ToaBeanReader(DataTable oTbl) {
        this.maxX = oTbl.Columns.Count - 1;
        this.maxY = oTbl.Rows.Count - 1;
        this.oTbl = oTbl;
        this.allTaoBeansMap = findAllTaoBeanCoords_private();
      }

      public DataTable getTeoBeanTable(string beanName) {
        DataTable result = new DataTable();
        Coordinate beanStart = findTaoBeanCoord(beanName);
        if (beanStart != Coordinate.NULL_COORD) {
          int offset = getColLeng(beanStart) - 1;
          Coordinate headerCoord = beanStart.clone().moveRight().moveDown(offset);
          Coordinate recCoord = headerCoord.clone().moveDown();
          //
          int tableW = getRowLeng(headerCoord);
          int tableH = getColLeng(recCoord);
          List<string> headerList = getRow(headerCoord, tableW);
          foreach (string headName in headerList) {
            result.Columns.Add(headName);
          }
          List<string> recList;
          for (int i = 0; i < tableH; i++) {
            recList = getRow(recCoord, tableW);
            result.Rows.Add(recList.ToArray());
            recCoord.moveDown();
          }
        }
        return result;
      }

      public List<string> getRow(Coordinate startCoord) {
        List<string> result = new List<string>();
        int leng = getRowLeng(startCoord);
        Coordinate c = startCoord.clone();
        for (int i = 0; i < leng; i++) {
          result.Add(getCell(c.moveRight()).Trim());
        }
        return result;
      }

      public List<string> getRow(Coordinate startCoord, int leng) {
        List<string> result = new List<string>();
        Coordinate c = startCoord.clone();
        for (int i = 0; i < leng; i++) {
          result.Add(getCell(c).Trim());
          c.moveRight();
        }
        return result;
      }

      public List<string> getCol(Coordinate startCoord) {
        List<string> result = new List<string>();
        int leng = getColLeng(startCoord);
        Coordinate c = startCoord.clone();
        for (int i = 0; i < leng; i++) {
          result.Add(getCell(c).Trim());
          c.moveDown();
        }
        return result;
      }

      public List<string> getCol(Coordinate startCoord, int leng) {
        List<string> result = new List<string>();
        Coordinate c = startCoord.clone();
        for (int i = 0; i < leng; i++) {
          result.Add(getCell(c.moveDown()).Trim());
        }
        return result;
      }

      /**
       * Row leng inclusive start position 
       */
      public int getRowLeng(Coordinate startCoord) {
        int result = 0;
        string cellStr = "";
        Coordinate c = startCoord.clone();
        while (c != Coordinate.NULL_COORD) {
          cellStr = getCell(c).Trim();
          if (cellStr.Length > 0) {
            result++;
          } else {
            break;
          }
          c.moveRight();
          if (c.getX() > maxX) {
            break;
          }
        }
        return result;
      }

      /**
       * Col leng inclusive start position 
       */
      public int getColLeng(Coordinate startCoord) {
        int result = 0;
        string cellStr = "";
        Coordinate c = startCoord.clone();
        while (c != Coordinate.NULL_COORD) {
          cellStr = getCell(c).Trim();
          if (cellStr.Length > 0) {
            result++;
          } else {
            break;
          }
          c.moveDown();
          if (c.getY() > maxY) {
            break;
          }
        }
        return result;
      }

      /**
       * Will find a TaoBean-cell that matched the name passed. 
       * Return will be the pos of the "TaoBean"-cell, thus the taoBean-name will be on the right side
       * Coordinate.NULL_COORD is returned if fail to find the bean
       */
      public Coordinate findTaoBeanCoord(string beanName) {
        bool found = allTaoBeansMap.ContainsKey(beanName.Trim());
        return found ? allTaoBeansMap[beanName] : Coordinate.NULL_COORD;
      }

      public Dictionary<string, Coordinate> getTaoBeanToCoordMap() {
        return allTaoBeansMap;
      }

      private static string TAO_BEAN = "TaoBean";
      private Dictionary<string, Coordinate> findAllTaoBeanCoords_private() {
        var result = new Dictionary<string, Coordinate>();
        Coordinate c = Coordinate.NULL_COORD;
        do {
          c = find(TAO_BEAN, next(c));
          if (c != Coordinate.NULL_COORD) {
            string key = getCell(c.clone().moveRight()).Trim();
            result.Add(key, c);
          }
        } while (c != Coordinate.NULL_COORD);
        return result;
      }


      /**
       * If found will return the coordiate of the cell. Otherwise Coordinate.NULL_COORD is returned
       */
      public Coordinate find(string searchTxt) {
        return find(searchTxt, Coordinate.NULL_COORD);
      }

      public Coordinate find(string searchTxt, Coordinate startCoord) {
        string searchTxtTrimed = searchTxt.Trim();
        Coordinate c = (startCoord == Coordinate.NULL_COORD) ? next(startCoord) : startCoord.clone();

        while (c != Coordinate.NULL_COORD) {
          string cellStr = getCell(c);
          if (searchTxtTrimed.Equals(cellStr)) {
            break;
          }
          c = next(c);
        }
        return c;
      }

      public string getCell(Coordinate c) {
        string resut = "";
        if (c != Coordinate.NULL_COORD) {
          resut = oTbl.Rows[c.getY()][c.getX()].ToString().Trim();
        }
        return resut;
      }

      /**
       * When walking thorw the table this method will give use the next coordinate. 
       * If end of table is reached Coordinate.NULL_COORD is returned.
       * If Coordinate.NULL_COORD is given as startCoord we beginn at the start 
       */
      public Coordinate next(Coordinate startCoord) {
        Coordinate result = Coordinate.NULL_COORD;
        int x = 0, y = 0;
        if (startCoord != Coordinate.NULL_COORD) {
          x = startCoord.getX();
          y = startCoord.getY();
        }

        if (x < maxX) {
          x++;
        } else {
          x = 0;
          y++;
        }
        if (y < maxY) {
          result = new Coordinate(x, y);
        } else {
          result = Coordinate.NULL_COORD;
        }
        return result;
      }

    }

  /**************************************************
   * 
   */
  public class Coordinate {
      private bool isVoid = true;
      public static Coordinate NULL_COORD = new Coordinate();
      private int x, y = 0;
      private Coordinate maxCoord = NULL_COORD;

      private Coordinate() {
        isVoid = true;
      }

      internal Coordinate(int x, int y) {
        this.isVoid = false;
        this.x = x;
        this.y = y;
      }

      public int getX() { return (int)x; }
      public int getY() { return (int)y; }

      // Move this instance
      public Coordinate moveLeft() { x = getLeftX(); return this; }
      public Coordinate moveRight() { x = getRightX(); return this; }
      public Coordinate moveUp() { y = getUpY(); return this; }
      public Coordinate moveDown() { y = getDownY(); return this; }

      public Coordinate moveLeft(int v) { x = (x - v) >= 0 ? (x - v) : 0; return this; }
      public Coordinate moveRight(int v) { x += v; return this; }
      public Coordinate moveUp(int v) { y = (y - v) >= 0 ? (y - v) : 0; return this; }
      public Coordinate moveDown(int v) { y += v; return this; }


      // getCopy but leve this unchanged
      public Coordinate clone() { return new Coordinate(x, y); }

      public bool isNull() {
        return isVoid;
      }

      private int getLeftX() { return (x <= 0) ? x : x - 1; }
      private int getRightX() { return x + 1; }
      private int getUpY() { return (y <= 0) ? y : y - 1; }
      private int getDownY() { return y + 1; }

      public override string ToString() {
        return string.Format("X:{0} ; Y:{1}", x, y);
      }

    }

}
