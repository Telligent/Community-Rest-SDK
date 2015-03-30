using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    /// <summary>
    /// Registered via config this tells the SDK about a user your system manages and authenticated
    /// </summary>
    public interface ILocalUserResolver
    {
        /// <summary>
        /// Allows a 3rd party system to identify a logged in user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        LocalUser GetLocalUserDetails(HttpContextBase context,Host host);
    }
}
