using System;
using System.Web;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class ProxyConfiguration
    {
        public ProxyConfiguration()
        {
            Enabled = false;
            CallbackUrl = "~/proxy.ashx";
        }
        public string CallbackUrl { get; set; }
        public bool Enabled { get; set; }
    }
}