using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public interface ILocalUserResolver
    {
        LocalUser GetLocalUserDetails(HttpContextBase context,Host host);
    }
}
