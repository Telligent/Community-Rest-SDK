using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Json;
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

        private static readonly int MAX_CHUNK_SIZE_BYTES = 15728640;
        private static readonly string MULTI_FORM_FORMAT = "UploadContext={0}&name={1}&chunk={2}&chunks={3}";

        private HttpWebRequest BuildStandardPostRequest(RestHost host, string url, string data, Stream stream, Action<HttpWebRequest> adjustRequest, out byte[] bytes)
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

                    request = BuildStandardPostRequest(host, url, data, null, adjustRequest, out bytes);
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

                    request = BuildStandardPostRequest(host, url, data, null, adjustRequest, out bytes);
                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                        requestStream.Close();
                    }
                    var response = (HttpWebResponse)request.GetResponse();
                    return response.GetResponseStream();

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

                    var response = await request.GetResponseAsync();
                    return ((HttpWebResponse)response).GetResponseStream();
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
                        return errorResponse.GetResponseStream();
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
                    var response = (HttpWebResponse)request.GetResponse();
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

        private byte[] PrepareDataChunk(byte[] chunk,int currentChunk,string boundary,int totalChunks,UploadedFile file)
        {
            StringBuilder sb = new StringBuilder();
            string fmtFileName = !String.IsNullOrEmpty(file.FileName) ? file.FileName.Remove(0, file.FileName.LastIndexOf("\\") + 1) : "noname";


            sb.Append(GetMultipartFormdata(boundary, "name", fmtFileName));
            sb.Append(GetMultipartFormdata(boundary, "chunk", currentChunk.ToString()));
            sb.Append(GetMultipartFormdata(boundary, "chunks", totalChunks.ToString()));
            sb.Append(GetFileMultipartFormdataFile(boundary, file.FileName, file.ContentType));

            var startDataBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var endDataBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            byte[] bytesToSend = new byte[startDataBytes.Length + chunk.Length + endDataBytes.Length];
            startDataBytes.CopyTo(bytesToSend, 0);
            chunk.CopyTo(bytesToSend, startDataBytes.Length);
            endDataBytes.CopyTo(bytesToSend, startDataBytes.Length + chunk.Length);

            return bytesToSend;
        }
        public UploadedFileInfo TransmitFile(RestHost host, string url, UploadedFile file,Action<FileUploadProgress> progressAction,
            Action<HttpWebRequest> adjustRequest)
        {

            int totalChunks = (int)Math.Ceiling((double)file.FileData.Length / MAX_CHUNK_SIZE_BYTES);
            int currentChunk = 1;
            UploadedFileInfo fileResponse = new UploadedFileInfo(file.UploadContext);
            using (var rdr = new BinaryReader(file.FileData))
                
                for(var i =currentChunk;i<= totalChunks;i++)
                {
                    FileUploadProgress progress = new FileUploadProgress() {UploadContext = file.UploadContext };
                    string boundary = Guid.NewGuid().ToString("N");
                    var chunk = rdr.ReadBytes(MAX_CHUNK_SIZE_BYTES);
                    var bytesToSend = PrepareDataChunk(chunk, currentChunk, boundary, totalChunks, file);

                    var request = CreateRequest(host, url, adjustRequest, host.PostTimeout);
                    request.Method = "POST";
         
                        try
                        {

                            request.ContentType = "multipart/form-data; boundary=" + boundary;
                            request.ContentLength = bytesToSend.Length;
                            using (var requestStream = request.GetRequestStream())
                            {
                                requestStream.Write(bytesToSend, 0, bytesToSend.Length);
                               
                                requestStream.Close();
                            }

                            using (var response = (HttpWebResponse)request.GetResponse())
                            {
                                var stream = response.GetResponseStream();
                                if (stream != null)
                                {
                                   using (var reader = new StreamReader(stream))
                                    {
                                        var responseData = reader.ReadToEnd();
                                        if (!string.IsNullOrEmpty(responseData))
                                        {
                                            var uploadResponse = JsonConvert.Deserialize(responseData);
                                            fileResponse.DownloadUrl = uploadResponse.result.downloadUrl;
                                        }
                                    }
                                }
                            }


                            if (progressAction != null)
                                progressAction(progress);

                            currentChunk ++;
                            
                        }
                        catch (Exception  ex)
                        {

                            fileResponse.IsError = true;
                            fileResponse.Message =ex.Message;
                            var webException =  ex as WebException;
                            if (webException != null)
                            {
                                var errorResponse = webException.Response as HttpWebResponse;
                                if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
                                {
                                    var resp =  ReadUploadError(errorResponse.GetResponseStream());
                                    if (resp != null && resp.error != null)
                                    {
                                        fileResponse.Message = resp.error.message;
                                    }

                                }
                            }
                        }
                        
                }
            return fileResponse;
        }
        public async Task<UploadedFileInfo> TransmitFileAsync(RestHost host, string url, UploadedFile file, Action<FileUploadProgress> progressAction,
             Action<HttpWebRequest> adjustRequest)
        {
            ExceptionDispatchInfo capturedException = null;
            int totalChunks = (int)Math.Ceiling((double)file.FileData.Length / MAX_CHUNK_SIZE_BYTES);
            int currentChunk = 1;
            UploadedFileInfo fileResponse = new UploadedFileInfo(file.UploadContext);
            using (var rdr = new BinaryReader(file.FileData))

                for (var i = currentChunk; i <= totalChunks; i++)
                {
                    FileUploadProgress progress = new FileUploadProgress() { UploadContext = file.UploadContext };
                    string boundary = Guid.NewGuid().ToString("N");
                    var chunk = rdr.ReadBytes(MAX_CHUNK_SIZE_BYTES);
                    var bytesToSend = PrepareDataChunk(chunk, currentChunk, boundary, totalChunks, file);

                    var request = CreateRequest(host, url, adjustRequest, host.PostTimeout);
                    request.Method = "POST";

                    try
                    {

                        request.ContentType = "multipart/form-data; boundary=" + boundary;
                        request.ContentLength = bytesToSend.Length;
                        using (var requestStream =await  request.GetRequestStreamAsync())
                        {
                            await requestStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

                            requestStream.Close();
                        }

                        using (HttpWebResponse response =  (HttpWebResponse)await request.GetResponseAsync())
                        {
                            var str = response.GetResponseStream();
                        }


                        if (progressAction != null)
                            progressAction(progress);

                        currentChunk++;

                    }
                    catch (Exception ex)
                    {

                        capturedException = ExceptionDispatchInfo.Capture(ex);
                    }

                    if (capturedException != null )
                    {
                        fileResponse.IsError = true;
                        fileResponse.Message = capturedException.SourceException.Message;
                        var webException = capturedException.SourceException as WebException;
                        if (webException != null  )
                        {
                            var errorResponse = webException.Response as HttpWebResponse;
                            if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                var resp = await ReadUploadErrorAsync(errorResponse.GetResponseStream());
                                if (resp != null && resp.error != null)
                                {
                                    fileResponse.Message = resp.error.message;
                                }

                            }
                        }
                    }

                }
            return fileResponse;
        }

        private string GetMultipartFormdata(string boundary, string name, string value)
        {
            return String.Format("--" + boundary + "\r\n" + "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n", name, value);
        }

        private string GetFileMultipartFormdataFile(string boundary, string fileName, string contentType)
        {
            return String.Format("--" + boundary + "\r\n" + "Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", fileName, contentType);
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
                return reader.ReadToEnd();
            }
        }

        private UploadEndpointResponse ReadUploadError(Stream response)
        {
            var serializer = new JavaScriptSerializer();
            var data = ReadResponseStream(response);
            if (!string.IsNullOrEmpty(data))
            {
                var result = serializer.Deserialize<UploadEndpointResponse>(data);
                return result;
            }
            return null;
            
        }
        private async Task<UploadEndpointResponse> ReadUploadErrorAsync(Stream response)
        {
            var serializer = new JavaScriptSerializer();
            var data =await ReadResponseStreamAsync(response);
            if (!string.IsNullOrEmpty(data))
            {
                var result = serializer.Deserialize<UploadEndpointResponse>(data);
                return result;
            }
            return null;

        }
    }

    internal class UploadEndpointResponse
    {
        public string jsonrpc { get; set; }
        public string result { get; set; }
        public string id { get; set; }
        public UploadEndpointError error { get; set; }
    }

    internal class UploadEndpointError
    {
        public string code { get; set; }
        public string message { get; set; }
    }
}
