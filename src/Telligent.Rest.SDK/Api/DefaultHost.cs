using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK.Configuration;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class Host : RestHost, IUserCreatableOAuthClientConfiguration, IUserSynchronizedOAuthClientConfiguration
    {
        private HostConfiguration  _settings;
        private IHostConfigurationManager _configurationManager = ServiceLocator.Get<IHostConfigurationManager>();
        public Host(string name)
        {
            _settings = _configurationManager.GetOptions(name);
            ValidateSettings(_settings);
        }
        #region Rest Host Members

        public override Guid Id
        {
            get { return _settings.Id; }
        }


        public override void ApplyAuthenticationToHostRequest(System.Net.HttpWebRequest request, bool forAccessingUser)
        {
            Telligent.Evolution.Extensibility.OAuthClient.Version1.User user = null;

            if (forAccessingUser)
                user = GetAccessingUser();

            if (user == null)
                user = Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.GetDefaultUser(this.Id);

            if (user != null)
                request.Headers["Authorization"] = "OAuth " + user.OAuthToken;

            request.Credentials = EvolutionCredentials;
        }

        public override string EvolutionRootUrl
        {
            get { return _settings.CommunityServerUrl.EndsWith("/") ? _settings.CommunityServerUrl : _settings.CommunityServerUrl + "/"; }
        }
        #endregion

        #region Default Host Settings

        public override string Name
        {
            get { return _settings.Name; }
        }

    
        private void ValidateSettings(HostConfiguration settings)
        {
            if (string.IsNullOrEmpty(settings.OAuth.OauthCallbackUrl))
                throw new ConfigurationErrorsException("OauthCallbackUrl must be specified");
            if (string.IsNullOrEmpty(settings.CommunityServerUrl))
                throw new ConfigurationErrorsException("CommunityRootUrl must be specified");
            if (string.IsNullOrEmpty(settings.OAuth.OauthClientId))
                throw new ConfigurationErrorsException("OauthClientId must be specified");
            if (string.IsNullOrEmpty(settings.OAuth.OauthSecret))
                throw new ConfigurationErrorsException("OauthSecret must be specified");
            if (string.IsNullOrEmpty(settings.OAuth.CookieName))
                throw new ConfigurationErrorsException("CookieName must be specified");
         

            if (settings.OAuth.LocalUserCreation.Enabled)
            {
                if (string.IsNullOrEmpty(settings.OAuth.LocalUserCreation.MembershipAdministrationUserName))
                    throw new ConfigurationErrorsException("MembershipAdministrationUserName must be specified when UseLocalAuthentication is true");

              //  if(_settings.LocalUserResolver == n ul)
            }
            if (settings.OAuth.LocalUserCreation.Enabled && settings.OAuth.LocalUserCreation.SSO.Enabled)
            {
                if (string.IsNullOrEmpty(settings.OAuth.LocalUserCreation.SSO.SynchronizationCookieName))
                    throw new ConfigurationErrorsException("SynchronizationCookieName must be specified when EnableSSO is true");
            }



        }

        #endregion


        #region OAuth Members
        public bool EnableEvolutionUserCreation
        {
            get
            {
                   return   _settings.OAuth.LocalUserCreation.Enabled;
            }
        }

        public string EvolutionUserCreationManagementUserName
        {
            get
            {

                    return _settings.OAuth.LocalUserCreation.MembershipAdministrationUserName;
            }
        }

        public virtual string LocalUserName
        {
            get
            {
                if (_settings.OAuth != null && _settings.OAuth.LocalUserCreation != null && _settings.OAuth != null)
                    return _settings.OAuth.LocalUserCreation.MembershipAdministrationUserName;

                return null;
            }
        }

        public virtual string LocalUserEmailAddress
        {
            get { throw new ApplicationException("When UseLocalAuthentication is true, you must override LocalUserEmailAddress in a derived class"); }
        }

        public virtual Dictionary<string, string> LocalUserDetails
        {
            get { return null; }
        }

        public virtual void UserCreationFailed(string username, string emailAddress, IDictionary<string, string> userData, string message, ErrorResponse errorResponse)
        {
            LogError(errorResponse.ToString() + ":" + message,
              new ApplicationException(string.Format("Failed to create account for {0},{1}:{2}", username,
                  emailAddress, errorResponse)));
        }


        public Uri EvolutionBaseUrl
        {
            get { return new Uri(this.EvolutionRootUrl); }
        }

        public Uri LocalOAuthClientHttpHandlerUrl
        {
            get
            {
                return new Uri(GetCurrentHttpContext().Request.Url, GetCurrentHttpContext().Response.ApplyAppPathModifier(_settings.OAuth.OauthCallbackUrl));
            }
        }

        public string OAuthClientId
        {
            get { return _settings.OAuth.OauthClientId; }
        }

        public string OAuthClientSecret
        {
            get { return _settings.OAuth.OauthSecret; }
        }

        public NetworkCredential EvolutionCredentials
        {
            get { return _settings.NetworkCredentials; }
        }

        public string DefaultUserName
        {
            get { return _settings.OAuth.AnonymousUsername; }
        }

        public string DefaultUserLanguageKey
        {
            get { return _settings.OAuth.DefaultLanguageKey; }
        }

        public virtual void SetAuthorizationCookie(string value)
        {
            HttpContextBase context = GetCurrentHttpContext();
            HttpResponseBase response = null;
            HttpRequestBase request = null;

            try
            {
                response = context.Response;
                request = context.Request;
            }
            catch
            {
            }

            if (response != null && request != null)
            {
                var cookie = response.Cookies[_settings.OAuth.CookieName];

                if (cookie != null)
                    response.Cookies.Remove(cookie.Name);

                cookie = request.Cookies[_settings.OAuth.CookieName];

                if (cookie == null)
                    cookie = new HttpCookie(_settings.OAuth.CookieName);

                cookie.HttpOnly = true;
                if (!string.IsNullOrEmpty(value))
                {
                    cookie.Value = value;
                    cookie.Expires = DateTime.Now.AddDays(30);
                }
                else
                {
                    cookie.Value = string.Empty;
                    cookie.Expires = DateTime.Now.AddDays(-30);
                }

                response.Cookies.Add(cookie);
            }
        }

        public virtual string GetAuthorizationCookieValue()
        {
            HttpContextBase context = GetCurrentHttpContext();
            HttpRequestBase request;

            try
            {
                request = context.Request;
            }
            catch
            {
                return null;
            }

            if (request != null)
            {
                var cookie = request.Cookies[_settings.OAuth.CookieName];
                if (cookie != null)
                    return cookie.Value;
            }

            return null;
        }

        public virtual void UserLoggedIn(System.Collections.Specialized.NameValueCollection state)
        {
            var usr = Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.GetAuthenticatedUser(this.Id);
            string rtnUrl = null;
            if (state != null)
            {
                rtnUrl = state["rtn"];
            }

            if (string.IsNullOrEmpty(rtnUrl))
                rtnUrl = "~/";

            GetCurrentHttpContext().Response.Redirect(rtnUrl);
        }

        public virtual void UserLoginFailed(System.Collections.Specialized.NameValueCollection state)
        {
            LogError("Login Failed", null);
        }

        public virtual void UserLoggedOut(System.Collections.Specialized.NameValueCollection state)
        {
            string rtnUrl = null;
            if (state != null)
            {
                rtnUrl = state["rtn"];
            }

            if (string.IsNullOrEmpty(rtnUrl))
                rtnUrl = "~/";

            GetCurrentHttpContext().Response.Redirect(rtnUrl);
        }

        public virtual void UserLogOutFailed(System.Collections.Specialized.NameValueCollection state)
        {
            LogError("Logout Failed", null);
        }

        public virtual bool EnableEvolutionUserSynchronization
        {
            get { return EnableEvolutionUserCreation && _settings.OAuth.LocalUserCreation.SSO.Enabled; }
        }

        public virtual string GetEvolutionUserSynchronizationCookieValue()
        {
            var context = GetCurrentHttpContext();
            if (context != null)
            {
                var cookie = context.Request.Cookies[_settings.OAuth.LocalUserCreation.SSO.SynchronizationCookieName];
                if (cookie != null)
                    return cookie.Value;
            }
            return null;
        }
        #endregion
        #region Helpers
        private User GetAccessingUser()
        {
            bool isGet = false;
            var context = GetCurrentHttpContext();
            var url = "";
            try
            {
                isGet = context.Request.HttpMethod.ToUpper() == "GET";
                url = context.Request.Url.OriginalString;
            }
            catch (Exception)
            {

                isGet = false;
            }

            var user = context.Items["SDK-User"] as Telligent.Evolution.Extensibility.OAuthClient.Version1.User;
            if (user == null)
            {

                if (isGet)
                {
                    NameValueCollection col = new NameValueCollection();
                    col.Add("rtn", url);
                    user =
                        Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.GetAuthenticatedUser(
                            this.Id, col, redirectUrl => context.Response.Redirect(redirectUrl.OriginalString, true));
                }
                else
                    user =
                        Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.GetAuthenticatedUser(
                            this.Id);

                if (user == null)
                    user =
                        Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.GetDefaultUser(
                            this.Id);

                context.Items["SDK-User"] = user;
            }

            return user;
        }
        #endregion
    }
    #region Configuration

    //public interface IConfigurationOptions
    //{
    //    string Name { get; set; }
    //    string OauthCallbackUrl { get; set; }
    //    string CommunityRootUrl { get; set; }
    //    NetworkCredential NetworkCredentials { get; set; }
    //    string SynchronizationCookieName { get; set; }
    //    bool EnableSSO { get; set; }
    //    bool UseLocalAuthentication { get; set; }
    //    string MembershipAdministrationUserName { get; set; }
    //    string DefaultLanguageKey { get; set; }
    //    string AnonymousUsername { get; set; }
    //    string OauthClientId { get; set; }
    //    string OauthSecret { get; set; }
    //    Guid? Id { get; set; }
    //    string CookieName { get; set; }
    //    Func<HttpContextBase, Host, LocalUser> LocalUserResolver { get; set; }
    //}

    //public class ConfigFileConfigurationSettings : IConfigurationOptions
    //{
    //    public ConfigFileConfigurationSettings()
    //    {
    //        DefaultLanguageKey = "en-US";
    //        MembershipAdministrationUserName = "admin";
    //        EnableSSO = false;
    //        UseLocalAuthentication = false;
    //        AnonymousUsername = "Anonymous";
    //        SynchronizationCookieName = "EvolutionSync";
    //        CookieName = "communityUser";
    //    }
    //    public string OauthCallbackUrl { get; set; }
    //    public string CommunityRootUrl { get; set; }
    //    public NetworkCredential NetworkCredentials { get; set; }
    //    public string SynchronizationCookieName { get; set; }
    //    public bool EnableSSO { get; set; }
    //    public bool UseLocalAuthentication { get; set; }
    //    public string MembershipAdministrationUserName { get; set; }
    //    public string DefaultLanguageKey { get; set; }
    //    public string AnonymousUsername { get; set; }
    //    public string OauthClientId { get; set; }
    //    public string OauthSecret { get; set; }
    //    public Guid? Id { get; set; }
    //    public string CookieName { get; set; }
    //    public Func<HttpContextBase, Host, LocalUser> LocalUserResolver { get; set; }


    //}

    public interface IHostConfigurationManager
    {
        HostConfiguration GetOptions(string name);
    }

    public class HostConfigurationManager : IHostConfigurationManager
    {
        private IRestCache _cache;
        private IConfigurationFile _file;
        private static readonly string _fileDataKey = "communityServer::configData";
        private static readonly string _hostCacheKey = "communityServer::SDK::host::";
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

    public interface IConfigurationFile
    {
        string GetConfigurationData();
    }

    public class WebConfigurationFile : IConfigurationFile
    {
        private HttpContextBase _context;
        public WebConfigurationFile(HttpContextBase context)
        {
            _context = context;
        }

        public string GetConfigurationData()
        {
            var path = _context.Server.MapPath("~/");
            var configFile = Path.Combine(path, "communityServer_SDK.config");
            if(!File.Exists(configFile))
                throw new ConfigurationErrorsException("Cannot find valid config file in web root");

            using (FileStream stream = new FileStream(configFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            
        }
    }

    public class FileSystemConfigurationFile:IConfigurationFile
    {

        public string GetConfigurationData()
        {
            var configFile = System.Configuration.ConfigurationManager.AppSettings["communityServer:SDK:configPath"];
            if (string.IsNullOrWhiteSpace(configFile))
                throw new ConfigurationErrorsException("Cannot find a valid configuration path in the application configuration app settings.  Add the full disk path to key communityServer:SDK:configPath");

            if (!File.Exists(configFile))
                throw new ConfigurationErrorsException("Cannot find valid config file at'" + configFile + "'");

            using (FileStream stream = new FileStream(configFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
    public class HostConfiguration
    {
        public HostConfiguration()
        {
                OAuth = new OAuthConfiguration();
        }
        public Guid Id { get; set; }
        public string CommunityServerUrl { get; set; }
        public string Name { get; set; }
        public NetworkCredential NetworkCredentials { get; set; }
        public OAuthConfiguration OAuth { get; set; }
    }

    public class OAuthConfiguration
    {
        public OAuthConfiguration()
        {
            DefaultLanguageKey = "en-us";
            AnonymousUsername = "Anonymous";
            CookieName = "CS-SDK-User";
            LocalUserCreation = new LocalUserConfiguration();
        }
        public string DefaultLanguageKey { get; set; }
        public string AnonymousUsername { get; set; }
        public string OauthClientId { get; set; }
        public string OauthSecret { get; set; }
        public string OauthCallbackUrl { get; set; }
        public string CookieName { get; set; }
        public LocalUserConfiguration LocalUserCreation { get; set; }

    }

    public class SSOConfiguration
    {
        public SSOConfiguration()
        {
            Enabled = false;
            SynchronizationCookieName = "EvolutionSync";
        }    
        public bool Enabled { get; set; }
        public string SynchronizationCookieName { get; set; }
    }

    public class LocalUserConfiguration
    {
        public LocalUserConfiguration()
        {
            MembershipAdministrationUserName = "admin";
            Enabled = false;
            SSO = new SSOConfiguration();
        }
        public string MembershipAdministrationUserName { get; set; }
        public Func<HttpContextBase, LocalUser> UserResolver { get; set; }
        public bool Enabled { get; set; }
        public SSOConfiguration SSO { get; set; }
    }
    #endregion

    public class LocalUser
    {
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public IDictionary<string, string> AdditionalData { get; set; }

    }


}
