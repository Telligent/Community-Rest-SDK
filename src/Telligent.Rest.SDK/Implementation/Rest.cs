using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using Telligent.Rest.SDK;
using Telligent.Rest.SDK.Model;
using System.Threading.Tasks;


namespace Telligent.Evolution.RestSDK.Implementations
{
    public class Rest : IRest
    {
        private static readonly Regex _tokenPattern = new Regex(@"(?<token>{(?<tokenValue>[a-zA-Z0-9]*)})", RegexOptions.Singleline | RegexOptions.Compiled);
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

        public string BuildQueryString(string url, NameValueCollection nvc)
        {
          
            var qs = nvc.MakeQuerystring();
            var delimiter = "?";

            if (url.IndexOf("?") >= 0)
                delimiter = "&";

             return string.Concat(url, delimiter, qs);
        
        }
        public string ReplaceTokens(string url,NameValueCollection parameters)
        {
            
            var newUrl =_tokenPattern.Replace(url, (m) =>
            {
                var tokenValue = m.Groups["tokenValue"].Value;
                if (parameters[tokenValue] != null)
                {
                    var val = parameters[tokenValue];
                    return val;
                }
                
                return m.Value;
            });

            return newUrl;

        }
    
        #region Helpers

        public string MakeEndpointUrl(RestHost host, int version, string endpoint)
        {
            string restUrl = host.EvolutionRootUrl;
            if (!restUrl.EndsWith("/"))
                restUrl += "/";

          
			return string.Concat(restUrl, "api.ashx/v",version, "/",endpoint);
        }

        private void SetAdditionalHeaders(HttpWebRequest req, NameValueCollection nvc)
        {
            foreach (string key in nvc)
            {
                req.Headers[key] = nvc[key];
            }
        }
		private void AdjustGetRequest(RestHost host, HttpWebRequest request, bool enableImpersonation,RestGetOptions options)
        {
			AdjustRequestBase(host, request, enableImpersonation);
            if(options != null && options.AdditionalHeaders != null)
                SetAdditionalHeaders(request,options.AdditionalHeaders);
        }

		private void AdjustPutRequest(RestHost host, HttpWebRequest request, bool enableImpersonation,RestPutOptions options)
        {
			AdjustRequestBase(host, request, enableImpersonation);
            
            if (options != null && options.AdditionalHeaders != null)
                SetAdditionalHeaders(request, options.AdditionalHeaders);

            request.Headers["Rest-Method"] = "PUT";
        }

		private void AdjustPostRequest(RestHost host, HttpWebRequest request, bool enableImpersonation,RestPostOptions options)
        {
			AdjustRequestBase(host, request, enableImpersonation);
            if (options != null && options.AdditionalHeaders != null)
                SetAdditionalHeaders(request, options.AdditionalHeaders);
        }
        private void AdjustBatchRequest(RestHost host, HttpWebRequest request, bool enableImpersonation, BatchRequestOptions options)
        {
            AdjustRequestBase(host, request, enableImpersonation);
            if (options != null && options.AdditionalHeaders != null)
                SetAdditionalHeaders(request, options.AdditionalHeaders);
        }
		private void AdjustDeleteRequest(RestHost host, HttpWebRequest request, bool enableImpersonation,RestDeleteOptions options)
        {
			AdjustRequestBase(host, request, enableImpersonation);
			
            if (options != null && options.AdditionalHeaders != null)
                SetAdditionalHeaders(request, options.AdditionalHeaders);

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
            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestGetOptions();

            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            return XElement.Parse(await _proxy.Get(host, MakeEndpointUrl(host, version, processedEndpoint), (request) => AdjustGetRequest(host, request, enableImpersonation,options)));
        }

        public async Task<XElement> PutEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestPutOptions();


            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            string postData = options.PostParameters.MakeQuerystring(true);

            return XElement.Parse(await _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData, null, (request) => AdjustPutRequest(host, request, enableImpersonation,options)));
        }

        public async Task<XElement> PostEndpointXml(RestHost host, int version, string endpoint,  HttpPostedFileBase file = null, bool enableImpersonation = true, RestPostOptions options = null)
        {

            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestPostOptions();


            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            string postData = options.PostParameters.MakeQuerystring(true);
            return XElement.Parse(await _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData, file, (request) => AdjustPostRequest(host, request, true,options)));
        }

        public async Task<XElement>  DeleteEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {

            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestDeleteOptions();


            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            return XElement.Parse(await _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), null, null, (request) => AdjustDeleteRequest(host, request, enableImpersonation,options)));
        }

       
       public async Task<Stream> PostEndpointStream(RestHost host, int version, string endpoint, Stream postStream, bool enableImpersonation, Action<WebResponse> responseAction, RestPostOptions options = null)
       {
           //TODO: Review this for refactor, is it necessary or can it be collpased
           return
               await
                   _proxy.PostEndpointStream(host, MakeEndpointUrl(host, version, endpoint), postStream,
                       (request) => AdjustPostRequest(host, request, enableImpersonation,options), responseAction);

       }

       public Task<string> GetEndpointJson(RestHost host, int version, string endpoint, RestGetOptions options = null)
        {

            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestGetOptions();


            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            return _proxy.Get(host, MakeEndpointUrl(host, version, processedEndpoint), (request) => AdjustGetRequest(host, request, true,options));
        }

        public Task<string>  PutEndpointJson(RestHost host, int version, string endpoint,  bool enableImpersonation = true, RestPutOptions options = null)
        {
            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestPutOptions();


            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            string postData = options.PostParameters.MakeQuerystring(true);

            return _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData, null, (request) => AdjustPutRequest(host, request, enableImpersonation,options));
        }

        public Task<string> PostEndpointJson(RestHost host, int version, string endpoint,  bool enableImpersonation = true, HttpPostedFileBase file = null, RestPostOptions options = null)
        {
            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestPostOptions();


            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            string postData = options.PostParameters.MakeQuerystring(true);

            return  _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData, file, (request) => AdjustPostRequest(host, request, true,options));
        }

        public Task<string>  DeleteEndpointJson(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            var processedEndpoint = endpoint;
            if (options == null)
                options = new RestDeleteOptions();


            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            return _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), null, null, (request) => AdjustDeleteRequest(host, request, enableImpersonation,options));
        }


        public Task<string> BatchEndpointJson(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests,options);
            return _proxy.Post(host, MakeEndpointUrl(host,version,"batch.json"), postData,null, (request) => AdjustBatchRequest(host, request, enableImpersonation, options));
        }
        public async Task<XElement> BatchEndpointXml(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests,options);
            return XElement.Parse(await _proxy.Post(host, MakeEndpointUrl(host, version, "batch.json"), postData, null, (request) => AdjustBatchRequest(host, request, enableImpersonation, options)));
        }
        private string CreatePostBatchData(IList<BatchRequest> requests,BatchRequestOptions options)
        {
            if (options == null)
                options = new BatchRequestOptions();

             if(requests == null || !requests.Any())
                throw new ArgumentException("Request must contain at least 1 request","requests");
            var postDataArr = requests.Select(r => r.ToString()).ToArray();
            var postData = string.Join("&", postDataArr);
            return postData + "&Sequential=" + options.RunSequentially.ToString().ToLowerInvariant();
        }
        
    }
}
