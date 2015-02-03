using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensions.OAuthAuthentication.Services;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Implementations
{
	internal class OAuthCredentialService : IOAuthCredentialService
	{
		private readonly IUserSyncService _userSyncService;

		internal OAuthCredentialService(IUserSyncService userSyncService)
		{
			_userSyncService = userSyncService;
		}

		#region IOAuthCredentialService Members

		public User GetUser(IOAuthClientConfiguration configuration, NameValueCollection state, Action<Uri> redirect)
		{
			var user = GetCurrentUser(configuration);
		    var syncClient = configuration as IUserSynchronizedOAuthClientConfiguration;
            var createClient = configuration as IUserCreatableOAuthClientConfiguration;

            if(createClient != null)
			    user = ValidateAgainstLocalUser(createClient, user, state, redirect);

            if(syncClient != null)
			    user = ValidateAgainstUserSynchronization(configuration as IUserSynchronizedOAuthClientConfiguration, user, state, redirect);

			if (user != null && user.TokenExpiresUtc.Subtract(DateTime.UtcNow).TotalMinutes < Constants.RefreshMarginMinutes)
				user = RefreshOAuthToken(configuration, user);

			if (user != null && string.CompareOrdinal(user.UserName, configuration.DefaultUserName) != 0)
				configuration.SetAuthorizationCookie(user.Serialize(configuration.OAuthClientSecret));
			else
				configuration.SetAuthorizationCookie(string.Empty);

			return user;
		}

		public Uri GetLoginUrl(IOAuthClientConfiguration configuration, System.Collections.Specialized.NameValueCollection state)
		{
			if (state == null)
				state = new NameValueCollection();

			state[Constants.ConfigurationIdQueryStringKey] = configuration.Name;

			return new Uri(string.Concat(
				configuration.EvolutionBaseUrl.OriginalString,
				"api.ashx/v2/oauth/authorize?client_id=",
				Uri.EscapeDataString(configuration.OAuthClientId),
				"&response_type=code&redirect_uri=",
				Uri.EscapeDataString(configuration.LocalOAuthClientHttpHandlerUrl.OriginalString),
				"&state=",
				Uri.EscapeDataString(MakeQueryString(state))
				), UriKind.Absolute);				
		}

		public void UserLoggedIn(IOAuthClientConfiguration configuration, string authorizationCode, NameValueCollection state)
		{
			var user = GetUserByAuthorizationCode(configuration, authorizationCode);
			if (user != null)
			{
				user.SynchronizedUserName = state[Constants.SynchronizedUserNameQueryStringKey];
				configuration.SetAuthorizationCookie(user.Serialize(configuration.OAuthClientSecret));
			}

			configuration.UserLoggedIn(state);
		}

		public void Logout(IOAuthClientConfiguration configuration)
		{
			configuration.SetAuthorizationCookie(string.Empty);
		}

		public Uri GetAuthenticatedRedirectUrl(IOAuthClientConfiguration configuration, string evolutionUrl)
		{
			var user = GetUser(configuration, null, null);
			return GetAuthenticatedRedirectUrlInternal(configuration, user == null ? null : user.UserName, evolutionUrl);
		}

		public Uri GetEvolutionLogOutUrl(IOAuthClientConfiguration configuration, NameValueCollection state)
		{
            var user = GetCurrentUser(configuration);
			if (user == null)
				return null;

			if (state == null)
				state = new NameValueCollection();

			state[Constants.ConfigurationIdQueryStringKey] = configuration.Name;

			HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(configuration.OAuthClientSecret));
			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

			return new Uri(
				string.Concat(
					configuration.EvolutionBaseUrl.OriginalString,
					"api.ashx/v2/oauth/logout?client_id=",
					Uri.EscapeDataString(configuration.OAuthClientId),
					"&username=",
					Uri.EscapeDataString(user.UserName),
					"&time_stamp=",
					Uri.EscapeDataString(timestamp),
					"&state=",
					Uri.EscapeDataString(MakeQueryString(state)),
					"&signature=",
					Uri.EscapeDataString(
                        Convert.ToBase64String(
						    hmac.ComputeHash(
							    Encoding.UTF8.GetBytes(
								    string.Concat(
									    user.UserName,
									    timestamp
								    )
							    )
						    )
					    )
                    )
				)
			);
		}

		#endregion

		#region Helpers

		private Uri GetAuthenticatedRedirectUrlInternal(IOAuthClientConfiguration configuration, string userName, string evolutionUrl)
		{
			if (string.IsNullOrEmpty(userName) || string.CompareOrdinal(userName, configuration.DefaultUserName) == 0)
				return new Uri(configuration.EvolutionBaseUrl, evolutionUrl.Replace("~/", configuration.EvolutionBaseUrl.OriginalString));

			HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(configuration.OAuthClientSecret));
			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

			return new Uri(
				string.Concat(
					configuration.EvolutionBaseUrl.OriginalString,
					"api.ashx/v2/oauth/redirect?client_id=",
					Uri.EscapeDataString(configuration.OAuthClientId),
					"&username=",
					Uri.EscapeDataString(userName),
					"&time_stamp=",
					Uri.EscapeDataString(timestamp),
					"&redirect_uri=",
					Uri.EscapeDataString(evolutionUrl),
					"&signature=",
					Uri.EscapeDataString(
                        Convert.ToBase64String(
						    hmac.ComputeHash(
							    Encoding.UTF8.GetBytes(
								    string.Concat(
									    userName,
									    timestamp,
									    evolutionUrl
								    )
							    )
						    )
					    )
                    )
				)
			);
		}

		private User ValidateAgainstUserSynchronization(IUserSynchronizedOAuthClientConfiguration configuration, User user, NameValueCollection state, Action<Uri> redirect)
		{
			if (!configuration.EnableEvolutionUserSynchronization || redirect == null)
				return user;
			
			var evolutionUserName = GetEvolutionAuthenticatedUserName(configuration);
			if (string.IsNullOrEmpty(evolutionUserName))
			{
				if (user != null)
					return null;
			}
			else if (user == null || (string.CompareOrdinal(evolutionUserName, user.UserName) != 0 && string.CompareOrdinal(evolutionUserName, user.SynchronizedUserName) != 0))
			{
				state[Constants.SynchronizedUserNameQueryStringKey] = evolutionUserName;

                IUserCreatableOAuthClientConfiguration userCreateClient = configuration as IUserCreatableOAuthClientConfiguration;
                userCreateClient = (IUserCreatableOAuthClientConfiguration)configuration;

                if (userCreateClient != null && userCreateClient.EnableEvolutionUserCreation)
                {
                    if (user == null)
                        redirect(GetEvolutionLogOutUrl(configuration, state));
                    else
                        redirect(GetAuthenticatedRedirectUrlInternal(configuration, user.UserName, GetLoginUrl(configuration, state).OriginalString));

                    return user;
                }
				else
                {
                    redirect(GetLoginUrl(configuration, state));
                    return null;
                }

			}

			return user;
		}

		private string GetEvolutionAuthenticatedUserName(IUserSynchronizedOAuthClientConfiguration configuration)
		{
            var value = configuration.GetEvolutionUserSynchronizationCookieValue();
			if (string.IsNullOrEmpty(value))
				return null;

			var queryString = System.Web.HttpUtility.ParseQueryString(value);
			if (queryString != null && !string.IsNullOrEmpty(queryString["lastAuthenticatedUserName"]))
				return queryString["lastAuthenticatedUserName"];
			else
				return null;
		}

        private User ValidateAgainstLocalUser(IUserCreatableOAuthClientConfiguration configuration, User user, NameValueCollection state, Action<Uri> redirect)
		{
            if (!configuration.EnableEvolutionUserCreation)
                return user;

            if(!string.IsNullOrEmpty(configuration.LocalUserName))
            {

                if (user == null || string.CompareOrdinal(user.UserName, configuration.LocalUserName) != 0)
                    if (_userSyncService.GetCreateUser(configuration))
                    {
                        IUserSynchronizedOAuthClientConfiguration syncClient = configuration as IUserSynchronizedOAuthClientConfiguration;

                        if (syncClient != null && syncClient.EnableEvolutionUserSynchronization && redirect != null)
                        {
                            redirect(GetAuthenticatedRedirectUrlInternal(syncClient, syncClient.LocalUserName, GetLoginUrl(syncClient, state).OriginalString));
                            return null;
                        }
                        else
                            return GetUserByUserName(configuration, configuration.LocalUserName);
                    }
            }
            else
            {
                if(user != null && string.CompareOrdinal(user.UserName, configuration.DefaultUserName) != 0)
                {
                    return null;
                }
            }

			return user;
		}

		private User GetUserByAuthorizationCode(IOAuthClientConfiguration configuration, string authorizationCode)
		{
			var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/oauth/token");
			request.Timeout = Constants.RequestTimeoutMilliseconds;
			ApplyHeaders(configuration, request, null);
			request.Method = "POST";

			string data = string.Concat(
				"client_id=",
				Uri.EscapeDataString(configuration.OAuthClientId),
				"&client_secret=",
				Uri.EscapeDataString(configuration.OAuthClientSecret),
				"&grant_type=authorization_code&code=",
				Uri.EscapeDataString(authorizationCode),
				"&redirect_uri=",
				Uri.EscapeDataString(configuration.LocalOAuthClientHttpHandlerUrl.OriginalString)
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

			var user = GetUserByAccessToken(configuration, response.access_token);

			user.OAuthToken = response.access_token;
			user.RefreshToken = response.refresh_token;
			user.TokenExpiresUtc = DateTime.UtcNow.AddSeconds(response.expires_in);

			return user;
		}

		private User GetUserByAccessToken(IOAuthClientConfiguration configuration, string accessToken)
		{
			string userName;
			int userId;
			GetUserInfoByAccessToken(configuration, accessToken, out userName, out userId);

			var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/users/" + userId.ToString("0") + ".xml?IncludeFields=User.Language");
			request.Timeout = Constants.RequestTimeoutMilliseconds;
			ApplyHeaders(configuration, request, accessToken);
			request.Method = "GET";

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
				throw new Exception("An error occured while attempting to acquire the accessing user language", e);
			}

			try
			{
				var language = (from userLanguage in XElement.Parse(rawResponse).Descendants("Language") select userLanguage.Value).FirstOrDefault();
				if (!string.IsNullOrEmpty(language))
					return new User(userName, userId, language);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("An error occured while attempting to parse the accessing user language", ex);
			}

			throw new InvalidOperationException("The accessing user language could not be determined");
		}

		private void GetUserInfoByAccessToken(IOAuthClientConfiguration configuration, string accessToken, out string userName, out int userId)
		{
			var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/info.xml?IncludeFields=InfoResult.AccessingUser,InfoResult.AccessingUserId");
			request.Timeout = Constants.RequestTimeoutMilliseconds;
			ApplyHeaders(configuration, request, accessToken);
			request.Method = "GET";

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
				throw new Exception("An error occured while attempting to acquire the accessing user name", e);
			}

			try
			{
				var xml = XElement.Parse(rawResponse);
				userName = (from accessingUser in xml.Descendants("AccessingUser") select accessingUser.Value).FirstOrDefault();
				userId = int.Parse((from accessingUser in xml.Descendants("AccessingUserId") select accessingUser.Value).FirstOrDefault());
				return;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("An error occured while attempting to parse the accessing user name", ex);
			}

			throw new InvalidOperationException("The accessing user could not be determined");
		}

		private string MakeQueryString(NameValueCollection keysAndValues)
		{
			StringBuilder queryString = new StringBuilder();
			if (keysAndValues != null)
			{
				bool isFirst = true;
				foreach (string key in keysAndValues.Keys)
				{
					foreach (string value in keysAndValues.GetValues(key))
					{
						if (isFirst)
							isFirst = false;
						else
							queryString.Append("&");

						queryString.Append(Uri.EscapeDataString(key));
						queryString.Append("=");
						queryString.Append(Uri.EscapeDataString(value));
					}
				}
			}

			return queryString.ToString();
		}

		private User GetCurrentUser(IOAuthClientConfiguration configuration)
		{
			User user = null;

			var cookieValue = configuration.GetAuthorizationCookieValue();
			if (!string.IsNullOrEmpty(cookieValue))
				user = User.Deserialize(cookieValue, configuration.OAuthClientSecret);

			return user;
		}

		private User GetUserByUserName(IOAuthClientConfiguration configuration, string userName)
		{
			var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/oauth/token");
			request.Timeout = Constants.RequestTimeoutMilliseconds;
			ApplyHeaders(configuration, request, null);
			request.Method = "POST";

			string data = string.Concat(
				"client_id=",
				Uri.EscapeDataString(configuration.OAuthClientId),
				"&client_secret=",
				Uri.EscapeDataString(configuration.OAuthClientSecret),
				"&grant_type=client_credentials&username=",
				Uri.EscapeDataString(userName)
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

			var user = GetUserByAccessToken(configuration, response.access_token);

			if (string.CompareOrdinal(user.UserName, userName) != 0)
				return null;

			user.OAuthToken = response.access_token;
			user.RefreshToken = response.refresh_token;
			user.TokenExpiresUtc = DateTime.UtcNow.AddSeconds(response.expires_in);

			return user;
		}

		private User RefreshOAuthToken(IOAuthClientConfiguration configuration, User user)
		{
			lock (user.SyncRoot)
			{
				if (user.TokenExpiresUtc.Subtract(DateTime.UtcNow).TotalMinutes >= Constants.RefreshMarginMinutes)
					return user;

				var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/oauth/token");
				request.Timeout = Constants.RequestTimeoutMilliseconds;
				ApplyHeaders(configuration, request, null);
				request.Method = "POST";

				string data = string.Concat(
					"client_id=",
					Uri.EscapeDataString(configuration.OAuthClientId),
					"&client_secret=",
					Uri.EscapeDataString(configuration.OAuthClientSecret),
					"&grant_type=refresh_token&refresh_token=",
					Uri.EscapeDataString(user.RefreshToken),
					"&redirect_uri=",
					Uri.EscapeDataString(configuration.LocalOAuthClientHttpHandlerUrl.OriginalString)
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
				catch
				{
					return null;
				}

				var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
				var response = serializer.Deserialize<OAuthResponse>(rawResponse);

				if (!string.IsNullOrEmpty(response.error))
					throw new Exception(response.error);

				user.OAuthToken = response.access_token;
				user.RefreshToken = response.refresh_token;
				user.TokenExpiresUtc = DateTime.UtcNow.AddSeconds(response.expires_in);

				return user;
			}
		}

		void ApplyHeaders(IOAuthClientConfiguration configuration, HttpWebRequest request, string accessToken)
		{
			if (configuration.EvolutionCredentials != null)
			{
				request.Credentials = configuration.EvolutionCredentials;
				if (!string.IsNullOrEmpty(accessToken))
					request.Headers["OAuth-Authorization"] = accessToken;
			}
			else if (!string.IsNullOrEmpty(accessToken))
			{
				request.Headers["Authorization"] = "OAuth " + accessToken;
			}
		}

		#endregion
	}
}
