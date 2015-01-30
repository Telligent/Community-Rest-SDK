using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Services;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using Telligent.Rest.SDK.Model;
using System.Threading.Tasks;


namespace Telligent.Evolution.RestSDK.Implementations
{
    public class Rest : IRest
    {
        private const string Json = ".json";
        private const string Xml = ".xml";
       
        private IRestCommunicationProxy _proxy;

        public Rest(IRestCommunicationProxy proxy)
        {
            _proxy = proxy;
        }
		
        public string FormatDateTime(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }

     

    
        #region Helpers

        private string MakeEndpointUrl(RestHost host, int version, string endpoint)
        {
            string restUrl = host.EvolutionRootUrl;
            if (!restUrl.EndsWith("/"))
                restUrl += "/";

          
			return string.Concat(restUrl, "api.ashx/v",version, "/",endpoint);
        }

		private void AdjustGetRequest(RestHost host, HttpWebRequest request, bool enableImpersonation)
        {
			AdjustRequestBase(host, request, enableImpersonation);
        }

		private void AdjustPutRequest(RestHost host, HttpWebRequest request, bool enableImpersonation)
        {
			AdjustRequestBase(host, request, enableImpersonation);
            request.Headers["Rest-Method"] = "PUT";
        }

		private void AdjustPostRequest(RestHost host, HttpWebRequest request, bool enableImpersonation)
        {
			AdjustRequestBase(host, request, enableImpersonation);
        }

		private void AdjustDeleteRequest(RestHost host, HttpWebRequest request, bool enableImpersonation)
        {
			AdjustRequestBase(host, request, enableImpersonation);
			request.Headers["Rest-Method"] = "DELETE";
        }

		private void AdjustRequestBase(RestHost host, HttpWebRequest request, bool enableImpersonation)
		{
			host.ApplyAuthenticationToHostRequest(request, enableImpersonation);
		}

		

        private bool ContainsJson(string endpoint)
        {
            return (endpoint.IndexOf(Json, StringComparison.Ordinal) > -1);
        }
        private bool ContainsXml(string endpoint)
        {
            return (endpoint.IndexOf(Rest.Xml, StringComparison.Ordinal) > -1);
        }
        #endregion

        public async Task<XElement> GetEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");

            return XElement.Parse(await _proxy.Get(host, MakeEndpointUrl(host, version, endpoint), (request) => AdjustGetRequest(host, request, enableImpersonation)));
        }

        public async Task<XElement> PutEndpointXml(RestHost host, int version, string endpoint, string postData, bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");

            return XElement.Parse(await _proxy.Post(host, MakeEndpointUrl(host, version, endpoint), postData, null, (request) => AdjustPutRequest(host, request, enableImpersonation)));
        }

        public async Task<XElement> PostEndpointXml(RestHost host, int version, string endpoint, string postData, HttpPostedFileBase file = null, bool enableImpersonation = true, RestPostOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");
            return XElement.Parse(await _proxy.Post(host, MakeEndpointUrl(host, version, endpoint), postData, file, (request) => AdjustPostRequest(host, request, true)));
        }

        public async Task<XElement>  DeleteEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");
            return XElement.Parse(await _proxy.Post(host, MakeEndpointUrl(host, version, endpoint), null, null, (request) => AdjustDeleteRequest(host, request, enableImpersonation)));
        }

       
       public async Task<Stream> PostEndpointStream(RestHost host, int version, string endpoint, Stream postStream, bool enableImpersonation, Action<WebResponse> responseAction, RestPostOptions options = null)
       {
           return
               await
                   _proxy.PostEndpointStream(host, MakeEndpointUrl(host, version, endpoint), postStream,
                       (request) => AdjustPostRequest(host, request, enableImpersonation), responseAction);

       }

       public Task<string> GetEndpointJson(RestHost host, int version, string endpoint, RestGetOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");

            return _proxy.Get(host, MakeEndpointUrl(host, version, endpoint), (request) => AdjustGetRequest(host, request, true));
        }

        public Task<string>  PutEndpointJson(RestHost host, int version, string endpoint, string postData, bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");

            return _proxy.Post(host, MakeEndpointUrl(host, version, endpoint), postData, null, (request) => AdjustPutRequest(host, request, enableImpersonation));
        }

        public Task<string> PostEndpointJson(RestHost host, int version, string endpoint, string postData, bool enableImpersonation = true, HttpPostedFileBase file = null, RestPostOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");

            return  _proxy.Post(host, MakeEndpointUrl(host, version, endpoint), postData, file, (request) => AdjustPostRequest(host, request, true));
        }

        public Task<string>  DeleteEndpointJson(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");

            return _proxy.Post(host, MakeEndpointUrl(host, version, endpoint), null, null, (request) => AdjustDeleteRequest(host, request, enableImpersonation));
        }
    }
}
