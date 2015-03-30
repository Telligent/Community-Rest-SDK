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
        /// <summary>
        /// A username for the user of the 3rd party site.  It must be unique in the community and must be the same on both sides.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// A email for the user of the 3rd party site.  It must be unique in the community and must be the same on both sides.
        /// </summary>
        public string EmailAddress { get; set; }
        /// <summary>
        ///Allows additional profile fields to be set on a user when they are created.
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; }

    }
}