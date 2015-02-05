using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Telligent.Rest.SDK
{
    public static class NameValueCollectionExtensions
    {
        public static string MakeQuerystring(this NameValueCollection nvc, bool encodeValues = true)
        {
            return String.Join("&", nvc.AllKeys.Select(a => a + "=" + (encodeValues ? HttpUtility.UrlEncode(nvc[a]) : nvc[a])));
        }
    }
}
