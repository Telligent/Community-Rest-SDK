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
        public void can_do_get_single_dynamic_request()
        {
            var endpoint = "/info.json";
            dynamic info = Host.GetRestEndpoint(2,endpoint);

            Assert.IsNotNull(info.InfoResult.SiteName);
        }
        [Test]
        public void can_do_get_paged_dynamic_request()
        {
            var endpoint = "/users.json";
            dynamic info = Host.GetRestEndpoint(2, endpoint);

            Assert.IsNotNull(info.InfoResult.SiteName);
        }
    }
}
