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

        private string ProcessGetEndpoint(string endpoint, RestGetOptions options)
        {
            var processedEndpoint = endpoint;
          
            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);
            return processedEndpoint;
        }
        private string ProcessPostEndpoint(string endpoint, RestPostOptions options)
        {
            var processedEndpoint = endpoint;
         
            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            return processedEndpoint;
        }
        private string ProcessPutEndpoint(string endpoint, RestPutOptions  options)
        {
            var processedEndpoint = endpoint;
          
            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            return processedEndpoint;
        }
        private string ProcessDeleteEndpoint(string endpoint, RestDeleteOptions  options)
        {
            var processedEndpoint = endpoint;
           
            if (options.PathParameters.HasKeys())
                processedEndpoint = ReplaceTokens(endpoint, options.PathParameters);
            if (options.QueryStringParameters.HasKeys())
                processedEndpoint = BuildQueryString(processedEndpoint, options.QueryStringParameters);

            return processedEndpoint;
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
        private void AdjustFileRequest(RestHost host, HttpWebRequest request, RestFileOptions  options)
        {
            if (options != null && options.AdditionalHeaders != null)
                SetAdditionalHeaders(request, options.AdditionalHeaders);
        }
		private void AdjustRequestBase(RestHost host, HttpWebRequest request, bool enableImpersonation)
		{
			host.ApplyAuthenticationToHostRequest(request, enableImpersonation);
		}

        #endregion

        public XElement GetEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            if (options == null)
                options = new RestGetOptions();
            var processedEndpoint = ProcessGetEndpoint(endpoint, options);
          
            return XElement.Parse(ReadResponseStream(_proxy.Get(host, MakeEndpointUrl(host, version, processedEndpoint), (request) => AdjustGetRequest(host, request, enableImpersonation,options))));
        }

        public XElement PutEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (options == null)
                options = new RestPutOptions();
            var processedEndpoint = ProcessPutEndpoint(endpoint, options);
           

            string postData = options.PostParameters.MakeQuerystring(true);

            return XElement.Parse(ReadResponseStream(  _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData,  (request) => AdjustPutRequest(host, request, enableImpersonation,options))));
        }

        public XElement PostEndpointXml(RestHost host, int version, string endpoint,  HttpPostedFileBase file = null, bool enableImpersonation = true, RestPostOptions options = null)
        {

            if (options == null)
                options = new RestPostOptions();

            var processedEndpoint = ProcessPostEndpoint(endpoint, options);
           

            string postData = options.PostParameters.MakeQuerystring(true);
            return XElement.Parse(ReadResponseStream( _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData, (request) => AdjustPostRequest(host, request, true,options))));
        }

        public XElement  DeleteEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {

            if (options == null)
                options = new RestDeleteOptions();
            var processedEndpoint = ProcessDeleteEndpoint(endpoint, options);
           

            return XElement.Parse( ReadResponseStream( _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), null,  (request) => AdjustDeleteRequest(host, request, enableImpersonation,options))));
        }

        public async Task<XElement> GetEndpointXmlAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            if (options == null)
                options = new RestGetOptions();
            var processedEndpoint = ProcessGetEndpoint(endpoint, options);

            return XElement.Parse(await ReadResponseStreamAsync(await _proxy.GetAsync(host, MakeEndpointUrl(host, version, processedEndpoint), (request) => AdjustGetRequest(host, request, enableImpersonation, options))));
        }

        public async Task<XElement> PutEndpointXmlAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (options == null)
                options = new RestPutOptions();
            var processedEndpoint = ProcessPutEndpoint(endpoint, options);


            string postData = options.PostParameters.MakeQuerystring(true);

            return XElement.Parse(await ReadResponseStreamAsync(await _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint), postData, (request) => AdjustPutRequest(host, request, enableImpersonation, options))));
        }

        public async Task<XElement> PostEndpointXmlAsync(RestHost host, int version, string endpoint, HttpPostedFileBase file = null, bool enableImpersonation = true, RestPostOptions options = null)
        {

            if (options == null)
                options = new RestPostOptions();

            var processedEndpoint = ProcessPostEndpoint(endpoint, options);


            string postData = options.PostParameters.MakeQuerystring(true);
            return XElement.Parse(await ReadResponseStreamAsync(await _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint), postData, (request) => AdjustPostRequest(host, request, true, options))));
        }

        public async Task<XElement> DeleteEndpointXmlAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {

            if (options == null)
                options = new RestDeleteOptions();
            var processedEndpoint = ProcessDeleteEndpoint(endpoint, options);


            return XElement.Parse(await ReadResponseStreamAsync(await _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint), null, (request) => AdjustDeleteRequest(host, request, enableImpersonation, options))));
        }

       
       public async Task<Stream> PostEndpointStream(RestHost host, int version, string endpoint, Stream postStream, bool enableImpersonation, Action<WebResponse> responseAction, RestPostOptions options = null)
       {
           //TODO: Review this for refactor, is it necessary or can it be collpased
           return
               await
                   _proxy.PostEndpointStream(host, MakeEndpointUrl(host, version, endpoint), postStream,
                       (request) => AdjustPostRequest(host, request, enableImpersonation,options), responseAction);

       }

       public string GetEndpointString(RestHost host, int version, string endpoint,bool enableImpersonation =true, RestGetOptions options = null)
        {
            if (options == null)
                options = new RestGetOptions();
            var processedEndpoint = ProcessGetEndpoint(endpoint, options);
            Stream stream = _proxy.Get(host, MakeEndpointUrl(host, version, processedEndpoint), (request) => AdjustGetRequest(host, request, enableImpersonation,options));
           return ReadResponseStream(stream);
        }

        public string  PutEndpointString(RestHost host, int version, string endpoint,  bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (options == null)
                options = new RestPutOptions();
            var processedEndpoint = ProcessPutEndpoint(endpoint, options);
           
            string postData = options.PostParameters.MakeQuerystring(true);

            var stream = _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData,  (request) => AdjustPutRequest(host, request, enableImpersonation,options));
            return ReadResponseStream(stream);
        }

        public string PostEndpointString(RestHost host, int version, string endpoint,  bool enableImpersonation = true,  RestPostOptions options = null)
        {
            if (options == null)
                options = new RestPostOptions();
            var processedEndpoint = ProcessPostEndpoint(endpoint,options);
           
            string postData = options.PostParameters.MakeQuerystring(true);

            var stream =  _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData,  (request) => AdjustPostRequest(host, request, true,options));
            return ReadResponseStream(stream);
        }

        public string  DeleteEndpointString(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            if (options == null)
                options = new RestDeleteOptions();
            var processedEndpoint = ProcessDeleteEndpoint(endpoint, options);
           

            var stream = _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), null,  (request) => AdjustDeleteRequest(host, request, enableImpersonation,options));
            return ReadResponseStream(stream);
        }

        public async Task<string> GetEndpointStringAsync(RestHost host, int version, string endpoint,bool enableImpersonation=true, RestGetOptions options = null)
        {
            if (options == null)
                options = new RestGetOptions();
            var processedEndpoint = ProcessGetEndpoint(endpoint, options);
            var stream = await _proxy.GetAsync(host, MakeEndpointUrl(host, version, processedEndpoint), (request) => AdjustGetRequest(host, request, enableImpersonation, options));
            return await ReadResponseStreamAsync(stream);
        }

        public async Task<string> PutEndpointStringAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (options == null)
                options = new RestPutOptions();
            var processedEndpoint = ProcessPutEndpoint(endpoint, options);

            string postData = options.PostParameters.MakeQuerystring(true);

            var stream = await _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint), postData, (request) => AdjustPutRequest(host, request, enableImpersonation, options));
            return await ReadResponseStreamAsync(stream);
        }

        public async Task<string> PostEndpointStringAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true,  RestPostOptions options = null)
        {
            if (options == null)
                options = new RestPostOptions();
            var processedEndpoint = ProcessPostEndpoint(endpoint, options);

            string postData = options.PostParameters.MakeQuerystring(true);

            var stream = await _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint), postData, (request) => AdjustPostRequest(host, request, true, options));
            return await ReadResponseStreamAsync(stream);
        }

        public async Task<string> DeleteEndpointStringAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            if (options == null)
                options = new RestDeleteOptions();
            var processedEndpoint = ProcessDeleteEndpoint(endpoint, options);


            var stream = await  _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint), null, (request) => AdjustDeleteRequest(host, request, enableImpersonation, options));
            return await ReadResponseStreamAsync(stream);
        }



        public async Task<string> BatchEndpointStringAsync(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests,options);
           var str = await _proxy.PostAsync(host, MakeEndpointUrl(host,version,"batch.json"), postData, (request) => AdjustBatchRequest(host, request, enableImpersonation, options));
            return await ReadResponseStreamAsync(str);
        }
        public async Task<XElement> BatchEndpointXmlAsync(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests,options);
            return XElement.Parse(await ReadResponseStreamAsync(await _proxy.PostAsync(host, MakeEndpointUrl(host, version, "batch.json"), postData,  (request) => AdjustBatchRequest(host, request, enableImpersonation, options))));
        }
        public string BatchEndpointString(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests, options);
            return ReadResponseStream( _proxy.Post(host, MakeEndpointUrl(host, version, "batch.json"), postData, (request) => AdjustBatchRequest(host, request, enableImpersonation, options)));
        }
        public XElement BatchEndpointXml(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests, options);
            return XElement.Parse(ReadResponseStream(_proxy.Post(host, MakeEndpointUrl(host, version, "batch.json"), postData, (request) => AdjustBatchRequest(host, request, enableImpersonation, options))));
        }
        private string CreatePostBatchData(IList<BatchRequest> requests,BatchRequestOptions options)
        {
            if (options == null)
                options = new BatchRequestOptions();

             if(requests == null || !requests.Any())
                throw new ArgumentException("Request must contain at least 1 request","requests");

            foreach (var req in requests)
            {
                req.EndpointUrl = ReplaceTokens(req.EndpointUrl, req.PathParameters);
            }
            var postDataArr = requests.Select(r => r.ToString()).ToArray();
            var postData = string.Join("&", postDataArr);
            return postData + "&Sequential=" + options.RunSequentially.ToString().ToLowerInvariant();
        }
        private async Task<string> ReadResponseStreamAsync(Stream str)
        {
            if (str == null)
                return null;
           // str.Position = 0;
            using (var reader = new StreamReader(str))
            {
                return await reader.ReadToEndAsync();
            }
        }
        private string ReadResponseStream(Stream str)
        {
            if (str == null)
                return null;
            //str.Position = 0;
            using (var reader = new StreamReader(str))
            {
                return  reader.ReadToEnd();
            }
        }


        public Task<Stream> GetEndpointStreamAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            if (options == null)
                options = new RestGetOptions();
            var processedEndpoint = ProcessGetEndpoint(endpoint, options);
            return _proxy.GetAsync(host, MakeEndpointUrl(host, version, processedEndpoint),
                (req) => AdjustGetRequest(host, req, enableImpersonation, options));
        }

        public Task<Stream> PutEndpointStreamAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (options == null)
                options = new RestPutOptions();
            var processedEndpoint = ProcessPutEndpoint(endpoint, options);

            string postData = options.PostParameters.MakeQuerystring(true);
            return _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint),postData,
                (req) => AdjustPutRequest(host, req, enableImpersonation, options));
        }

        public Task<Stream> PostEndpointStreamAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            if (options == null)
                options = new RestPostOptions();
            var processedEndpoint = ProcessPostEndpoint(endpoint, options);

            string postData = options.PostParameters.MakeQuerystring(true);
            return _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint), postData,
                (req) => AdjustPostRequest(host, req, enableImpersonation, options));
        }

        public Task<Stream> DeleteEndpointStreamAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            if (options == null)
                options = new RestDeleteOptions();
            var processedEndpoint = ProcessDeleteEndpoint(endpoint, options);

      
            return _proxy.PostAsync(host, MakeEndpointUrl(host, version, processedEndpoint),null,
                (req) => AdjustDeleteRequest(host, req, enableImpersonation, options));
        }

        public Stream GetEndpointStream(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            if (options == null)
                options = new RestGetOptions();
            var processedEndpoint = ProcessGetEndpoint(endpoint, options);
            return _proxy.Get(host, MakeEndpointUrl(host, version, processedEndpoint),
                (req) => AdjustGetRequest(host, req, enableImpersonation, options));
        }

        public Stream PutEndpointStream(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            if (options == null)
                options = new RestPutOptions();
            var processedEndpoint = ProcessPutEndpoint(endpoint, options);

            string postData = options.PostParameters.MakeQuerystring(true);
            return _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData,
                (req) => AdjustPutRequest(host, req, enableImpersonation, options));
        }

        public Stream PostEndpointStream(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            if (options == null)
                options = new RestPostOptions();
            var processedEndpoint = ProcessPostEndpoint(endpoint, options);

            string postData = options.PostParameters.MakeQuerystring(true);
            return _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), postData,
                (req) => AdjustPostRequest(host, req, enableImpersonation, options));
        }

        public Stream DeleteEndpointStream(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            if (options == null)
                options = new RestDeleteOptions();
            var processedEndpoint = ProcessDeleteEndpoint(endpoint, options);


            return _proxy.Post(host, MakeEndpointUrl(host, version, processedEndpoint), null,
                (req) => AdjustDeleteRequest(host, req, enableImpersonation, options));
        }


        public Task<Stream> BatchEndpointStreamAsync(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests, options);
            return _proxy.PostAsync(host, MakeEndpointUrl(host, version, "batch.json"), postData, (request) => AdjustBatchRequest(host, request, enableImpersonation, options));
        }

        public Stream BatchEndpointStream(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var postData = CreatePostBatchData(requests, options);
            return _proxy.Post(host, MakeEndpointUrl(host, version, "batch.json"), postData, (request) => AdjustBatchRequest(host, request, enableImpersonation, options));
        }

        public UploadedFileInfo TransmitFile(RestHost host, UploadedFile file,RestFileOptions options = null)
        {
            if (options == null)
                options = new RestFileOptions();

            string url = GetUploadUrl(host.EvolutionRootUrl, file.UploadContext);
            return _proxy.TransmitFile(host, url,file,options.UploadProgress,
                (request) => AdjustFileRequest(host, request, options));


        }
        public Task<UploadedFileInfo> TransmitFileAsync(RestHost host, UploadedFile file, RestFileOptions options = null)
        {
            if (options == null)
                options = new RestFileOptions();

            string url = GetUploadUrl(host.EvolutionRootUrl, file.UploadContext);
            return _proxy.TransmitFileAsync(host, url, file, options.UploadProgress,
                (request) => AdjustFileRequest(host, request, options));


        }
        private string GetUploadUrl(string rootUrl,Guid uploadContext)
        {
            return rootUrl + (rootUrl.EndsWith("/") ? "" : "/") + "multipleupload?UploadContext=" + uploadContext.ToString("N");
        }
 
    }
}
