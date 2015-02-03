using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Implementations;
using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK.Configuration;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class Host : RestHost, IUserCreatableOAuthClientConfiguration, IUserSynchronizedOAuthClientConfiguration
    {
        private HostConfiguration  _settings;
        private IHostConfigurationManager _configurationManager = ServiceLocator.Get<IHostConfigurationManager>();
        internal Host(string name)
        {
            _settings = _configurationManager.GetOptions(name);
            ValidateSettings(_settings);
        }
        #region Rest Host Members

       

        public override void ApplyAuthenticationToHostRequest(HttpWebRequest request, bool forAccessingUser)
        {
            User user = null;

            if (forAccessingUser)
                user = GetAccessingUser();

            if (user == null)
                user = OAuthAuthentication.GetDefaultUser(this.Name);

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

        public virtual string Name
        {
            get { return _settings.Name; }
        }

    
        private void ValidateSettings(HostConfiguration settings)
        {
            if (String.IsNullOrEmpty(settings.OAuth.OauthCallbackUrl))
                throw new ConfigurationErrorsException("OauthCallbackUrl must be specified");
            if (String.IsNullOrEmpty(settings.CommunityServerUrl))
                throw new ConfigurationErrorsException("CommunityRootUrl must be specified");
            if (String.IsNullOrEmpty(settings.OAuth.OauthClientId))
                throw new ConfigurationErrorsException("OauthClientId must be specified");
            if (String.IsNullOrEmpty(settings.OAuth.OauthSecret))
                throw new ConfigurationErrorsException("OauthSecret must be specified");
            if (String.IsNullOrEmpty(settings.OAuth.CookieName))
                throw new ConfigurationErrorsException("CookieName must be specified");
         

            if (settings.OAuth.LocalUserCreation.Enabled)
            {
                if (String.IsNullOrEmpty(settings.OAuth.LocalUserCreation.MembershipAdministrationUserName))
                    throw new ConfigurationErrorsException("MembershipAdministrationUserName must be specified when UseLocalAuthentication is true");

              //  if(_settings.LocalUserResolver == n ul)
            }
            if (settings.OAuth.LocalUserCreation.Enabled && settings.OAuth.LocalUserCreation.SSO.Enabled)
            {
                if (String.IsNullOrEmpty(settings.OAuth.LocalUserCreation.SSO.SynchronizationCookieName))
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

        public   string LocalUserName
        {
            get
            {
                if (_settings.OAuth != null && _settings.OAuth.LocalUserCreation != null && _settings.OAuth != null)
                    return _settings.OAuth.LocalUserCreation.MembershipAdministrationUserName;

                return null;
            }
        }

        public   string LocalUserEmailAddress
        {
            get { throw new ApplicationException("When UseLocalAuthentication is true, you must override LocalUserEmailAddress in a derived class"); }
        }

        public   Dictionary<string, string> LocalUserDetails
        {
            get { return null; }
        }

        public virtual void UserCreationFailed(string username, string emailAddress, IDictionary<string, string> userData, string message, ErrorResponse errorResponse)
        {
            LogError(errorResponse.ToString() + ":" + message,
              new ApplicationException(String.Format("Failed to create account for {0},{1}:{2}", username,
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
                if (!String.IsNullOrEmpty(value))
                {
                    cookie.Value = value;
                    cookie.Expires = DateTime.Now.AddDays(30);
                }
                else
                {
                    cookie.Value = String.Empty;
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

        public virtual void UserLoggedIn(NameValueCollection state)
        {
            var usr = OAuthAuthentication.GetAuthenticatedUser(this.Name);
            string rtnUrl = null;
            if (state != null)
            {
                rtnUrl = state["rtn"];
            }

            if (String.IsNullOrEmpty(rtnUrl))
                rtnUrl = "~/";

            GetCurrentHttpContext().Response.Redirect(rtnUrl);
        }

        public virtual void UserLoginFailed(NameValueCollection state)
        {
            LogError("Login Failed", null);
        }

        public virtual void UserLoggedOut(NameValueCollection state)
        {
            string rtnUrl = null;
            if (state != null)
            {
                rtnUrl = state["rtn"];
            }

            if (String.IsNullOrEmpty(rtnUrl))
                rtnUrl = "~/";

            GetCurrentHttpContext().Response.Redirect(rtnUrl);
        }

        public virtual void UserLogOutFailed(NameValueCollection state)
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

            var user = context.Items["SDK-User"] as User;
            if (user == null)
            {

                if (isGet)
                {
                    NameValueCollection col = new NameValueCollection();
                    col.Add("rtn", url);
                    user =
                        OAuthAuthentication.GetAuthenticatedUser(
                            this.Name, col, redirectUrl => context.Response.Redirect(redirectUrl.OriginalString, true));
                }
                else
                    user =
                        OAuthAuthentication.GetAuthenticatedUser(
                            this.Name);

                if (user == null)
                    user =
                        OAuthAuthentication.GetDefaultUser(
                            this.Name);

                context.Items["SDK-User"] = user;
            }

            return user;
        }
        #endregion

      
        public static Host Get(string name)
        {
            var cachedHost = OAuthAuthentication.GetConfiguration(name);
            if (cachedHost != null)
                return cachedHost as Host;

            var newHost = new Host(name);
            OAuthAuthentication.RegisterConfiguration(newHost);

            return newHost;

        }

       
    }
   
}
