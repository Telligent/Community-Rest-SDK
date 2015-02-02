using System.Collections.Generic;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class LocalUser
    {
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public IDictionary<string, string> AdditionalData { get; set; }

    }
}