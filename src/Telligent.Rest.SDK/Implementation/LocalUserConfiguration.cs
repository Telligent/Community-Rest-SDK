using System;
using System.Web;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.Evolution.RestSDK.Implementations
{
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
}