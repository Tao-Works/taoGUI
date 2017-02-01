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

    [TestMethod]
    public void Test_TaoStatAccessor_NO_Cache() {
      TaoStatAccessorI statAccessor = TaoAccessorFactory.getStatAccessor(AccessType.NoCache, ".", "appId");
      TaoStatisticVo taoStats = statAccessor.getStats("A_TaoReport", "dev");
      Assert.IsNotNull(taoStats);
    }

    [TestMethod]
    public void Test_TaoStatAccessor_WITH_Cache() {
      TaoStatAccessorI statAccessor = TaoAccessorFactory.getStatAccessor(AccessType.WithCache, ".", "appId");
      TaoStatisticVo taoStats = statAccessor.getStats("A_TaoReport", "dev");
      Assert.IsNotNull(taoStats);
    }

  }
}
