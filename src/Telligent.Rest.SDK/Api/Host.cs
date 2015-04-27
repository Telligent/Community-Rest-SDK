using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Implementations;
using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK;
using Telligent.Rest.SDK.Implementation;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class Host : RestHost, IUserCreatableOAuthClientConfiguration, IUserSynchronizedOAuthClientConfiguration
    {
        private HostConfiguration _settings;
        private IHostConfigurationManager _configurationManager;
        private IProxyService _proxyService;
        private IUrlManipulationService _urlManipulation;

        internal Host(string name, IProxyService proxyService, IHostConfigurationManager hostConfigurationManager, IUrlManipulationService urlManipulationService)
        {
            _configurationManager = hostConfigurationManager;
            _urlManipulation = urlManipulationService;
            _proxyService = proxyService;
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


            }
            if (settings.OAuth.LocalUserCreation.Enabled && settings.SSO.Enabled)
            {
                if (String.IsNullOrEmpty(settings.SSO.SynchronizationCookieName))
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
                return _settings.OAuth.LocalUserCreation.Enabled;
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
        /// The user name used for local user creation.  Requires you provide an implementation of the ResolveLocalUser function of the host.
        /// </summary>
        public string LocalUserName
        {
            get
            {
                if (_settings.OAuth.LocalUserCreation.Enabled)
                {
                    if (this.ResolveLocalUser != null)
                    {
                        var user = this.ResolveLocalUser(this, new ResolveLocalUserArgs(GetCurrentHttpContext()));
                        if (user != null)
                            return user.Username;

                    }
                    else
                    {
                        throw new ApplicationException(
                            "You must provide a definition for Host.ResolveLocalUser when using local user creation");
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// The email used for local user creation.  Requires you provide an implementation of the ResolveLocalUser function of the host.
        /// </summary>
        public string LocalUserEmailAddress
        {
            get
            {
                if (_settings.OAuth.LocalUserCreation.Enabled)
                {
                    if (this.ResolveLocalUser != null)
                    {
                        var user = this.ResolveLocalUser(this, new ResolveLocalUserArgs(GetCurrentHttpContext()));
                        if (user != null)
                            return user.EmailAddress;

                    }
                    else
                    {
                        throw new ApplicationException(
                            "You must provide a definition for Host.ResolveLocalUser when using local user creation");
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Additonal Profile fields that can be defined for a local user.  Requires you provide an implementation of the ResolveLocalUser function of the host
        /// </summary>
        public Dictionary<string, string> LocalUserDetails
        {
            get
            {
                if (_settings.OAuth.LocalUserCreation.Enabled)
                {
                    if (this.ResolveLocalUser != null)
                    {
                        var user = this.ResolveLocalUser(this, new ResolveLocalUserArgs(GetCurrentHttpContext()));
                        if (user != null)
                            return user.AdditionalData;

                    }
                    else
                    {
                        throw new ApplicationException("You must provide a definition for Host.ResolveLocalUser when using local user creation");
                    }
                }

                else
                    throw new ApplicationException();

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

            if (this.CommunityUserCreationFailed != null)
                CommunityUserCreationFailed(this, errorResponse, new UserCreationFailedArgs(GetCurrentHttpContext())
                {
                    UserName = username,
                    EmailAddress = emailAddress,
                    AdditionalData = userData,
                    Message = message
                });
            else
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

            if (this.OAuthUserLoggedIn != null)
            {
                OAuthUserLoggedIn(this, new OAuthUserLoggedInArgs(GetCurrentHttpContext()) { State =state,User = usr});
            }
            else
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
           
        }
        /// <summary>
        /// Invoked by the framework when a user fails to authenticate using OAuth
        /// </summary>
        /// <param name="state"></param>
        public virtual void UserLoginFailed(NameValueCollection state)
        {
            LogError("Login Failed", null);
            if(this.OAuthLoginFailed != null)
                OAuthLoginFailed(this, new OAuthLoginFailedArgs(GetCurrentHttpContext()) { State = state});
        }
        /// <summary>
        /// Invoked by the framework when a user logs out via oauth
        /// </summary>
        /// <param name="state"></param>
        public virtual void UserLoggedOut(NameValueCollection state)
        {
            if (this.OAuthUserLoggedOut != null)
            {
                OAuthUserLoggedOut(this, new OAuthUserLoggedOutArgs(GetCurrentHttpContext()) { State = state});
            }
            else
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
           
        }
        /// <summary>
        /// Invoked by the framework when a user fails to log out via Oauth
        /// </summary>
        /// <param name="state"></param>
        public virtual void UserLogOutFailed(NameValueCollection state)
        {
            LogError("Logout Failed", null);
            if (this.OAuthLogoutFailed != null)
                OAuthLogoutFailed(this, new OAuthLogoutFailedArgs(GetCurrentHttpContext()) { State = state });
        }
        /// <summary>
        /// Set via config file, turns on Single Sign on
        /// </summary>
        public virtual bool EnableEvolutionUserSynchronization
        {
            get { return  _settings.SSO.Enabled; }
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
                var cookie = context.Request.Cookies[_settings.SSO.SynchronizationCookieName];
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

            var newHost = new Host(name, ServiceLocator.Get<IProxyService>(), ServiceLocator.Get<IHostConfigurationManager>(), ServiceLocator.Get<IUrlManipulationService>());
            OAuthAuthentication.RegisterConfiguration(newHost);

            return newHost;

        }
        #region Proxying
        internal string ResolveRemoteUrlsToHostUrls(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var context = GetCurrentHttpContext();
            var prefixUrl = context.Response.ApplyAppPathModifier("~/");
            var cacheKey = string.Concat("ResolveRemoteUrlsToHostUrls-Pattern-", prefixUrl);
            Func<string, string> resolveUrls = Cache.Get(cacheKey) as Func<string, string>;
            if (resolveUrls == null)
            {
                var proxyPath = _urlManipulation.GetCallbackUrl(GetCurrentHttpContext(), CallbackUrl, Name, "", "");
                var proxyUri = new Uri(GetCurrentHttpContext().Request.Url, _urlManipulation.GetCallbackUrl(GetCurrentHttpContext(), CallbackUrl, Name, "", ""));
                var remoteUrls = new Regex(
                    string.Concat(
                        "(?:(?<notencoded>",
                        Regex.Escape(proxyUri.AbsoluteUri),
                        "|",
                        Regex.Escape(proxyUri.AbsolutePath.Replace(prefixUrl, "~/")),
                        "|",
                        Regex.Escape(proxyUri.AbsolutePath),
                        @")[a-z0-9\/\&\?\=\%\._\-;~\+]*|(?<encoded>",
                        Regex.Escape(Uri.EscapeDataString(proxyUri.AbsoluteUri)).Replace("%20", "(?:\\+|%20)"), // Uri.EscapeDataString converts spaces to %20, but + is also valid
                        "|",
                        Regex.Escape(Uri.EscapeDataString(proxyUri.AbsolutePath.Replace(prefixUrl, "~/"))).Replace("%20", "(?:\\+|%20)"), // Uri.EscapeDataString converts spaces to %20, but + is also valid
                        "|",
                        Regex.Escape(Uri.EscapeDataString(proxyUri.AbsolutePath)).Replace("%20", "(?:\\+|%20)"), // Uri.EscapeDataString converts spaces to %20, but + is also valid
                        @")(?:(?!%22)[a-z0-9\/\%\._\-;~\+])*)"
                    ), RegexOptions.Singleline | RegexOptions.IgnoreCase);
                resolveUrls = new Func<string, string>(input =>
                    remoteUrls.Replace(input, new MatchEvaluator(match =>
                    {
                        if (match.Groups["encoded"].Success)
                        {
                            // decode, process, and re-encode
                            var unescaped = Uri.UnescapeDataString(match.Value);
                            return Uri.EscapeDataString(_proxyService.MakeFullUrl(this, _proxyService.UnescapeRemoteUrl(this, unescaped.Substring(unescaped.LastIndexOf(proxyPath) + proxyPath.Length))));
                        }
                        else
                            return _proxyService.MakeFullUrl(this, _proxyService.UnescapeRemoteUrl(this, match.Value.Substring(match.Value.LastIndexOf(proxyPath) + proxyPath.Length)));
                    }))
                );

                Cache.Put(cacheKey, resolveUrls, 30 * 60);
            }

            return resolveUrls(text);
        }
        public override void ApplyRemoteHeadersToRequest(System.Net.HttpWebRequest request)
        {
            if (_settings.Proxy.Enabled)
            {
                request.Headers["X-Remote-Redirect-Url"] = (new Uri(GetCurrentHttpContext().Request.Url, _urlManipulation.GetCallbackUrl(GetCurrentHttpContext(), CallbackUrl, Name, "", ""))).AbsoluteUri;
                request.Headers["X-Remote-UrlEncode-Redirects"] = "False";
            }

        }


        private string CallbackUrl
        {
            get { return GetCurrentHttpContext().Response.ApplyAppPathModifier(_settings.Proxy.CallbackUrl); }
        }

        internal string GetEvolutionRedirectUrl(string url)
        {
            if (_settings.Proxy.Enabled)
            {
                if (this.GetRedirectUrl != null)
                {
                    var redirectUrl = this.GetRedirectUrl(this, url, new RedirectUrlArgs(GetCurrentHttpContext()));
                    if (!string.IsNullOrEmpty(redirectUrl))
                        return redirectUrl;
                }
            }

            return _proxyService.MakeFullUrl(this, url);
        }
        #endregion


        #region  Delegates

        public Func<Host, ResolveLocalUserArgs, LocalUser> ResolveLocalUser { private get; set; }
        public Func<Host, string, RedirectUrlArgs, string> GetRedirectUrl { private get; set; }
        public Action<Host, ErrorResponse, UserCreationFailedArgs> CommunityUserCreationFailed { private get; set; }
        public Action<Host,  OAuthLoginFailedArgs> OAuthLoginFailed { private get; set; }
        public Action<Host, OAuthLogoutFailedArgs> OAuthLogoutFailed { private get; set; }
        public Action<Host, OAuthUserLoggedOutArgs> OAuthUserLoggedOut { private get; set; }
        public Action<Host, OAuthUserLoggedInArgs> OAuthUserLoggedIn { private get; set; }

        #endregion

        #region Login and Logout APIs
        public virtual string ProcessSynchronizedLogin(string returnUrl)
        {

            if ( _settings.SSO.Enabled)
            {

                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("rtn", returnUrl);
                var evoUser = Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.GetAuthenticatedUser(this.Name, nvc, (x) => returnUrl = x.OriginalString);
            }

            return returnUrl;
        }

        public string ProcessSynchronizedLogout(string returnUrl)
        {

            if ( _settings.SSO.Enabled)
            {
               
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("rtn", returnUrl);
                var evoUri = Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.EvolutionLogOut(Name, nvc);
                if (evoUri != null)
                    returnUrl = evoUri.OriginalString;
            }
            Telligent.Evolution.Extensibility.OAuthClient.Version1.OAuthAuthentication.LogOut(Name);

            return returnUrl;
        }

        public string CommunityOAuthLoginUrl(string returnUrl)
        {

            if (!_settings.OAuth.LocalUserCreation.Enabled)
            {
                if (string.IsNullOrEmpty(returnUrl))
                    returnUrl = HttpContext.Current.Request.Url.OriginalString;
               
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("rtn", HttpContext.Current.Request.Url.OriginalString);
                return OAuthAuthentication.Login(Name, nvc).OriginalString;
            }

            return null;
        }
        public void CommunityOAuthLogout()
        {

            if (!_settings.OAuth.LocalUserCreation.Enabled)
            {
             
               OAuthAuthentication.LogOut(Name);
            }

        }
        #endregion

    }



}
