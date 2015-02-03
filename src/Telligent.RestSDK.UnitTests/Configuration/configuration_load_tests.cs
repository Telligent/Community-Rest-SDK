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
    public class TestUserResolver:ILocalUserResolver
    {

        public LocalUser GetLocalUserDetails(System.Web.HttpContextBase copntext, Host host)
        {
            return new LocalUser("testuser", "dummy@localhost.com");
        }
    }

    [TestFixture]
    public class configuration_load_tests
    {
        private static readonly string _data = @"
<communityServerHosts>
  <host id=""3D1118BB-0B61-2222-9332-541558F97887"" name=""site"" communityServerUrl=""http://mycommunity.com/"" networkUsername=""networkUser"" networkPassword=""password"" networkDomain=""domain"">
    <oauth clientId=""123456"" 
           clientSecret=""98765432123456789"" 
           callbackUrl=""~/auth.ashx"" 
           cookieName=""evoUser""
           defaultLanguage=""en-GB""
           anonymousUsername=""Anon"">
        <localAuthentication enabled =""true"" membershipAdministrationUsername=""myadmin"" userResolver=""Telligent.RestSDK.IntegrationTests.Configuration.TestUserResolver, Telligent.RestSDK.UnitTests"">
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
            _config = manager.GetOptions("site");
        }


        [Test]
        public void sso_is_enabled()
        {
            Assert.IsTrue(_config.OAuth.LocalUserCreation.SSO.Enabled);
        }
        [Test]
        public void local_auth_is_enabled()
        {
            
            Assert.IsTrue(_config.OAuth.LocalUserCreation.Enabled);
        }
        [Test]
        public void local_auth_admin_user()
        {
            
            Assert.IsTrue(_config.OAuth.LocalUserCreation.MembershipAdministrationUserName.Equals("myadmin", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void sso_sync_cookie()
        {
            
            Assert.IsTrue(_config.OAuth.LocalUserCreation.SSO.SynchronizationCookieName.Equals("SyncCookie", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void oauth_callback()
        {
            
            Assert.IsTrue(_config.OAuth.OauthCallbackUrl.Equals("~/auth.ashx", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void oauth_cookie()
        {
            
            Assert.IsTrue(_config.OAuth.CookieName.Equals("evoUser", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void default_language()
        {
            
            Assert.IsTrue(_config.OAuth.DefaultLanguageKey.Equals("en-GB", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void anonymous_user()
        {
            
            Assert.IsTrue(_config.OAuth.AnonymousUsername.Equals("Anon", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void Name_Set()
        {
            
            Assert.IsTrue(_config.Name.Equals("site", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void Url_Set()
        {
            
            Assert.IsTrue(_config.CommunityServerUrl.Equals("http://mycommunity.com/", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void Id_Set()
        {
            Assert.AreEqual(_config.Id.Value, Guid.Parse("3D1118BB-0B61-2222-9332-541558F97887"));
        }
        [Test]
        public void Network_Set()
        {
            Assert.IsNotNull(_config.NetworkCredentials);
        }
        [Test]
        public void NetworkUser_Set()
        {

            Assert.IsTrue(_config.NetworkCredentials.UserName.Equals("networkUser", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void NetworkPassword_Set()
        {

            Assert.IsTrue(_config.NetworkCredentials.Password.Equals("password", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void NetworkDomain_Set()
        {

            Assert.IsTrue(_config.NetworkCredentials.Domain.Equals("domain", StringComparison.CurrentCultureIgnoreCase));
        }

        [Test]
        public void UserResolver_not_null()
        {
          Assert.IsNotNull(_config.OAuth.LocalUserCreation.UserResolver);  
        }
        [Test]
        public void UserResolver_is_of_type()
        {
            Assert.IsAssignableFrom<TestUserResolver>(_config.OAuth.LocalUserCreation.UserResolver);
        }
    }
}
