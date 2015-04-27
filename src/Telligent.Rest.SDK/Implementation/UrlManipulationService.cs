using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;
using System.Web;


namespace Telligent.Rest.SDK
{
    public class UrlManipulationService : IUrlManipulationService
    {
        readonly IEncode Encode;
        readonly IDecode Decode;

        public UrlManipulationService(IEncode encode,IDecode decode)
        {
            Encode = encode;
            Decode = decode;
        }

        public string ModifyUrl(string url, string queryStringModification, string targetModification)
        {
            string queryString = string.Empty;
            string targetLocation = string.Empty;

            if (url.Contains("#"))
            {
                targetLocation = url.Substring(url.IndexOf("#") + 1);
                url = url.Substring(0, url.IndexOf("#"));
            }

            if (url.Contains("?"))
            {
                queryString = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }

            if (!string.IsNullOrEmpty(queryStringModification))
            {
                NameValueCollection qs = ParseQueryString(queryString);
                NameValueCollection newQs = ParseQueryString(queryStringModification);  

                foreach (string key in newQs.AllKeys)
                {
                    qs.Remove(key);
                    foreach (string value in newQs.GetValues(key))
                    {
                        if (!string.IsNullOrEmpty(value))
                            qs.Add(key, value);
                    }
                }

                queryString = MakeQueryString(qs);
            }

            if (!string.IsNullOrEmpty(targetModification))
                targetLocation = targetModification;

            return url + (string.IsNullOrEmpty(queryString) ? "" : ("?" + queryString)) + (string.IsNullOrEmpty(targetLocation) ? "" : ("#" + targetLocation));
        }

        public NameValueCollection ParseQueryString(string queryString)
        {
            NameValueCollection items = new NameValueCollection();

            if (!string.IsNullOrEmpty(queryString))
            {
                int index = queryString.IndexOf('?');
                if (index >= 0)
                    queryString = queryString.Substring(index + 1);

                foreach (string keyValuePair in queryString.Split('&'))
                {
                    string[] keyValue = keyValuePair.Split('=');
                    if (keyValue.Length == 2)
                        items.Add(Decode.UrlDecode(keyValue[0]), Decode.UrlDecode(keyValue[1]));
                }
            }

            return items;
        }

        public string MakeQueryString(NameValueCollection queryStringValues)
        {
            if (queryStringValues == null || queryStringValues.Count == 0)
                return string.Empty;

            StringBuilder qs = new StringBuilder();
            foreach (string key in queryStringValues.AllKeys)
            {
                foreach (string value in queryStringValues.GetValues(key))
                {
                    if (qs.Length > 0)
                        qs.Append("&");

                    qs.Append(Encode.UrlEncode(key));
                    qs.Append("=");
                    qs.Append(Encode.UrlEncode(value));
                }
            }

            return qs.ToString();
        }

        public string ConvertQueryStringToHash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            return url.Replace('?', '#');
        }


        public CallbackUrlData ParseCallbackUrl(string rawUrl)
        {
            string url = rawUrl;
            url = url.Substring(url.IndexOf("/rhn_") + 5);


            int endOfName = url.IndexOf("/");
            string name = url.Substring(0, endOfName);
            url = url.Substring(name.Length +1);

            CallbackUrlData data = new CallbackUrlData();
            data.HostName = HttpUtility.UrlDecode(name);
            data.Url = url;
            return data;
        }
        public string GetCallbackUrl(HttpContextBase context,string callbackUrlBase,string hostName, string path, string queryString)
        {
            string url = ModifyUrl(string.Concat(callbackUrlBase, "/rhn_", HttpUtility.UrlEncode(hostName), "/", path), queryString, null);
            if (string.IsNullOrEmpty(url))
                return url;

            try
            {
                if (context == null)
                    return url;

                return new Uri(context.Request.Url, url).OriginalString;
            }
            catch
            {
                return url;
            }
        }
    }
}
