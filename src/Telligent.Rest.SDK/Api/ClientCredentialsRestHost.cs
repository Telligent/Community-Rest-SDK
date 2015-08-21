using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;


namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public class ClientCredentialsRestHost : RestHost
    {
        private string _communityUrl = null;

        private string DefaultUsername { get; set; }
        private string ImpersonatingUsername { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        public ClientCredentialsRestHost(string defaultUsername, string communityUrl,string clientId,string clientSecret)
            : base()
        {
            _communityUrl = communityUrl.EndsWith("/") ? communityUrl : communityUrl + "/"; ;
            ClientId = clientId;
            ClientSecret = clientSecret;
            DefaultUsername = defaultUsername;
        }

       
        public override int PostTimeout
        {
            get { return 300000; }
        }

        public override string Name
        {
            get { return "Client Credentials Rest Host"; }
        }

        public ClientCredentialsRestHost():base()
        {
            var config = ConfigurationManager.AppSettings;

            if(string.IsNullOrEmpty(config["defaultUsername"]))
                throw new ConfigurationErrorsException("defaultUsername is expected in the appSettings section of the application configuration");

            if (string.IsNullOrEmpty(config["communityUrl"]))
                throw new ConfigurationErrorsException("communityUrl is expected in the appSettings section of the application configuration");

            if (string.IsNullOrEmpty(config["clientId"]))
                throw new ConfigurationErrorsException("clientId is expected in the appSettings section of the application configuration");

            if (string.IsNullOrEmpty(config["clientSecret"]))
                throw new ConfigurationErrorsException("clientSecret is expected in the appSettings section of the application configuration");

            this.DefaultUsername = config["DefaultUsername"];
            this.ClientId = config["clientId"];
            this.ClientSecret = config["clientSecret"];

            var url = config["communityUrl"];
            _communityUrl = url.EndsWith("/") ? url : url + "/";
        }
        public override void ApplyAuthenticationToHostRequest(System.Net.HttpWebRequest request, bool forAccessingUser)
        {
            OauthUser user = null;
            if (forAccessingUser && !string.IsNullOrEmpty(ImpersonatingUsername))
                user = GetUser(this.ImpersonatingUsername);

            if (user == null)
                user = GetUser(DefaultUsername);

            request.Headers["Authorization"] = "OAuth " + user.Token;
        }

        public override string EvolutionRootUrl
        {
            get { return _communityUrl; }

        }

        public void Impersonate(string username, Action<ClientCredentialsRestHost> impersonatedActions)
        {
            this.ImpersonatingUsername = username;
            impersonatedActions(this);
            this.ImpersonatingUsername = null;
        }
        public async void ImpersonateAsync(string username, Action<ClientCredentialsRestHost> impersonatedActions)
        {
            this.ImpersonatingUsername = username;
            impersonatedActions(this);
            this.ImpersonatingUsername = null;
        }

        private OauthUser GetUser(string username)
        {
            string userCacheKey = GetUserCacheKey(username);
            var user = this.Cache.Get(userCacheKey) as OauthUser;

            if (user == null)
            {
                user = GetToken(username);
                this.Cache.Put(userCacheKey, user, 600);
            }
            else
            {
                if (user.TokenExpiresUtc.Subtract(DateTime.UtcNow).TotalMinutes <= 3)
                {
                    user = RefreshToken(user);
                    this.Cache.Remove(userCacheKey);
                    this.Cache.Put(userCacheKey, user, 600);
                }
            }

            return user;


        }

        private static readonly string _userFmtString = "SDK::User::{0}";

        private string GetUserCacheKey(string username)
        {
            return string.Format(_userFmtString, username);

        }

        private OauthUser RefreshToken(OauthUser user)
        {
            var request = (HttpWebRequest) WebRequest.Create(this.EvolutionRootUrl + "api.ashx/v2/oauth/token");
            request.Timeout = this.PostTimeout;
            request.Method = "POST";

            string data = string.Concat(
                "client_id=",
                Uri.EscapeDataString(ClientId),
                "&client_secret=",
                Uri.EscapeDataString(ClientSecret),
                "&grant_type=refresh_token&refresh_token=",
                Uri.EscapeDataString(user.RefreshToken)

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
                using (HttpWebResponse webResponse = (HttpWebResponse) request.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        rawResponse = reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                throw new Exception(
                    "An error occured while attempting to acquire a refresh token for an authorization code");
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var response = serializer.Deserialize<OauthResponse>(rawResponse);

            if (!string.IsNullOrEmpty(response.error))
                throw new Exception(response.error);

            return new OauthUser(user.Username, response);

            return user;

        }

        private OauthUser GetToken(string username)
        {
            var request = (HttpWebRequest) WebRequest.Create(this.EvolutionRootUrl + "api.ashx/v2/oauth/token");
            request.Timeout = this.PostTimeout;
            request.Method = "POST";

            string data = string.Concat(
                "client_id=",
                Uri.EscapeDataString(ClientId),
                "&client_secret=",
                Uri.EscapeDataString(ClientSecret),
                "&grant_type=client_credentials&username=",
                Uri.EscapeDataString(username)
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
                using (HttpWebResponse webResponse = (HttpWebResponse) request.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        rawResponse = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occured while attempting to acquire a token for an authorization code", e);
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var response = serializer.Deserialize<OauthResponse>(rawResponse);

            if (!string.IsNullOrEmpty(response.error))
                throw new Exception(response.error);


            return new OauthUser(username, response);

        }

        internal class OauthResponse
        {
            public string error { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }

        }

        internal class OauthUser
        {
            public OauthUser()
            {

            }

            public OauthUser(string username, OauthResponse response)
            {
                Username = username;
                Token = response.access_token;
                TokenExpiresUtc = DateTime.UtcNow.AddSeconds(response.expires_in);
                RefreshToken = response.refresh_token;
            }

            public string Username { get; set; }
            public string Token { get; set; }
            public string RefreshToken { get; set; }
            public DateTime TokenExpiresUtc { get; set; }
        }
    }
}