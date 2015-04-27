using System;
using System.Net;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class HostConfiguration
    {
        public HostConfiguration()
        {
            OAuth = new OAuthConfiguration();
            Proxy = new ProxyConfiguration();
            SSO= new SSOConfiguration();
        }
        public Guid? Id { get; set; }
        public string CommunityServerUrl { get; set; }
        public string Name { get; set; }
        public NetworkCredential NetworkCredentials { get; set; }
        public OAuthConfiguration OAuth { get; set; }
        public ProxyConfiguration Proxy { get; set; }
        public SSOConfiguration SSO { get; set; }
    }
}