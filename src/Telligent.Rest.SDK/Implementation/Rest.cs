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


namespace Telligent.Evolution.RestSDK.Implementations
{
    public class Rest : IRest
    {
        private const string Json = ".json";
        private const string Xml = ".xml";
        private readonly static Regex isXmlRegex = new Regex("<.+>.*</.+>\r\n",
                                                        RegexOptions.IgnoreCase |
                                                        RegexOptions.Multiline |
                                                        RegexOptions.Singleline |
                                                        RegexOptions.CultureInvariant |
                                                        RegexOptions.IgnorePatternWhitespace |
                                                        RegexOptions.Compiled);

      

        public Rest()
        {
          
        }

		public XElement GetEndpointXml(RestHost host,int version, string endpoint,RestGetOptions options = null)
        {
            return GetEndpointXml(host, version, endpoint, true,options);
        }

        public XElement GetEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation, RestGetOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");

			return XElement.Parse(Get(host, MakeEndpointUrl(host,version, endpoint), (request) => AdjustGetRequest(host, request, true)));
        }

        public XElement PutEndpointXml(RestHost host, int version, string endpoint, string postData,bool enableImpersonation = true,RestPutOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");

			return XElement.Parse(Post(host, MakeEndpointUrl(host,version, endpoint), postData, null, (request) => AdjustPutRequest(host, request,enableImpersonation)));
        }

        public XElement PostEndpointXml(RestHost host, int version, string endpoint, string postData,RestPostOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");

            return PostEndpointXml(host,version, endpoint, postData, true,options);
        }

        public XElement PostEndpointXml(RestHost host, int version, string endpoint, string postData, bool enableImpersonation,RestPostOptions options = null)
        {
            return PostEndpointXml(host,version, endpoint, postData, null, enableImpersonation);
        }

        public Stream PostEndpointStream(RestHost host, int version, string endpoint, Stream postStream,RestPostOptions options = null)
		{
			return PostEndpointStream(host,version, endpoint, postStream, true, null,options);
		}

        public Stream PostEndpointStream(RestHost host, int version, string endpoint, Stream postStream, bool enableImpersonation, Action<WebResponse> responseAction,RestPostOptions options)
		{
			bool retry = true;
			HttpWebRequest request = null;
			while (retry)
			{
				retry = false;
				try
				{
					request = CreateRequest(host, MakeEndpointUrl(host,version, endpoint), (r) => AdjustPostRequest(host, r, true), host.PostTimeout);
					request.Method = "POST";
					request.ContentType = "application/octet-stream";

					if (postStream == null)
					{
						request.ContentLength = 0;
					}
					else
					{
						request.ContentLength = postStream.Length;
						using (var requestStream = request.GetRequestStream())
						{
							byte[] buffer = new byte[64 * 1024];
							int read;
							while((read = postStream.Read(buffer, 0, buffer.Length)) > 0)
							{
								requestStream.Write(buffer, 0, read);
							}
							requestStream.Close();
						}
					}

					var response = request.GetResponse();
					if (responseAction != null)
						responseAction(response);
			
					return response.GetResponseStream();
				}
				catch (WebException ex)
				{
					var errorResponse = ex.Response as HttpWebResponse;
					if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
						retry = true;
					else
						throw;
				}
			}

			return null;
		}

		public XElement PostEndpointXml(RestHost host,int version, string endpoint, string postData, HttpPostedFileBase file, bool enableImpersonation,RestPostOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");
			return XElement.Parse(Post(host, MakeEndpointUrl(host,version, endpoint), postData, file, (request) => AdjustPostRequest(host, request, true)));
        }

        public XElement DeleteEndpointXml(RestHost host, int version, string endpoint,bool enableImpersonation = true,RestDeleteOptions options = null)
        {
            if (!ContainsXml(endpoint))
                throw new ArgumentException("This call is not valid on non XML endpoints", "endpoint");
            return XElement.Parse(Post(host, MakeEndpointUrl(host,version, endpoint), null, null, (request) => AdjustDeleteRequest(host, request, enableImpersonation)));
        }

        public string FormatDateTime(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }

        public string GetEndpointJson(RestHost host, int version, string endpoint,RestGetOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");

            return  Get(host, MakeEndpointUrl(host,version, endpoint), (request) => AdjustGetRequest(host, request, true));
        }

        public string PutEndpointJson(RestHost host, int version, string endpoint, string postData,bool enableimpersonation = true,RestPutOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");

            return Post(host, MakeEndpointUrl(host,version, endpoint), postData, null, (request) => AdjustPutRequest(host, request, enableimpersonation));
        }

        public string PostEndpointJson(RestHost host, int version, string endpoint, string postData,RestPostOptions options = null)
        {
            return PostEndpointJson(host,version, endpoint, postData, null,options);
        }

        public string PostEndpointJson(RestHost host, int version, string endpoint, string postData, HttpPostedFileBase file,RestPostOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");
           
            return  Post(host, MakeEndpointUrl(host,version, endpoint), postData, file, (request) => AdjustPostRequest(host, request, true));
        }

        public string DeleteEndpointJson(RestHost host, int version, string endpoint,bool enableImpersonation = true,RestDeleteOptions options = null)
        {
            if (!ContainsJson(endpoint))
                throw new ArgumentException("This call is not valid on non JSON endpoints", "endpoint");

            return  Post(host, MakeEndpointUrl(host,version, endpoint), null, null, (request) => AdjustDeleteRequest(host, request, enableImpersonation));
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

		public string Get(RestHost host, string url, Action<HttpWebRequest> adjustRequest)
        {
			bool retry = true;
			HttpWebRequest request = null;
			while (retry)
			{
				retry = false;

				try
				{
					request = CreateRequest(host, url, adjustRequest, host.GetTimeout);
					request.Method = "GET";

					using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
					{
						return ReadResponseStream(response);
					}
				}
				catch (WebException ex)
				{
					var errorResponse = ex.Response as HttpWebResponse;
					if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
						retry = true;
					else if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
						return ReadResponseStream(errorResponse);
					else
						throw;
				}
			}

			return string.Empty;
        }

		public string Post(RestHost host, string url, string data, HttpPostedFileBase file, Action<HttpWebRequest> adjustRequest)
        {
			bool retry = true;
			HttpWebRequest request = null;
			while (retry)
			{
				retry = false;

				try
				{
					request = CreateRequest(host, url, adjustRequest, host.PostTimeout);
					request.Method = "POST";

					if (data == null)
						request.ContentLength = 0;
					else
					{
						byte[] bytes;
						if (file != null)
						{
							string boundary = Guid.NewGuid().ToString("N");
							string newData = "";

							string[] pairs = data.Split(new char[] { '&' });
							foreach (string pair in pairs)
							{
								string[] param = pair.Split(new char[] { '=' });
								newData += GetMultipartFormdata(boundary, param[0], param.Length > 1 ? HttpUtility.UrlDecode(param[1]) : String.Empty);
							}

							string fileName = !String.IsNullOrEmpty(file.FileName) ? file.FileName.Remove(0, file.FileName.LastIndexOf("\\") + 1) : "noname";
							string contentType = !String.IsNullOrEmpty(file.ContentType) ? file.ContentType : "text/plain";

							newData += GetFileMultipartFormdata(boundary, fileName, contentType);

							byte[] fileData = new byte[file.ContentLength];
							byte[] newDataBytes = Encoding.UTF8.GetBytes(newData);
							byte[] endDataBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

							file.InputStream.Read(fileData, 0, file.ContentLength);

							bytes = new byte[newDataBytes.Length + fileData.Length + endDataBytes.Length];

							newDataBytes.CopyTo(bytes, 0);
							fileData.CopyTo(bytes, newDataBytes.Length);
							endDataBytes.CopyTo(bytes, newDataBytes.Length + fileData.Length);

							request.ContentType = "multipart/form-data; boundary=" + boundary;
						}
						else
						{
							bytes = Encoding.UTF8.GetBytes(data);
							request.ContentType = isXmlRegex.IsMatch(data) ? "text/xml; charset=utf-8" : "application/x-www-form-urlencoded; charset=utf-8";
						}

						request.ContentLength = bytes.Length;

						using (var requestStream = request.GetRequestStream())
						{
							requestStream.Write(bytes, 0, bytes.Length);
							requestStream.Close();
						}
					}

					using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
					{
						return ReadResponseStream(response);
					}
				}
				catch (WebException ex)
				{
					var errorResponse = ex.Response as HttpWebResponse;
					if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
						retry = true;
					else if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
						return ReadResponseStream(errorResponse);
					else
						throw;
				}
			}

			return string.Empty;
        }

		private HttpWebRequest CreateRequest(RestHost host, string url, Action<HttpWebRequest> adjustRequest, int timeout)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
			adjustRequest(request);

            request.Timeout = timeout;

			var httpContext = host.GetCurrentHttpContext();
			if (httpContext != null && httpContext.Request != null)
			{
				if (!string.IsNullOrEmpty(httpContext.Request.UserAgent))
					request.UserAgent = httpContext.Request.UserAgent;

				if (!string.IsNullOrEmpty(httpContext.Request.Headers["Cookie"]))
					request.Headers["Cookie"] = httpContext.Request.Headers["Cookie"];
			}

            return request;
        }

        private string ReadResponseStream(HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        private string GetMultipartFormdata(string boundary, string name, string value)
        {
            return String.Format("--" + boundary + "\r\n" + "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n", name, value);
        }

        private string GetFileMultipartFormdata(string boundary, string fileName, string contentType)
        {
            return String.Format("--" + boundary + "\r\n" + "Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", fileName, contentType);
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
    }
}
