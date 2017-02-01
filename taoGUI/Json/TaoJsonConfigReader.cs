using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taoGUI.Json {
                  
  class TaoJsonConfigReader {

    public static Dictionary<string, TaoJsonProjectCtx> getTaoProjectCtxMap(string fileLocation) {
      string jsonStr = getJsonStrFromFile(fileLocation);
      var projectContextList = JsonConvert.DeserializeObject<List<TaoJsonProjectCtx>>(jsonStr);
      var result = new Dictionary<string, TaoJsonProjectCtx>();
      foreach (TaoJsonProjectCtx ctx in projectContextList) {
        result.Add(ctx.applicationId, ctx);
      }
      return result;
    }

    public static Dictionary<string, TaoJsonDbConnection> getTaoDbConnectionMap(string fileLocation) {
      string jsonStr = getJsonStrFromFile(fileLocation);
      var projectContextList = JsonConvert.DeserializeObject<List<TaoJsonDbConnection>>(jsonStr);
      var result = new Dictionary<string, TaoJsonDbConnection>();
      foreach (TaoJsonDbConnection dbConnection in projectContextList) {
        result.Add(dbConnection.connectionId, dbConnection);
      }
      return result;
    }

    public static Dictionary<string, TaoJsonGroupByDimension> getTaoGroupByDimensionMap(string fileLocation) {
      string jsonStr = getJsonStrFromFile(fileLocation);
      var userDimensions = JsonConvert.DeserializeObject<List<TaoJsonGroupByDimension>>(jsonStr);
      var result = new Dictionary<string, TaoJsonGroupByDimension>();
      foreach (TaoJsonGroupByDimension userDimension in userDimensions) {
        result.Add(userDimension.dimension, userDimension);
      }
      return result;
    }

    //----------------------------------
    // Json Classes
    //----------------------------------

    /*
     * [
     *   {
     *     "applicationId" : "tao.conf.baer.gdrp",
     *     "description"   : "",
     *     "folder"        : "C:\Users\user\Documents\Tao\TaoApp.BJB"
     *   }
     * ]
     */
    public class TaoJsonProjectCtx {
      public string applicationId { get; set; }
      public string description { get; set; }
      public string folder { get; set; }
      public string getAppFolder() {
        return folder + "/" + applicationId;
      }
    }

    /*
     * [
     *   {
     *     "connectionId": "DbGdrp",
     *     "jdbcDriver": "oracle.jdbc.OracleDriver",
     *     "username": "jbhost",
     *     "password": "jbhost",
     *     "jdbcUrl": "jdbc:oracle:thin:@//5.249.152.169:1521/XE"
     *  }
     * ]
     */
    public class TaoJsonDbConnection {
      public string connectionId { get; set; }
      public string jdbcDriver { get; set; }
      public string username { get; set; }
      public string password { get; set; }
      public string jdbcUrl { get; set; }
    }

    /*
     * [
     *   {
     *     "dimension" : "Functional",
     *     "attributes" : [ "Parties", "Products", "Reporting", "Reconciliation" ]
     *   }
     * ]
     */
    public class TaoJsonGroupByDimension {
      public string dimension { get; set; }
      public List<string> attributes { get; set; }
    }

    /**
     * Reader helper that filtes any comment lines (starting with #) and replaces escape backslaches 
     */
    private static string getJsonStrFromFile(string fileLocation) {
      StringBuilder sb = new StringBuilder();
      if (File.Exists(fileLocation)) {
        foreach (string line in File.ReadAllLines(fileLocation)) {
          if (line.StartsWith("#")) {
          } else {
            string l = line.Replace(@"\", @"\\");
            sb.Append(l);
            sb.Append("\n");
          }
        }
      }
      string jsonStr = sb.ToString();
      return jsonStr;
    }

  }
}
