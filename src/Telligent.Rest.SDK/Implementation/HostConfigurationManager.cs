using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class HostConfigurationManager : IHostConfigurationManager
    {
        private IRestCache _cache;
        private IConfigurationFile _file;
        private static readonly string _fileDataKey = "communityServer::configData";
        private static readonly string _hostCacheKey = "communityServer::SDK::hostconfig::";
        public HostConfigurationManager(IRestCache cache,IConfigurationFile configFile)
        {
            _cache = cache;
            _file = configFile;
        }
        public HostConfiguration GetOptions(string name)
        {

            var config = _cache.Get(GetCacheKey(name));
            if (config != null)
                return (HostConfiguration)config;

            var fileConfig = Read(name);
            if (fileConfig == null)
                throw new ConfigurationErrorsException("No host entry defined in configuraton for '" + name + "'");

            _cache.Put(GetCacheKey(name), fileConfig, 300);

            return fileConfig;

        }



        private HostConfiguration Read(string name)
        {
            var configData = GetConfigData();
            if (string.IsNullOrEmpty(configData))
                throw new ConfigurationErrorsException("Invalid configuration data");

            HostConfiguration config = new HostConfiguration();

            XDocument doc = XDocument.Parse(configData);

            var hostNode = doc.Root.Elements("host").Where(e => e.Attribute("name").Value == name).FirstOrDefault();
            if(hostNode == null)
                throw new ConfigurationErrorsException("No host has been defined in confiuration with the name '" + name + "'");

            if (hostNode.Attribute("id") != null)
                config.Id = Guid.Parse(hostNode.Attribute("id").Value);
            if (hostNode.Attribute("name") != null)
                config.Name = hostNode.Attribute("name").Value;
            if (hostNode.Attribute("communityServerUrl") != null)
                config.CommunityServerUrl = hostNode.Attribute("communityServerUrl").Value;

            string networkUser = null;
            string networkPassword = null;
            string networkDomain = null;

            if (hostNode.Attribute("networkUsername") != null)
                networkUser = hostNode.Attribute("networkUsername").Value;
            if (hostNode.Attribute("networkPassword") != null)
                networkPassword  = hostNode.Attribute("networkPassword").Value;
            if (hostNode.Attribute("networkDomain") != null)
                networkDomain = hostNode.Attribute("networkDomain").Value;

            if (!string.IsNullOrWhiteSpace(networkUser) && !string.IsNullOrWhiteSpace(networkPassword))
            {
                config.NetworkCredentials = new NetworkCredential(networkUser,networkPassword);
                if (!string.IsNullOrWhiteSpace(networkDomain))
                    config.NetworkCredentials.Domain = networkDomain;
            }
            var proxyNode = hostNode.Element("remoteProxy");
            if (proxyNode != null)
            {
                if (proxyNode.Attribute("enabled") != null)
                    config.Proxy.Enabled = bool.Parse(proxyNode.Attribute("enabled").Value);
                if (proxyNode.Attribute("callbackUrl") != null)
                    config.Proxy.CallbackUrl = proxyNode.Attribute("callbackUrl").Value;

              
            }

            var sso = hostNode.Element("sso");
            if (sso != null)
            {
                if (sso.Attribute("enabled") != null)
                    config.SSO.Enabled = bool.Parse(sso.Attribute("enabled").Value);
                if (sso.Attribute("synchronizationCookieName") != null)
                    config.SSO.SynchronizationCookieName = sso.Attribute("synchronizationCookieName").Value;
            }

            var oauthNode = hostNode.Element("oauth");
            if (oauthNode != null)
            {
                if (oauthNode.Attribute("clientId") != null)
                    config.OAuth.OauthClientId = oauthNode.Attribute("clientId").Value;
                if (oauthNode.Attribute("clientSecret") != null)
                    config.OAuth.OauthSecret = oauthNode.Attribute("clientSecret").Value;
                if (oauthNode.Attribute("callbackUrl") != null)
                    config.OAuth.OauthCallbackUrl = oauthNode.Attribute("callbackUrl").Value;
                if (oauthNode.Attribute("cookieName") != null)
                    config.OAuth.CookieName = oauthNode.Attribute("cookieName").Value;
                if (oauthNode.Attribute("defaultLanguage") != null)
                    config.OAuth.DefaultLanguageKey = oauthNode.Attribute("defaultLanguage").Value;
                if (oauthNode.Attribute("anonymousUsername") != null)
                    config.OAuth.AnonymousUsername = oauthNode.Attribute("anonymousUsername").Value;

                var localAuth = oauthNode.Element("localAuthentication");
                if (localAuth != null)
                {
                    if (localAuth.Attribute("enabled") != null)
                        config.OAuth.LocalUserCreation.Enabled = bool.Parse(localAuth.Attribute("enabled").Value);
                    if (localAuth.Attribute("membershipAdministrationUsername") != null)
                        config.OAuth.LocalUserCreation.MembershipAdministrationUserName = localAuth.Attribute("membershipAdministrationUsername").Value;

                  

                   
                }
            }

            return config;
        }

        private string GetConfigData()
        {
            var configData = _cache.Get(_fileDataKey);
            if (configData != null)
                return configData.ToString();

            configData = _file.GetConfigurationData();
            if (string.IsNullOrEmpty(configData.ToString()))
                throw new ConfigurationErrorsException("Invalid configuration data");
            
            _cache.Put(_fileDataKey,configData,300);
            return configData.ToString();
        }
        private string GetCacheKey(string name)
        {
            return string.Concat(_hostCacheKey, name);
        }
        
    }
}