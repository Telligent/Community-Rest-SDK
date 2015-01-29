using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Rest.SDK.Configuration;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
     public class DefaultHost:RestHost,IOAuthClientConfiguration,IUserCreatableOAuthClientConfiguration,IUserSynchronizedOAuthClientConfiguration
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
            //Apply an Oauth token here
        }

        public override string EvolutionRootUrl
        {
            get {return Settings.CommunityRootUrl; }
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
             if(config == null)
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

         }

         private void ValidateSettings(ConfigurationSettings settings)
         {
             if(string.IsNullOrEmpty(settings.OauthCallbackUrl))
                 throw new ConfigurationErrorsException("OauthCallbackUrl must be specified");
             if (string.IsNullOrEmpty(settings.CommunityRootUrl))
                 throw new ConfigurationErrorsException("CommunityRootUrl must be specified");
             if (string.IsNullOrEmpty(settings.OauthClientId))
                 throw new ConfigurationErrorsException("OauthClientId must be specified");
             if (string.IsNullOrEmpty(settings.OauthSecret))
                 throw new ConfigurationErrorsException("OauthSecret must be specified");
             if(!_settings.Id.HasValue)
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
             get {return Settings.MembershipAdministrationUserName; }
         }

         public virtual string LocalUserName
         {
             get { return GetCurrentHttpContext().User.Identity.Name; }
         }

         public virtual string LocalUserEmailAddress
         {
             get { throw new ApplicationException("When UseLocalAuthentication is ture, you must override LocalUserEmailAddress in a derived class"); }
         }

         public virtual Dictionary<string, string> LocalUserDetails
         {
             get { return null; }
         }

         public virtual void UserCreationFailed(string username, string emailAddress, IDictionary<string, string> userData, string message, ErrorResponse errorResponse)
         {
             throw new NotImplementedException();
         }


         public Uri EvolutionBaseUrl
         {
             get { throw new NotImplementedException(); }
         }

         public Uri LocalOAuthClientHttpHandlerUrl
         {
             get { throw new NotImplementedException(); }
         }

         public string OAuthClientId
         {
             get { throw new NotImplementedException(); }
         }

         public string OAuthClientSecret
         {
             get { throw new NotImplementedException(); }
         }

         public NetworkCredential EvolutionCredentials
         {
             get { throw new NotImplementedException(); }
         }

         public string DefaultUserName
         {
             get { throw new NotImplementedException(); }
         }

         public string DefaultUserLanguageKey
         {
             get { throw new NotImplementedException(); }
         }

         public virtual void SetAuthorizationCookie(string value)
         {
             throw new NotImplementedException();
         }

         public virtual string GetAuthorizationCookieValue()
         {
             throw new NotImplementedException();
         }

         public virtual void UserLoggedIn(System.Collections.Specialized.NameValueCollection state)
         {
             throw new NotImplementedException();
         }

         public virtual void UserLoginFailed(System.Collections.Specialized.NameValueCollection state)
         {
             throw new NotImplementedException();
         }

         public virtual void UserLoggedOut(System.Collections.Specialized.NameValueCollection state)
         {
             throw new NotImplementedException();
         }

         public virtual void UserLogOutFailed(System.Collections.Specialized.NameValueCollection state)
         {
             throw new NotImplementedException();
         }

         public virtual bool EnableEvolutionUserSynchronization
         {
             get { throw new NotImplementedException(); }
         }

         public virtual string GetEvolutionUserSynchronizationCookieValue()
         {
             throw new NotImplementedException();
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
     }
     #endregion

#region Helpers
#endregion


}
