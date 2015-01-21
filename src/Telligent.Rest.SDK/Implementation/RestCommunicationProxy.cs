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

namespace Telligent.Rest.SDK.Implementation
{
    public class RestCommunicationProxy
    {
        private readonly static Regex isXmlRegex = new Regex("<.+>.*</.+>\r\n",
                                                       RegexOptions.IgnoreCase |
                                                       RegexOptions.Multiline |
                                                       RegexOptions.Singleline |
                                                       RegexOptions.CultureInvariant |
                                                       RegexOptions.IgnorePatternWhitespace |
                                                       RegexOptions.Compiled);

        public async Task<string> Post(RestHost host, string url, string data, HttpPostedFileBase file, Action<HttpWebRequest> adjustRequest)
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

                            await file.InputStream.ReadAsync(fileData, 0, file.ContentLength);

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
                        using (var requestStream = await request.GetRequestStreamAsync())
                        {
                            await requestStream.WriteAsync(bytes, 0, bytes.Length);
                            requestStream.Close();
                        }
                    }

                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        return await ReadResponseStreamAsync(response);
                    }
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
                            return await ReadResponseStreamAsync(errorResponse);
                        else
                            capturedException.Throw();
                    }
                    else
                    {
                        capturedException.Throw();
                    }

                }
            }

            return string.Empty;
        }
        public async Task<string> Get(RestHost host, string url, Action<HttpWebRequest> adjustRequest)
        {
            bool retry = true;
            HttpWebRequest request = null;
            ExceptionDispatchInfo capturedException = null;
            while (retry)
            {
                retry = false;

                try
                {
                    request = CreateRequest(host, url, adjustRequest, host.GetTimeout);
                    request.Method = "GET";

                    var response = await request.GetResponseAsync();
                    using (response)
                    {
                        return await ReadResponseStreamAsync((HttpWebResponse)response);
                    }
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
                        return await ReadResponseStreamAsync(errorResponse);
                    else
                        capturedException.Throw(); 
                }
                else
                {
                    capturedException.Throw();
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

        private async Task<string> ReadResponseStreamAsync(HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return await reader.ReadToEndAsync();
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
    }
}
