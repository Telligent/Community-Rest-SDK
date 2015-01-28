using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.Rest.SDK.Api
{
     public class DefaultHost:RestHost
     {
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
     }
}
