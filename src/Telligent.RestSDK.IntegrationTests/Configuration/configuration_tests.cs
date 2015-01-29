using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telligent.Rest.SDK.Configuration;

namespace Telligent.RestSDK.IntegrationTests.Configuration
{
    [TestFixture]
    public class configuration_tests
    {
        [Test]
        public void can_load_from_app_config()
        {
            var section = TelligentConfigurationSection.Current;

            Assert.IsNotNull(section);
        }
        [Test]
        public void config_has_3_hosts()
        {
            var data = TelligentConfigurationSection.Current;

            Assert.AreEqual(data.Hosts.Count,3);
        }
    }
}
