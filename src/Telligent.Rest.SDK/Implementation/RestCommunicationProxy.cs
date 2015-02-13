using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Rest.SDK.Model;

namespace Telligent.Rest.SDK.Implementation
{
    public class RestCommunicationProxy : IRestCommunicationProxy
    {
        private readonly static Regex isXmlRegex = new Regex("<.+>.*</.+>\r\n",
                                                       RegexOptions.IgnoreCase |
                                                       RegexOptions.Multiline |
                                                       RegexOptions.Singleline |
                                                       RegexOptions.CultureInvariant |
                                                       RegexOptions.IgnorePatternWhitespace |
                                                       RegexOptions.Compiled);

        private HttpWebRequest BuildStandardPostRequest(RestHost host, string url, string data,Stream stream, Action<HttpWebRequest> adjustRequest,out byte[] bytes)
        {
            HttpWebRequest request = null;
            request = CreateRequest(host, url, adjustRequest, host.PostTimeout);
            request.Method = "POST";
            bytes = null;
            if (data == null)
                request.ContentLength = 0;
            else
            {
                bytes = Encoding.UTF8.GetBytes(data);
                request.ContentType = isXmlRegex.IsMatch(data) ? "text/xml; charset=utf-8" : "application/x-www-form-urlencoded; charset=utf-8";
                request.ContentLength = bytes.Length;
            }

            return request;

        }

        private HttpWebRequest BuildGetRequest(RestHost host, string url, Action<HttpWebRequest> adjustRequest)
        {
            HttpWebRequest request = CreateRequest(host, url, adjustRequest, host.GetTimeout);
            request.Method = "GET";
            return request;
        }
        public async Task<Stream> PostAsync(RestHost host, string url, string data, Action<HttpWebRequest> adjustRequest)
        {
            bool retry = true;
            HttpWebRequest request = null;
            ExceptionDispatchInfo capturedException = null;
            while (retry)
            {
                retry = false;

                try
                {
                    byte[] bytes = null;

                    request = BuildStandardPostRequest(host, url, data,null, adjustRequest, out bytes);
                    using (var requestStream = await request.GetRequestStreamAsync())
                    {
                        await requestStream.WriteAsync(bytes, 0, bytes.Length);
                        requestStream.Close();
                    }

                    var response = (HttpWebResponse)request.GetResponse();
                    return response.GetResponseStream();
                }
                catch (WebException ex)
                {
                    capturedException = ExceptionDispatchInfo.Capture(ex);
                }

                if (capturedException != null)
                {
                    var webException = capturedException.SourceException as WebException;
                    if (webException != null)
                    {
                        var errorResponse = webException.Response as HttpWebResponse;
                        if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
                            retry = true;
                        else if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
                            return errorResponse.GetResponseStream();
                        else
                            capturedException.Throw();
                    }
                    else
                    {
                        capturedException.Throw();
                    }

                }
            }

            return null;
        }
        public Stream Post(RestHost host, string url, string data, Action<HttpWebRequest> adjustRequest)
        {
            bool retry = true;
            HttpWebRequest request = null;
            while (retry)
            {
                retry = false;

                try
                {
                    byte[] bytes = null;

                     request = BuildStandardPostRequest(host, url, data,null, adjustRequest, out bytes);
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                        requestStream.Close();
                    }
                    var response = (HttpWebResponse) request.GetResponse();
                        return response.GetResponseStream();
                    
                }
                catch (WebException   ex)
                {
                    var errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
                        retry = true;
                    else if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
                        return errorResponse.GetResponseStream();
                    else
                        throw ex;
                }

            }

            return null;
        }
        public async Task<Stream> PostEndpointStream(RestHost host, string url, Stream postStream, Action<HttpWebRequest> adjustRequest, Action<WebResponse> responseAction)
        {
            bool retry = true;
            HttpWebRequest request = null;
            ExceptionDispatchInfo capturedException = null;
            while (retry)
            {
                retry = false;
                try
                {
                    request = CreateRequest(host, url, adjustRequest, host.PostTimeout);
                    request.Method = "POST";
                    request.ContentType = "application/octet-stream";

                    if (postStream == null)
                    {
                        request.ContentLength = 0;
                    }
                    else
                    {
                        request.ContentLength = postStream.Length;
                        using (var requestStream = await request.GetRequestStreamAsync())
                        {
                            byte[] buffer = new byte[64 * 1024];
                            int read;
                            while ((read = await postStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await requestStream.WriteAsync(buffer, 0, read);
                            }
                            requestStream.Close();
                        }
                    }

                    var response = await request.GetResponseAsync();
                    if (responseAction != null)
                        responseAction(response);

                    return response.GetResponseStream();
                }
                catch (WebException ex)
                {
                    capturedException = ExceptionDispatchInfo.Capture(ex);
                }

                if (capturedException != null)
                {
                    var webException = capturedException.SourceException as WebException;
                    if (webException != null)
                    {
                        var errorResponse = webException.Response as HttpWebResponse;
                        if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden &&
                            host.RetryFailedRemoteRequest(request))
                            retry = true;
                        else
                            capturedException.Throw();
                    }
                    else
                    {
                        capturedException.Throw();
                    }

                }
            }

            return null;
        }
       
        public async Task<Stream> GetAsync(RestHost host, string url, Action<HttpWebRequest> adjustRequest)
        {
            bool retry = true;
            HttpWebRequest request = null;
            ExceptionDispatchInfo capturedException = null;
            while (retry)
            {
                retry = false;

                try
                {
                    request = BuildGetRequest(host, url, adjustRequest);

                    var response = await request.GetResponseAsync() ;
                         return ((HttpWebResponse) response).GetResponseStream();
                }
                catch (WebException ex)
                {
                    capturedException = ExceptionDispatchInfo.Capture(ex);
                }
            }

            if (capturedException != null)
            {
                var webException = capturedException.SourceException as WebException;
                if (webException != null)
                {
                    var errorResponse = webException.Response as HttpWebResponse;
                    if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
                        retry = true;
                    else if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
                        return  errorResponse.GetResponseStream();
                    else
                        capturedException.Throw();
                }
                else
                {
                    capturedException.Throw();
                }

            }
            return null;
        }
        public Stream Get(RestHost host, string url, Action<HttpWebRequest> adjustRequest)
        {
            bool retry = true;
            HttpWebRequest request = null;
            ExceptionDispatchInfo capturedException = null;
            while (retry)
            {
                retry = false;

                try
                {
                    request = BuildGetRequest(host, url, adjustRequest);
                    var response =  (HttpWebResponse)request.GetResponse();
                    Stream stream = null;

                       var str = response.GetResponseStream();
                      return str;
                }
                catch (WebException ex)
                {
                    var errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
                        retry = true;
                    else if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
                        return errorResponse.GetResponseStream();
                    else
                        throw ex;
                }
            }

            return null;
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

      
        private string GetMultipartFormdata(string boundary, string name, string value)
        {
            return String.Format("--" + boundary + "\r\n" + "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n", name, value);
        }

        private string GetFileMultipartFormdata(string boundary, string fileName, string contentType)
        {
            return String.Format("--" + boundary + "\r\n" + "Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", fileName, contentType);
        }
    }
}
