using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telligent.Rest.SDK.Configuration;

namespace Telligent.RestSDK.Configuration.Tests
{
    [TestFixture]
    public class default_values
    {
        private HostElement  config;
        [TestFixtureSetUp]
        public void Setup()
        {
            config = TelligentConfigurationSection.Current.Hosts["default"];
        }
        [Test]
        public void oauthcallback_default()
        {
            Assert.AreEqual(config.OauthCallbackUrl,"~/oauth.ashx");
        }
        [Test]
        public void clientId_default()
        {
            Assert.AreEqual(config.OauthClientId, "987654321");
        }
        [Test]
        public void secret_default()
        {
            Assert.AreEqual(config.OauthSecret, "98765432123456789");
        }
        [Test]
        public void community_url_default()
        {
            Assert.AreEqual(config.CommunityRootUrl, "http://trunk.local.com/");
        }
        [Test]
        public void enablesso_default()
        {
            Assert.AreEqual(config.EnableSSO, true);
        }
        [Test]
        public void localauth_default()
        {
            Assert.AreEqual(config.UseLocalAuthentication, true);
        }
        [Test]
        public void networkuser_default()
        {
            Assert.AreEqual(config.NetworkUserName, "networkuser");
        }
        [Test]
        public void networkpassword_default()
        {
            Assert.AreEqual(config.NetworkPassword, "password");
        }
        [Test]
        public void networkdomain_default()
        {
            Assert.AreEqual(config.NetworkDomain, "domain");
        }
        [Test]
        public void synccookie_default()
        {
            Assert.AreEqual(config.SynchronizationCookieName, "cookie");
        }
        [Test]
        public void membershipuser_default()
        {
            Assert.AreEqual(config.MembershipAdministrationUsername, "adminuser");
        }
        [Test]
        public void language_default()
        {
            Assert.AreEqual(config.DefaultLanguageKey, "en-GB");
        }
        [Test]
        public void anonymous_default()
        {
            Assert.AreEqual(config.AnonymousUsername, "anonuser");
        }
        [Test]
        public void authcookie_default()
        {
            Assert.AreEqual(config.CookieName, "authcookie");
        }
    }
}
