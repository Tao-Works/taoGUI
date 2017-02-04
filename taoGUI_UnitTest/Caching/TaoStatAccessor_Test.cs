using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taoGUI.Caching;
using static taoGUI.Caching.TaoAccessorFactory;

namespace taoGUI_UnitTest.Caching {

  [TestClass]
  public class TaoStatAccessor_Test {
    private static string DIR_Test_Data = @"..\..\Test_Data";

    [TestMethod]
    public void Test_TaoStatAccessor_NO_Cache() {
      TaoStatAccessorI statAccessor = TaoAccessorFactory.getStatAccessor(AccessType.NoCache, DIR_Test_Data, "appId");
      TaoStatisticVo taoStats = statAccessor.getStats("A_TaoReport", "dev");
      Assert.IsNotNull(taoStats);
    }

    [TestMethod]
    public void Test_TaoStatAccessor_WITH_Cache() {
      TaoStatAccessorI statAccessor = TaoAccessorFactory.getStatAccessor(AccessType.WithCache, DIR_Test_Data, "appId");
      TaoStatisticVo taoStats = statAccessor.getStats("A_TaoReport", "dev");
      Assert.IsNotNull(taoStats);
    }

  }
}
