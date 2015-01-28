using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Rest.SDK.Configuration;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
     public class DefaultHost:RestHost
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
            get { throw new NotImplementedException(); }
        }

        public override void ApplyAuthenticationToHostRequest(System.Net.HttpWebRequest request, bool forAccessingUser)
        {
            //Apply an Oauth token here
        }

        public override string EvolutionRootUrl
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

         #region Default Host Settings

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
             _settings.EnableSSO = hostSection.EnableSSO;
             _settings.MembershipAdministrationUserName = hostSection.MembershipAdministrationUsername;
             _settings.OauthCallbackUrl = hostSection.OauthCallbackUrl;
             _settings.OauthClientId = hostSection.OauthClientId;
             _settings.OauthSecret = hostSection.OauthSecret;
             _settings.UseLocalAuthentication = hostSection.UseLocalAuthentication;
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

             var host = RestHost.Get(hostSection.Id);
             if (host == null)
             {
                 host = new DefaultHost(name);
                 RestHost.Register(host);
             }

             return (DefaultHost) host;

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
}
