using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Telligent.RestSDK.IntegrationTests
{
   
    [SetUpFixture]
    public class Setup
    {
        public static string Token = null;

        /// <summary>
        /// Modify the following values with values from your community in order to execute tests
        /// </summary>
        private static readonly string _oauthClientId = "5e539135-4807-4d39-b525-e36c343ffc1d";
        private static readonly string _oauthSecret = "34c70aa225d842c7a713f4eaa40424a0d63551ee7f124b91b2a69b2694751eac";
        private static readonly string _communityUserName = "admin";
        public static readonly string CommunityUrl = "http://trunk.local.com/";

        [SetUp]
        public void SetupTests()
        {
            if (Token == null)
            {
                var request = (HttpWebRequest)WebRequest.Create(CommunityUrl + "api.ashx/v2/oauth/token");
                request.Timeout = 30000;
                request.Method = "POST";

                string data = string.Concat(
                    "client_id=",
                    Uri.EscapeDataString(_oauthClientId),
                    "&client_secret=",
                    Uri.EscapeDataString(_oauthSecret),
                    "&grant_type=client_credentials&username=",
                    Uri.EscapeDataString(_communityUserName)
                    );

                byte[] bytes = Encoding.UTF8.GetBytes(data);

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;

                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }

                string rawResponse = null;
                try
                {
                    using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            rawResponse = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("An error occured while attempting to acquire a refresh token for an authorization code", e);
                }

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var response = serializer.Deserialize<OAuthResponse>(rawResponse);

                if (!string.IsNullOrEmpty(response.error))
                    throw new Exception(response.error);

                Token = response.access_token;
            }
        }
    }
    public class OAuthResponse
    {
        public string error { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}
