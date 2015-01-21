using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensions.OAuthAuthentication.Services;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Implementations
{
	internal class DefaultOAuthUserService: IDefaultOAuthUserService
	{
		
		private object _defaultUserLock = new object();
	    private static readonly string _defaultUserItemKey = "_oauth_defaultUser";

		#region IDefaultOAuthUserService Members

		public User GetDefaultUser(IOAuthClientConfiguration configuration)
		{
		    User defaultUser = null;
            defaultUser = configuration.Items[_defaultUserItemKey] as User;

            if (defaultUser != null && defaultUser.TokenExpiresUtc.Subtract(DateTime.UtcNow).TotalMinutes >= Constants.RefreshMarginMinutes)
                return defaultUser;

			lock (_defaultUserLock)
			{
                if (defaultUser != null && defaultUser.TokenExpiresUtc.Subtract(DateTime.UtcNow).TotalMinutes >= Constants.RefreshMarginMinutes)
                    return defaultUser;

				var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/oauth/token");
				request.Timeout = Constants.RequestTimeoutMilliseconds;
				
				if (configuration.EvolutionCredentials != null)
					request.Credentials = configuration.EvolutionCredentials;

				request.Method = "POST";

				string data = string.Concat(
					"client_id=",
					Uri.EscapeDataString(configuration.OAuthClientId),
					"&client_secret=",
					Uri.EscapeDataString(configuration.OAuthClientSecret),
					"&grant_type=client_credentials&username=",
					Uri.EscapeDataString(configuration.DefaultUserName)
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
					throw new Exception("An error occured while attempting to authorize the default user", e);
				}

				var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
				var response = serializer.Deserialize<OAuthResponse>(rawResponse);

				if (!string.IsNullOrEmpty(response.error))
					throw new Exception(response.error);

				var user = new User(configuration.DefaultUserName, GetUserIdByAccessToken(configuration, response.access_token), configuration.DefaultUserLanguageKey);
				user.OAuthToken = response.access_token;
				user.RefreshToken = response.refresh_token;
				user.TokenExpiresUtc = DateTime.UtcNow.AddSeconds(response.expires_in);

                defaultUser = user;
			    configuration.Items[_defaultUserItemKey] = user;
                return defaultUser;
			}
		}

		#endregion

		#region Helpers

		private int GetUserIdByAccessToken(IOAuthClientConfiguration configuration, string accessToken)
		{
			var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/info.xml?IncludeFields=InfoResult.AccessingUser,InfoResult.AccessingUserId");
			request.Timeout = Constants.RequestTimeoutMilliseconds;

			if (configuration.EvolutionCredentials != null)
			{
				request.Credentials = configuration.EvolutionCredentials;
				request.Headers["OAuth-Authorization"] = accessToken;
			}
			else
			{
				request.Headers["Authorization"] = "OAuth " + accessToken;
			}

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
				throw new Exception("An error occured while attempting to acquire default user details", e);
			}

			try
			{
				return int.Parse((from accessingUser in XElement.Parse(rawResponse).Descendants("AccessingUserId") select accessingUser.Value).FirstOrDefault());
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("An error occured while attempting to parse the default user's Id", ex);
			}

			throw new InvalidOperationException("Default user details could not be determined");
		}

		#endregion
	}
}
