using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Rest.SDK.Model;

namespace Telligent.Rest.SDK.Implementation
{
    public interface IProxyService
    {
        string UnescapeRemoteUrl(Host host, string url);
        string MakeFullUrl(Host host, string url);
        void ProxyOrRedirect(Host host, string url, System.Web.HttpContextBase context);
    }

    public class ProxyService : IProxyService
    {
        private readonly string ProxyInfoCacheKey = "RSW_ProxyInfo";
        static Dictionary<string, Action<System.Web.HttpResponseBase, string>> RestrictedResponseHeaders = new Dictionary<string, Action<System.Web.HttpResponseBase, string>>
		{
			{ "Content-Type", (r, v) => r.ContentType = v },
			{ "Content-Length", (r, v) => { 
				long length; 
				if (long.TryParse(v, out length) && length > -1)
					r.Headers["Content-Length"] = v;
			}}
		};
      
        static Dictionary<string, Action<HttpWebRequest, string>> RestrictedRequestHeaders = new Dictionary<string, Action<HttpWebRequest, string>>
		{
			{ "Accept", (r, v) => r.Accept = v },
			{ "Connection", (r, v) => r.Connection = v },
			{ "Content-Type", (r, v) => r.ContentType = v },
			{ "Date", (r, v) => r.Date = DateTime.Parse(v) },
			{ "Expect", (r, v) => r.Expect = v },
			{ "Host", (r, v) => r.Host = v },
			{ "If-Modified-Since", (r, v) => r.IfModifiedSince = DateTime.Parse(v) },
			{ "Range", (r, v) => { 
				if (v.StartsWith("bytes=")) 
				{
					foreach (var rangeSet in v.Substring(6).Split(','))
						{
							var range = rangeSet.Split(new char[] {'-'}, StringSplitOptions.None);
							if (range.Length == 2)
							{
								int range1, range2;
								if (range[0].Length > 0 && int.TryParse(range[0], out range1))
								{
									if (range[1].Length > 0 && int.TryParse(range[1], out range2))
										r.AddRange(range1, range2);
									else
										r.AddRange(range1);
								}
								else if (range[1].Length > 0 && int.TryParse(range[1], out range2))
									r.AddRange(-range2);
							}
						}
					}
				}
				},
			{ "Referer", (r, v) => r.Referer = v },
			{ "Transfer-Encoding", (r, v) => r.TransferEncoding = v },
			{ "User-Agent", (r, v) => r.UserAgent = v }
		};

        private IRest _rest;
        public ProxyService(IRest restProxy)
        {
            _rest = restProxy;
        }
        private string UnescapeRemoteUrl(Host host, string url, System.Web.HttpContextBase context)
        {
            string unescapedUrl = UnescapeRemoteUrl(host, url);
            if (string.IsNullOrEmpty(unescapedUrl))
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "The URL could not be decoded.";
                context.Response.End();
            }

            return unescapedUrl;
        }

        public string UnescapeRemoteUrl(Host host, string url)
        {
            var cacheKey = "RemotingServer-RemoteUrl:" + url;
            var unescapedUrl = (string)host.Cache.Get(cacheKey);
            if (unescapedUrl == null)
            {
                var element = _rest.GetEndpointXml(host, 2, "remoting/url/unencode.xml?Url=" + Uri.EscapeDataString(url), false).Descendants("Url").FirstOrDefault();
                if (element == null || string.IsNullOrEmpty(element.Value))
                    unescapedUrl = string.Empty;
                else
                    unescapedUrl = element.Value;

                host.Cache.Put(cacheKey, unescapedUrl, 30 * 60);
            }

            return unescapedUrl;
        }
        public void ProxyOrRedirect(Host host, string url, System.Web.HttpContextBase context)
        {
            var proxyInfo = GetProxyInfo(host);
            if (string.IsNullOrEmpty(url))
            {
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "The URL was null or empty.";
            }

            url = UnescapeRemoteUrl(host, url, context);
            if (!string.IsNullOrEmpty(url) && proxyInfo.ShouldProxy != null && proxyInfo.ShouldProxy.IsMatch(url))
                Proxy(host, context, url, proxyInfo);
            else
                Redirect(host, context, url);
        }

        private void Proxy(Host  host, System.Web.HttpContextBase context, string url, ProxyInfo proxyInfo)
        {
            CachedProxyResponse cachedResponse = null;
            string cacheKey = null;
            if (context.Request.HttpMethod == "GET")
            {
                cacheKey = string.Concat(host.Name, url);
                cachedResponse = (CachedProxyResponse)System.Web.HttpContext.Current.Items[cacheKey];
                if (cachedResponse == null)
                    cachedResponse = (CachedProxyResponse)host.Cache.Get(cacheKey);

                if (cachedResponse != null)
                {
                    context.Response.StatusCode = cachedResponse.Status;
                    context.Response.StatusDescription = cachedResponse.StatusDescription;
                    context.Response.TrySkipIisCustomErrors = true;

                    foreach (string name in cachedResponse.Headers.Keys)
                    {
                        if (RestrictedResponseHeaders.ContainsKey(name))
                            AttemptSetRestrictedHeader(context.Response, name, cachedResponse.Headers[name]);
                        else
                            context.Response.Headers[name] = cachedResponse.Headers[name];
                    }

                    using (var outputStream = context.Response.OutputStream)
                    {
                        outputStream.Write(cachedResponse.Content, 0, cachedResponse.Content.Length);
                        outputStream.Close();
                    }

                    return;
                }
            }

            bool retry = true;
            HttpWebRequest request = null;
            while (retry)
            {
                retry = false;

                try
                {
                    request = (HttpWebRequest)WebRequest.Create(MakeFullUrl(host, url));
                    request.Method = context.Request.HttpMethod;
                    request.Timeout = context.Request.HttpMethod == "GET" ? host.GetTimeout : host.PostTimeout;
                    host.ApplyAuthenticationToHostRequest(request, true);
                    host.ApplyRemoteHeadersToRequest(request);

                    foreach (string name in proxyInfo.RequestHeaders)
                    {
                        if (name.StartsWith("Cookie:"))
                        {
                            var cookieName = name.Substring(7);
                            var cookie = context.Request.Cookies[cookieName];
                            if (cookie != null)
                            {
                                if (request.CookieContainer == null)
                                    request.CookieContainer = new CookieContainer();

                                request.CookieContainer.Add(new Cookie(cookieName, cookie.Value, "/", request.RequestUri.Host));
                            }
                        }
                        else
                        {
                            var value = context.Request.Headers[name];
                            if (!string.IsNullOrEmpty(value))
                            {
                                if (WebHeaderCollection.IsRestricted(name))
                                    AttemptSetRestrictedHeader(request, name, value);
                                else
                                    request.Headers[name] = value;
                            }
                        }
                    }

                    if (request.Method == "POST")
                    {
                        // assume the content type is x-www-form-urlencoded if it isn't set (not great, but this occurs in the wild)
                        if (string.IsNullOrEmpty(request.ContentType) || request.ContentType.StartsWith("application/x-www-form-urlencoded"))
                        {
                            var buffer = new byte[(int)context.Request.ContentLength];
                            using (var inStream = context.Request.InputStream)
                            {
                                inStream.Read(buffer, 0, buffer.Length);
                                inStream.Close();
                            }

                            try
                            {
                                buffer = Encoding.UTF8.GetBytes(host.ResolveRemoteUrlsToHostUrls(Encoding.UTF8.GetString(buffer)));
                            }
                            catch
                            {
                                // this is likely because the content type wasn't set correctly, ignore.
                            }

                            request.ContentLength = buffer.Length;

                            using (var outStream = request.GetRequestStream())
                            {
                                outStream.Write(buffer, 0, buffer.Length);
                                outStream.Close();
                            }
                        }
                        else
                        {
                            request.ContentLength = context.Request.ContentLength;
                            var buffer = new byte[8 * 1024];
                            int read;

                            using (var inStream = context.Request.InputStream)
                            {
                                using (var outStream = request.GetRequestStream())
                                {
                                    while ((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        outStream.Write(buffer, 0, read);
                                    }

                                    outStream.Close();
                                }

                                inStream.Close();
                            }
                        }
                    }

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        ProcessProxiedResponse(host, context, proxyInfo, request, response, cacheKey);
                    }
                }
                catch (WebException ex)
                {
                    var errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse != null)
                    {
                        if (errorResponse.StatusCode == HttpStatusCode.Forbidden && host.RetryFailedRemoteRequest(request))
                            retry = true;

                        if (!retry)
                            ProcessProxiedResponse(host, context, proxyInfo, request, errorResponse, cacheKey);
                    }
                    else
                        throw;
                }
            }
        }
        private ProxyInfo GetProxyInfo(Host host)
        {
            var proxyInfo = (ProxyInfo)host.Cache.Get(ProxyInfoCacheKey);
            if (proxyInfo == null)
            {
                var x = _rest .GetEndpointXml(host,2, "remoting/proxyinfo.xml", false);
                proxyInfo = new ProxyInfo();

                var e = x.Descendants("UrlPattern").FirstOrDefault();
                if (e != null)
                    proxyInfo.ShouldProxy = new Regex(e.Value, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                e = x.Descendants("HttpRequestHeaders").FirstOrDefault();
                if (e != null)
                    proxyInfo.RequestHeaders = e.Descendants("string").Select(y => y.Value).ToArray();
                else
                    proxyInfo.RequestHeaders = new string[0];

                e = x.Descendants("HttpResponseHeaders").FirstOrDefault();
                if (e != null)
                    proxyInfo.ResponseHeaders = e.Descendants("string").Select(y => y.Value).ToArray();
                else
                    proxyInfo.ResponseHeaders = new string[0];

                host.Cache.Put(ProxyInfoCacheKey, proxyInfo, 60 * 10);
            }

            return proxyInfo;
        }
        private void ProcessProxiedResponse(Host host, System.Web.HttpContextBase context, ProxyInfo proxyInfo, HttpWebRequest request, HttpWebResponse response, string cacheKey)
        {
            var buffer = new byte[8 * 1024];
            int read;
            CachedProxyResponse cachedResponse = null;
            var cacheInfo = (request.Method != "GET") ? null : GetCacheInfo(response);
            MemoryStream cachedOutput = null;
            if (cacheInfo != null && ((cacheInfo.IsPublic && response.ContentLength < 256 * 1024 && ((int)response.StatusCode) - 200 < 100) || response.ContentLength < 64 * 1024))
            {
                cachedResponse = new CachedProxyResponse { Headers = new Dictionary<string, string>(), Status = ((int)response.StatusCode), StatusDescription = response.StatusDescription };
                cachedOutput = new MemoryStream();
            }

            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.StatusDescription = response.StatusDescription;
            context.Response.TrySkipIisCustomErrors = true;

            foreach (string name in proxyInfo.ResponseHeaders)
            {
                if (!string.IsNullOrEmpty(response.Headers[name]))
                {
                    if (RestrictedResponseHeaders.ContainsKey(name))
                        AttemptSetRestrictedHeader(context.Response, name, response.Headers[name]);
                    else
                        context.Response.Headers[name] = response.Headers[name];

                    if (cachedResponse != null)
                        cachedResponse.Headers[name] = response.Headers[name];
                }
            }

            using (var outStream = context.Response.OutputStream)
            {
                using (var inStream = response.GetResponseStream())
                {
                    while ((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outStream.Write(buffer, 0, read);
                        outStream.Flush();
                        if (cachedOutput != null)
                            cachedOutput.Write(buffer, 0, read);
                    }

                    inStream.Close();
                }

                outStream.Close();
            }

            if (cacheInfo != null && cachedResponse != null && cachedOutput != null && cachedOutput.Length < 256000)
            {
                cachedResponse.Content = cachedOutput.ToArray();
                if (cacheInfo.IsPublic)
                    host.Cache.Put(cacheKey, cachedResponse, cacheInfo.CacheDurationSeconds);
                else
                    System.Web.HttpContext.Current.Items[cacheKey] = cachedResponse;
            }
        }
        private void AttemptSetRestrictedHeader(HttpWebRequest request, string name, string value)
        {
            Action<HttpWebRequest, string> a;
            if (RestrictedRequestHeaders.TryGetValue(name, out a))
                a(request, value);
        }
        private CacheInfo GetCacheInfo(WebResponse response)
        {
            CacheInfo cacheInfo = null;
            var header = response.Headers["Cache-Control"];
            if (string.IsNullOrEmpty(header) || header == "public")
                cacheInfo = new CacheInfo { IsPublic = true };
            else if (header == "private")
                cacheInfo = new CacheInfo { IsPublic = false };
            else
                return null;

            DateTime expiresDate;
            if (!DateTime.TryParse(response.Headers["Expires"], out expiresDate))
                cacheInfo.CacheDurationSeconds = 60 * 5;
            else
            {
                cacheInfo.CacheDurationSeconds = (int)expiresDate.ToUniversalTime().Subtract(DateTime.UtcNow).TotalSeconds;
                if (cacheInfo.CacheDurationSeconds <= 0)
                    return null;
            }

            return cacheInfo;
        }
        private void AttemptSetRestrictedHeader(System.Web.HttpResponseBase response, string name, string value)
        {
            Action<System.Web.HttpResponseBase, string> a;
            if (RestrictedResponseHeaders.TryGetValue(name, out a))
                a(response, value);
        }
        private void Redirect(Host host, System.Web.HttpContextBase context, string url)
        {
            context.Response.Redirect(MakeFullUrl(host, host.GetEvolutionRedirectUrl(url)));
        }
        public string MakeFullUrl(Host host, string url)
        {
            if (string.IsNullOrEmpty(url) || url.StartsWith("#"))
                return url;

            // support app-relative urls mapped to the Evolution Root
            if (url.StartsWith("~/"))
                url = url.Substring(2);

            Uri baseUrl = new Uri(host.EvolutionRootUrl);
            Uri combinedUrl = new Uri(baseUrl, url);

            string[] components = combinedUrl.AbsoluteUri.Split(new char[] { '?' }, 2);
            if (components.Length == 2)
                components[1] = host.ResolveRemoteUrlsToHostUrls(components[1]);

            return string.Join("?", components);
        }
    }

    public class ProxyInfo
    {
        public System.Text.RegularExpressions.Regex ShouldProxy { get; set; }
        public string[] RequestHeaders { get; set; }
        public string[] ResponseHeaders { get; set; }
    }

    public class CacheInfo
    {
        public bool IsPublic { get; set; }
        public int CacheDurationSeconds { get; set; }
    }

    public class CachedProxyResponse
    {
        public Dictionary<string, string> Headers;
        public byte[] Content { get; set; }
        public int Status { get; set; }
        public string StatusDescription { get; set; }
    }
}
