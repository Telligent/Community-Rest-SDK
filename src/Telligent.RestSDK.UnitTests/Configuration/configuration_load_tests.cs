using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Implementations;
using Telligent.Rest.SDK.Model;

namespace Telligent.RestSDK.IntegrationTests.Configuration
{
    [TestFixture]
    public class configuration_load_tests
    {
        private static readonly string _data =@"
<communityServerHosts>
  <host id=""3D1118BB-0B61-2222-9332-541558F97887"" name=""site"" communityServerUrl=""http://mycommunity.com/"">
    <oauth clientId=""123456"" 
           clientSecret=""98765432123456789"" 
           callbackUrl=""~/auth.ashx"" 
           cookieName=""evoUser""
           defaultLanguage=""en-GB""
           anonymousUsername=""Anon"">
        <localAuthentication enabled =""true"" membershipAdministrationUsername=""myadmin"" userResolver=""SomeNamespace.SomeClass, Assembly"">
        <sso enabled=""true"" synchronizationCookieName=""SyncCookie"" />
      </localAuthentication>
    </oauth>
  </host>
</communityServerHosts>";

        private IHostConfigurationManager _manager;
        private HostConfiguration _config;
        [TestFixtureSetUp]
        public void Setup()
        {
            var fileMock = new Mock<IConfigurationFile>();
            fileMock.Setup(m => m.GetConfigurationData()).Returns(() => _data);
            var manager = new HostConfigurationManager(new Mock<IRestCache>().Object, fileMock.Object);
        }

       
    }
}
