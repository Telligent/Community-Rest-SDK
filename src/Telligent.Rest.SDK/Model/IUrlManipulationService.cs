using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Telligent.Rest.SDK
{
    public interface IUrlManipulationService
    {
        string ModifyUrl(string url, string queryStringModification, string targetModification);
        System.Collections.Specialized.NameValueCollection ParseQueryString(string queryString);
        string MakeQueryString(System.Collections.Specialized.NameValueCollection queryStringValues);
        string ConvertQueryStringToHash(string url);

        CallbackUrlData ParseCallbackUrl(string rawUrl);

        string GetCallbackUrl(HttpContextBase context,string callbackUrlBase,string hostName, string path, string queryString);
    }

    public  class CallbackUrlData
    {
       public string HostName { get; set; }
        public string Url { get; set; }
    }
}
