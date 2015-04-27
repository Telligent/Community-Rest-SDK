using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using System.Xml;

using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK;
using Telligent.Rest.SDK.Implementation;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class UrlProxy : IHttpHandler
    {

        readonly IEncode Encode = ServiceLocator.Get<IEncode>();
		readonly IProxyService  _proxy = ServiceLocator.Get<IProxyService>();
        private readonly IUrlManipulationService _urls = ServiceLocator.Get<IUrlManipulationService>();

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string url = context.Request.RawUrl;
            var data = _urls.ParseCallbackUrl(url);

            if (data == null || string.IsNullOrEmpty(data.HostName) || string.IsNullOrEmpty(data.Url))
            {
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Error parsing request";
            }
            else
            {

                Host host = Host.Get(data.HostName);
                if (host == null)
                {
                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "The host could not be determined";
                }

                _proxy.ProxyOrRedirect(host, data.Url, new HttpContextWrapper(context));
            }

        }
       
    }
}
