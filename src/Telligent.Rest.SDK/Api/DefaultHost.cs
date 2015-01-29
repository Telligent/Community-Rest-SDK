using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Rest.SDK.Configuration;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class DefaultHost : RestHost, IOAuthClientConfiguration, IUserCreatableOAuthClientConfiguration, IUserSynchronizedOAuthClientConfiguration
    {
        private ConfigurationSettings _settings;


        public DefaultHost(string name)
        {
            Name = name;
            _settings = new ConfigurationSettings();
            LoadConfiguration(_settings);
            ValidateSettings(_settings);
        }
        #region Rest Host Members
        public override Guid Id
        {
            get { return Settings.Id.Value; }
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
            get { return Settings.CommunityRootUrl.EndsWith("/") ? Settings.CommunityRootUrl : Settings.CommunityRootUrl + "/"; }
        }
        #endregion

        #region Default Host Settings
        protected ConfigurationSettings Settings
        {
            get { return _settings; }
        }
        public virtual string Name { get; private set; }

        public virtual void LoadConfiguration(ConfigurationSettings settings)
        {
            var config = TelligentConfigurationSection.Current;
            if (config == null)
                throw new ConfigurationErrorsException("Cannot find Configuration in config file.");

            var hostSection = config.Hosts[this.Name];
            if (hostSection == null)
                throw new ConfigurationErrorsException("Cannot find configuration for host '" + this.Name + "'");

            _settings.AnonymousUsername = hostSection.AnonymousUsername;
            _settings.CommunityRootUrl = hostSection.CommunityRootUrl;
            _settings.DefaultLanguageKey = hostSection.DefaultLanguageKey;
            _settings.EnableSSO = hostSection.EnableSSO.GetValueOrDefault(false);
            _settings.MembershipAdministrationUserName = hostSection.MembershipAdministrationUsername;
            _settings.OauthCallbackUrl = hostSection.OauthCallbackUrl;
            _settings.OauthClientId = hostSection.OauthClientId;
            _settings.OauthSecret = hostSection.OauthSecret;
            _settings.UseLocalAuthentication = hostSection.UseLocalAuthentication.GetValueOrDefault(false);
            _settings.Id = hostSection.Id;
            _settings.SynchronizationCookieName = hostSection.SynchronizationCookieName;
            _settings.CookieName = hostSection.CookieName;

            if (!string.IsNullOrWhiteSpace(hostSection.NetworkUserName) &&
                !string.IsNullOrWhiteSpace(hostSection.NetworkPassword))
            {
                settings.NetworkCredentials = new NetworkCredential(hostSection.NetworkUserName, hostSection.NetworkPassword);
                if (!string.IsNullOrWhiteSpace(hostSection.NetworkDomain))
                    settings.NetworkCredentials.Domain = hostSection.NetworkDomain;
            }

        }

        private void ValidateSettings(ConfigurationSettings settings)
        {
            if (string.IsNullOrEmpty(settings.OauthCallbackUrl))
                throw new ConfigurationErrorsException("OauthCallbackUrl must be specified");
            if (string.IsNullOrEmpty(settings.CommunityRootUrl))
                throw new ConfigurationErrorsException("CommunityRootUrl must be specified");
            if (string.IsNullOrEmpty(settings.OauthClientId))
                throw new ConfigurationErrorsException("OauthClientId must be specified");
            if (string.IsNullOrEmpty(settings.OauthSecret))
                throw new ConfigurationErrorsException("OauthSecret must be specified");
            if (string.IsNullOrEmpty(settings.CookieName))
                throw new ConfigurationErrorsException("CookieName must be specified");
            if (!_settings.Id.HasValue)
                throw new ConfigurationErrorsException("Id must be specified");

            if (_settings.UseLocalAuthentication)
            {
                if (string.IsNullOrEmpty(settings.MembershipAdministrationUserName))
                    throw new ConfigurationErrorsException("MembershipAdministrationUserName must be specified when UseLocalAuthentication is true");
            }
            if (_settings.EnableSSO)
            {
                if (string.IsNullOrEmpty(settings.SynchronizationCookieName))
                    throw new ConfigurationErrorsException("SynchronizationCookieName must be specified when EnableSSO is true");
            }



        }

        public static DefaultHost GetHost(string name, bool autoRegister = true)
        {
            var config = TelligentConfigurationSection.Current;
            if (config == null)
                throw new ConfigurationErrorsException("Cannot find Configuration in config file.");

            var hostSection = config.Hosts[name];
            if (hostSection == null)
                throw new ConfigurationErrorsException("Cannot find configuration for host '" + name + "'");

            var host = Get(hostSection.Id);
            if (host == null && autoRegister)
            {
                host = new DefaultHost(name);
                RegisterHost((DefaultHost)host);
            }

            return host as DefaultHost;

        }

        public static void RegisterHost(DefaultHost host)
        {
            Register(host);
            if (host.EnableEvolutionUserCreation || host.EnableEvolutionUserSynchronization)
                OAuthAuthentication.RegisterConfiguration(host);
        }
        #endregion


        #region OAuth Members
        public bool EnableEvolutionUserCreation
        {
            get { return Settings.UseLocalAuthentication; }
        }

        public string EvolutionUserCreationManagementUserName
        {
            get { return Settings.MembershipAdministrationUserName; }
        }

        public virtual string LocalUserName
        {
            get { return GetCurrentHttpContext().User.Identity.Name; }
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
              return new Uri(GetCurrentHttpContext().Request.Url, GetCurrentHttpContext().Response.ApplyAppPathModifier(Settings.OauthCallbackUrl));
            }
        }

        public string OAuthClientId
        {
            get {return Settings.OauthClientId; }
        }

        public string OAuthClientSecret
        {
            get { return Settings.OauthSecret; }
        }

        public NetworkCredential EvolutionCredentials
        {
            get {return Settings.NetworkCredentials; }
        }

        public string DefaultUserName
        {
            get { return Settings.AnonymousUsername; }
        }

        public string DefaultUserLanguageKey
        {
            get { return Settings.DefaultLanguageKey; }
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
                var cookie = response.Cookies[Settings.CookieName];

                if (cookie != null)
                    response.Cookies.Remove(cookie.Name);

                cookie = request.Cookies[Settings.CookieName];

                if (cookie == null)
                    cookie = new HttpCookie(Settings.CookieName);

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
                var cookie = request.Cookies[Settings.CookieName];
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
           LogError("Login Failed",null);
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
            get { return Settings.EnableSSO; }
        }

        public virtual string GetEvolutionUserSynchronizationCookieValue()
        {
            var context = GetCurrentHttpContext();
            if (context != null)
            {
                var cookie = context.Request.Cookies[Settings.SynchronizationCookieName];
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

    public class ConfigurationSettings
    {
        public ConfigurationSettings()
        {
            DefaultLanguageKey = "en-US";
            MembershipAdministrationUserName = "admin";
            EnableSSO = false;
            UseLocalAuthentication = false;
            AnonymousUsername = "Anonymous";
            SynchronizationCookieName = "EvolutionSync";
            CookieName = "communityUser";
        }
        public string OauthCallbackUrl { get; set; }
        public string CommunityRootUrl { get; set; }
        public NetworkCredential NetworkCredentials { get; set; }
        public string SynchronizationCookieName { get; set; }
        public bool EnableSSO { get; set; }
        public bool UseLocalAuthentication { get; set; }
        public string MembershipAdministrationUserName { get; set; }
        public string DefaultLanguageKey { get; set; }
        public string AnonymousUsername { get; set; }
        public string OauthClientId { get; set; }
        public string OauthSecret { get; set; }
        public Guid? Id { get; set; }
        public string CookieName { get; set; }
    }
    #endregion




}
