using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Telligent.RestSDK.IntegrationTests
{
    public class get_tests:IntegrationTestBase
    {
        [Test]
        public async Task can_do_get_single_dynamic_request()
        {
            var endpoint = "/info.json";
            dynamic info = await Host.GetToDynamicAsync(2,endpoint);

            Assert.IsNotNull(info.InfoResult.SiteName);
        }
        [Test]
        public async Task can_do_get_multiple_dynamic_request()
        {
            var endpoint = "/info.json";
            var o1 =  Host.GetToDynamicAsync(2, endpoint);
            var o2 = Host.GetToDynamicAsync(2, endpoint);
            var o3 = Host.GetToDynamicAsync(2, endpoint);
            var o4 = Host.GetToDynamicAsync(2, endpoint);
            var o5 = Host.GetToDynamicAsync(2, endpoint);
            Task.WaitAll();
            Assert.IsNotNull(o1);
            Assert.IsNotNull(o2);
            Assert.IsNotNull(o3);
            Assert.IsNotNull(o4);
            Assert.IsNotNull(o5);
        }
        //[Test]
        //public void can_do_get_paged_dynamic_request()
        //{
        //    var endpoint = "/users.json";
        //    dynamic users = Host.GetToDynamic(2, endpoint);

        //    Assert.IsNotNull(info.InfoResult.SiteName);
        //}
    }
}
