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
    public class override_values
    {
        private HostElement  config;
        [TestFixtureSetUp]
        public void Setup()
        {
            config = TelligentConfigurationSection.Current.Hosts["site3"];
        }
        [Test]
        public void oauthcallback_default()
        {
            Assert.AreEqual(config.OauthCallbackUrl,"~/site3/oauth.ashx");
        }
        [Test]
        public void clientId_default()
        {
            Assert.AreEqual(config.OauthClientId, "123456789");
        }
        [Test]
        public void secret_default()
        {
            Assert.AreEqual(config.OauthSecret, "123456789876543212345678987654321");
        }
        [Test]
        public void community_url_default()
        {
            Assert.AreEqual(config.CommunityRootUrl, "http://site3/");
        }
        [Test]
        public void enablesso_default()
        {
            Assert.AreEqual(config.EnableSSO, false);
        }
        [Test]
        public void localauth_default()
        {
            Assert.AreEqual(config.UseLocalAuthentication, false);
        }
        [Test]
        public void networkuser_default()
        {
            Assert.AreEqual(config.NetworkUserName, "networkuser2");
        }
        [Test]
        public void networkpassword_default()
        {
            Assert.AreEqual(config.NetworkPassword, "password2");
        }
        [Test]
        public void networkdomain_default()
        {
            Assert.AreEqual(config.NetworkDomain, "domain2");
        }
        [Test]
        public void synccookie_default()
        {
            Assert.AreEqual(config.SynchronizationCookieName, "cookie2");
        }
        [Test]
        public void membershipuser_default()
        {
            Assert.AreEqual(config.MembershipAdministrationUsername, "adminuser2");
        }
        [Test]
        public void language_default()
        {
            Assert.AreEqual(config.DefaultLanguageKey, "fr-fr");
        }
        [Test]
        public void anonymous_default()
        {
            Assert.AreEqual(config.AnonymousUsername, "anonuser2");
        }
        [Test]
        public void authcookie_default()
        {
            Assert.AreEqual(config.CookieName, "authcookie2");
        }
    }
}
