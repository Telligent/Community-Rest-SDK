using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Telligent.Rest.SDK.Configuration
{
    public class TelligentConfigurationSection : ConfigurationSection
    {
        public static TelligentConfigurationSection Current
        {
            get
            {
                return (TelligentConfigurationSection)System.Configuration.ConfigurationManager.GetSection("telligentCommunityGroup/telligentCommunitySDK");
            } 
        }
        [ConfigurationProperty("oauthCallbackUrl", IsRequired = true)]
        public string OauthCallbackUrl
        {
            get { return this["oauthCallbackUrl"].ToString(); }
            set { this["oauthCallbackUrl"] = value; }
        }
        [ConfigurationProperty("communityRootUrl", IsRequired = true)]
        public string CommunityRootUrl
        {
            get { return this["communityRootUrl"].ToString(); }
            set { this["communityRootUrl"] = value; }
        }
        [ConfigurationProperty("networkUserName", DefaultValue = "", IsRequired = false)]
        public string NetworkUserName
        {
            get { return this["networkUserName"].ToString(); }
            set { this["networkUserName"] = value; }
        }
        [ConfigurationProperty("networkPassword", DefaultValue = "", IsRequired = false)]
        public string NetworkPassword
        {
            get { return this["networkPassword"].ToString(); }
            set { this["networkPassword"] = value; }
        }
        [ConfigurationProperty("networkDomain", DefaultValue = "", IsRequired = false)]
        public string NetworkDomain
        {
            get { return this["networkDomain"].ToString(); }
            set { this["networkDomain"] = value; }
        }
        [ConfigurationProperty("synchronizationCookieName", DefaultValue = "EvolutionSync", IsRequired = false)]
        public string SynchronizationCookieName
        {
            get { return this["synchronizationCookieName"].ToString(); }
            set { this["synchronizationCookieName"] = value; }
        }
        [ConfigurationProperty("enableSSO", DefaultValue = false, IsRequired = false)]
        public bool EnableSSO
        {
            get { return (bool)this["enableSSO"]; }
            set { this["enableSSO"] = value; }
        }
        [ConfigurationProperty("useLocalAuthentication", DefaultValue = false, IsRequired = false)]
        public bool UseLocalAuthentication
        {
            get { return (bool)this["useLocalAuthentication"]; }
            set { this["useLocalAuthentication"] = value; }
        }
        [ConfigurationProperty("membershipAdministrationUsername", DefaultValue = "admin", IsRequired = false)]
        public string MembershipAdministrationUsername
        {
            get { return this["membershipAdministrationUsername"].ToString(); }
            set { this["membershipAdministrationUsername"] = value; }
        }
        [ConfigurationProperty("defaultLanguage", DefaultValue = "en-US", IsRequired = false)]
        public string DefaultLanguageKey
        {
            get { return this["defaultLanguage"].ToString(); }
            set { this["defaultLanguage"] = value; }
        }
        [ConfigurationProperty("anonymousUsername", DefaultValue = "Anonymous", IsRequired = false)]
        public string AnonymousUsername
        {
            get { return this["anonymousUsername"].ToString(); }
            set { this["anonymousUsername"] = value; }
        }
        [ConfigurationProperty("oauthClientId", IsRequired = true)]
        public string OauthClientId
        {
            get { return this["oauthClientId"].ToString(); }
            set { this["oauthClientId"] = value; }
        }
        [ConfigurationProperty("oauthSecret", IsRequired = true)]
        public string OauthSecret
        {
            get { return this["oauthSecret"].ToString(); }
            set { this["oauthSecret"] = value; }
        }
        [ConfigurationProperty("", IsRequired = false, IsKey = false, IsDefaultCollection = true)]
        public HostCollection Hosts
        {
            get
            {
                var col = base[""] as HostCollection;
                if (col != null && col.Parent == null)
                    col.Parent = this;
                return col;
            }
          
        }  
    }

    public class HostCollection : ConfigurationElementCollection
    {
        internal TelligentConfigurationSection Parent { get; set; }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public HostElement this[string name]
        {
            get { return BaseGet(name) as HostElement; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HostElement(){Parent = this};
        }

        protected override string ElementName
        {
            get { return "add"; }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((HostElement) element).Name;
        }
    }
    
    public class HostElement : ConfigurationElement
    {
        public HostCollection Parent { get; set; }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }
        [ConfigurationProperty("id", IsRequired = true)]
        public Guid Id
        {
            get { return (Guid)this["id"]; }
            set { this["id"] = value; }
        }
        [ConfigurationProperty("oauthCallbackUrl", IsRequired = false)]
        public string OauthCallbackUrl
        {
            get
            {
                var data = this["oauthCallbackUrl"];
                if (data == null)
                    return Parent.Parent.OauthCallbackUrl;

                return data.ToString();
            }
            set { this["oauthCallbackUrl"] = value; }
        }
        [ConfigurationProperty("communityRootUrl", IsRequired = false)]
        public string CommunityRootUrl
        {
            get
            {
                var data = this["communityRootUrl"];
                if (data == null)
                    return Parent.Parent.CommunityRootUrl;

                return data.ToString();
            }
            set { this["communityRootUrl"] = value; }
        }
        [ConfigurationProperty("networkUserName",  IsRequired = false)]
        public string NetworkUserName
        {
            get
            {
                var data = this["networkUserName"];
                if (data == null)
                    return Parent.Parent.NetworkUserName;

                return data.ToString();
            }
            set { this["networkUserName"] = value; }
        }
        [ConfigurationProperty("networkPassword", IsRequired = false)]
        public string NetworkPassword
        {
            get
            {
                var data = this["networkPassword"];
                if (data == null)
                    return Parent.Parent.NetworkPassword;

                return data.ToString();
            }
            set { this["networkPassword"] = value; }
        }
        [ConfigurationProperty("networkDomain",  IsRequired = false)]
        public string NetworkDomain
        {
            get
            {
                var data = this["networkDomain"];
                if (data == null)
                    return Parent.Parent.NetworkDomain;

                return data.ToString();
            }
            set { this["networkDomain"] = value; }
        }
        [ConfigurationProperty("synchronizationCookieName", IsRequired = false)]
        public string SynchronizationCookieName
        {
            get
            {
                var data = this["synchronizationCookieName"];
                if (data == null)
                    return Parent.Parent.NetworkDomain;

                return data.ToString();
            }
            set { this["synchronizationCookieName"] = value; }
        }
        [ConfigurationProperty("enableSSO",  IsRequired = false)]
        public bool EnableSSO
        {
            get
            {
                var data = this["enableSSO"];
                if (data == null)
                    return Parent.Parent.EnableSSO;

                return (bool)data;
            }
            set { this["enableSSO"] = value; }
        }
        [ConfigurationProperty("useLocalAuthentication", IsRequired = false)]
        public bool UseLocalAuthentication
        {
            get
            {
                var data = this["useLocalAuthentication"];
                if (data == null)
                    return Parent.Parent.EnableSSO;

                return (bool)data;
            }
            set { this["useLocalAuthentication"] = value; }
        }
        [ConfigurationProperty("membershipAdministrationUsername",  IsRequired = false)]
        public string MembershipAdministrationUsername
        {
            get
            {
                var data = this["membershipAdministrationUsername"];
                if (data == null)
                    return Parent.Parent.MembershipAdministrationUsername;

                return data.ToString();
            }
            set { this["membershipAdministrationUsername"] = value; }
        }
        [ConfigurationProperty("defaultLanguage", IsRequired = false)]
        public string DefaultLanguageKey
        {
            get
            {
                var data = this["defaultLanguage"];
                if (data == null)
                    return Parent.Parent.DefaultLanguageKey;

                return data.ToString();
            }
            set { this["defaultLanguage"] = value; }
        }
        [ConfigurationProperty("anonymousUsername",  IsRequired = false)]
        public string AnonymousUsername
        {
            get
            {
                var data = this["anonymousUsername"];
                if (data == null)
                    return Parent.Parent.AnonymousUsername;

                return data.ToString();
            }
            set { this["anonymousUsername"] = value; }
        }
        [ConfigurationProperty("oauthClientId", IsRequired = false)]
        public string OauthClientId
        {
            get
            {
                var data = this["oauthClientId"];
                if (data == null)
                    return Parent.Parent.OauthClientId;

                return data.ToString();
            }
            set { this["oauthClientId"] = value; }
        }
        [ConfigurationProperty("oauthSecret", IsRequired = false)]
        public string OauthSecret
        {
            get
            {
                var data = this["oauthSecret"];
                if (data == null)
                    return Parent.Parent.OauthSecret;

                return data.ToString();
            }
            set { this["oauthSecret"] = value; }
        }

        
    }
}
