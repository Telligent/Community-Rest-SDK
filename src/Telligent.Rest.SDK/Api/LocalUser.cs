using System.Collections.Generic;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class LocalUser
    {
        public LocalUser(string username,string email)
        {
            Username = username;
            EmailAddress = email;
        }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }

    }
}