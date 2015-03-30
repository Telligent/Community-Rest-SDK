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

       
        /// <summary>
        /// Called before a request is made to apply the appropriate OAuth header fro the logged in user or default user
        /// </summary>
        /// <param name="request"></param>
        /// <param name="forAccessingUser"></param>
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
        /// <summary>
        /// Teh root url for the community site
        /// </summary>
        public override string EvolutionRootUrl
        {
            get { return _settings.CommunityServerUrl.EndsWith("/") ? _settings.CommunityServerUrl : _settings.CommunityServerUrl + "/"; }
        }
        #endregion

        #region Default Host Settings
        /// <summary>
        /// The name of the host as specified int he configuration file
        /// </summary>
        public virtual string Name
        {
            get { return _settings.Name; }
        }

    /// <summary>
    /// Checks for valid host settings as read by the configuration file
    /// </summary>
    /// <param name="settings"></param>
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

                if(_settings.OAuth.LocalUserCreation.UserResolver == null)
                    throw new ConfigurationErrorsException("UserResolver must be specified when UseLocalAuthentication is true");
            }
            if (settings.OAuth.LocalUserCreation.Enabled && settings.OAuth.LocalUserCreation.SSO.Enabled)
            {
                if (String.IsNullOrEmpty(settings.OAuth.LocalUserCreation.SSO.SynchronizationCookieName))
                    throw new ConfigurationErrorsException("SynchronizationCookieName must be specified when EnableSSO is true");
            }



        }

        #endregion


        #region OAuth Members

        /// <summary>
        /// When true, means that user information is taken from the host site and a user is automatically matched in community or created.
        /// </summary>
        public bool EnableEvolutionUserCreation
        {
            get
            {
                   return   _settings.OAuth.LocalUserCreation.Enabled;
            }
        }
        /// <summary>
        /// The user name used to create users when EnableEvolutionUserCreation is true.
        /// </summary>
        public string EvolutionUserCreationManagementUserName
        {
            get
            {

                    return _settings.OAuth.LocalUserCreation.MembershipAdministrationUserName;
            }
        }
        /// <summary>
        /// The user name used for local user creation.  In this case it invokes a defined instance of ILocalUserResolver to obtain this information
        /// </summary>
        public   string LocalUserName
        {
            get
            {
                var user = _settings.OAuth.LocalUserCreation.UserResolver.GetLocalUserDetails(GetCurrentHttpContext(),this);
                if (user != null)
                    return user.Username;
                return null;
            }
        }
        /// <summary>
        /// The email used for local user creation.  In this case it invokes a defined instance of ILocalUserResolver to obtain this information
        /// </summary>
        public   string LocalUserEmailAddress
        {
            get
            {
                var user = _settings.OAuth.LocalUserCreation.UserResolver.GetLocalUserDetails(GetCurrentHttpContext(), this);
                if (user != null)
                    return user.EmailAddress;
                return null;
            }
        }
        /// <summary>
        /// Additonal Profile fields that can be defined for a local user. In this case it invokes a defined instance of ILocalUserResolver to obtain this information
        /// </summary>
        public   Dictionary<string, string> LocalUserDetails
        {
            get
            {
                var user = _settings.OAuth.LocalUserCreation.UserResolver.GetLocalUserDetails(GetCurrentHttpContext(), this);
                if (user != null )
                    return user.AdditionalData;
                return null;
            }
        }
        /// <summary>
        /// Invoked by the framework when a user cannot be created in the community
        /// </summary>
        /// <param name="username"></param>
        /// <param name="emailAddress"></param>
        /// <param name="userData"></param>
        /// <param name="message"></param>
        /// <param name="errorResponse"></param>
        public virtual void UserCreationFailed(string username, string emailAddress, IDictionary<string, string> userData, string message, ErrorResponse errorResponse)
        {
            LogError(errorResponse.ToString() + ":" + message,
              new ApplicationException(String.Format("Failed to create account for {0},{1}:{2}", username,
                  emailAddress, errorResponse)));
        }

        /// <summary>
        /// The base url of the community, same value as EvolutionRootUrl
        /// </summary>
        public Uri EvolutionBaseUrl
        {
            get { return new Uri(this.EvolutionRootUrl); }
        }
        /// <summary>
        /// The application escaped location of an HttpHandler for handling Oauth reponses.
        /// </summary>
        public Uri LocalOAuthClientHttpHandlerUrl
        {
            get
            {
                return new Uri(GetCurrentHttpContext().Request.Url, GetCurrentHttpContext().Response.ApplyAppPathModifier(_settings.OAuth.OauthCallbackUrl));
            }
        }
        /// <summary>
        /// The community Oauth Client Id as specified in configuration
        /// </summary>
        public string OAuthClientId
        {
            get { return _settings.OAuth.OauthClientId; }
        }
        /// <summary>
        ///  The community Oauth Client secret as specified in configuration
        /// </summary>
        public string OAuthClientSecret
        {
            get { return _settings.OAuth.OauthSecret; }
        }
        /// <summary>
        /// Used to get through a windows challeng on the community side. Defined in configuration
        /// </summary>
        public NetworkCredential EvolutionCredentials
        {
            get { return _settings.NetworkCredentials; }
        }
        /// <summary>
        /// This is used when a user is not signed in, traditionally anonymous.
        /// </summary>
        public string DefaultUserName
        {
            get { return _settings.OAuth.AnonymousUsername; }
        }
        /// <summary>
        /// The default language: MAY BE REMOVED
        /// </summary>
        public string DefaultUserLanguageKey
        {
            get { return _settings.OAuth.DefaultLanguageKey; }
        }
        /// <summary>
        /// Internal method that creates a local cookie identifying the logged on user.
        /// </summary>
        /// <param name="value"></param>
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
        /// <summary>
        /// Reads the cookie set in the SetAuthorizationCookie method
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Fired by the framework once a user is authenticaed by Oauth
        /// </summary>
        /// <param name="state"></param>
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
        /// <summary>
        /// Invoked by the framework when a user fails to authenticate using OAuth
        /// </summary>
        /// <param name="state"></param>
        public virtual void UserLoginFailed(NameValueCollection state)
        {
            LogError("Login Failed", null);
        }
        /// <summary>
        /// Invoked by the framework when a user logs out via oauth
        /// </summary>
        /// <param name="state"></param>
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
        /// <summary>
        /// Invoked by the framework when a user fails to log out via Oauth
        /// </summary>
        /// <param name="state"></param>
        public virtual void UserLogOutFailed(NameValueCollection state)
        {
            LogError("Logout Failed", null);
        }
        /// <summary>
        /// Set via config file, turns on Single Sign on
        /// </summary>
        public virtual bool EnableEvolutionUserSynchronization
        {
            get { return EnableEvolutionUserCreation && _settings.OAuth.LocalUserCreation.SSO.Enabled; }
        }
        /// <summary>
        /// Reads the sync cookie generated by community when EnableEvolutionUserSynchronization is true.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Gets the logged on user and stores it in context for the request.  It will also invoke the login and SSO process if enabled.
        /// </summary>
        /// <returns></returns>
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

      /// <summary>
      /// Provides static access to get a host or force it to be loaded from configuration
      /// </summary>
      /// <param name="name">The name of the host from the configuraton file</param>
      /// <returns>Telligent.Evolution.Extensibility.Version1.Host</returns>
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
