using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Implementations;

namespace Telligent.RestSDK.IntegrationTests.Configuration
{
    [TestFixture]
   public class configuration_defaults
    {
        [Test]
        public void Oauth_is_not_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNotNull(h.OAuth);
        }
        [Test]
        public void local_auth_is_not_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNotNull(h.OAuth.LocalUserCreation);
        }
        [Test]
        public void sso_is_not_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNotNull(h.OAuth.LocalUserCreation.SSO);
        }
        [Test]
        public void sso_is_not_enabled()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsFalse(h.OAuth.LocalUserCreation.SSO.Enabled);
        }
        [Test]
        public void local_auth_is_not_enabled()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsFalse(h.OAuth.LocalUserCreation.Enabled);
        }
        [Test]
        public void local_auth_admin_user()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsTrue(h.OAuth.LocalUserCreation.MembershipAdministrationUserName.Equals("admin",StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void sso_sync_cookie()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsTrue(h.OAuth.LocalUserCreation.SSO.SynchronizationCookieName.Equals("EvolutionSync", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void oauth_callback()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsTrue(h.OAuth.OauthCallbackUrl.Equals("~/oauth.ashx", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void oauth_cookie()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsTrue(h.OAuth.CookieName.Equals("CS-SDK-User", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void default_language()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsTrue(h.OAuth.DefaultLanguageKey.Equals("en-us", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void anonymous_user()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsTrue(h.OAuth.AnonymousUsername.Equals("Anonymous", StringComparison.CurrentCultureIgnoreCase));
        }
        [Test]
        public void Name_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNull(h.Name);
        }
        [Test]
        public void Url_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNull(h.CommunityServerUrl);
        }
        [Test]
        public void Id_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNull(h.Id);
        }
        [Test]
        public void Credentials_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNull(h.NetworkCredentials);
        }
        [Test]
        public void OauthClient_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNull(h.OAuth.OauthClientId);
        }
        [Test]
        public void OauthClientSecret_null()
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNull(h.OAuth.OauthSecret);
        }
        [Test]
        public void NetworkNull_null()  
        {
            HostConfiguration h = new HostConfiguration();
            Assert.IsNull(h.NetworkCredentials);
        }
     
    }
}
