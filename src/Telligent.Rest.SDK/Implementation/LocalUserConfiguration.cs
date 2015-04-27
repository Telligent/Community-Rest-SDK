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
           
        }
        public string MembershipAdministrationUserName { get; set; }
        public bool Enabled { get; set; }
       
    }
}