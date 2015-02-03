using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Telligent.Evolution.Extensions.OAuthAuthentication.Services;
using Telligent.Evolution.RestSDK.Services;

namespace Telligent.Evolution.Extensibility.OAuthClient.Version1
{
	public static class OAuthAuthentication
	{
		/// <summary>
		/// Registers a client configuration for use with authentication methods.
		/// </summary>
		/// <param name="configuration">The configuration to register.</param>
		public static void RegisterConfiguration(IOAuthClientConfiguration configuration)
		{
			ServiceLocator.Get<IConfigurationManagerService>().Add(configuration);
		}
        public static IOAuthClientConfiguration GetConfiguration(string hostName)
        {
            var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
            return config;
        }
		/// <summary>
		/// Removes a client configuration.
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to remove.</param>
		public static void UnregisterConfiguration(string hostName)
		{
			ServiceLocator.Get<IConfigurationManagerService>().Remove(hostName);
		}

		/// <summary>
		/// Retrieves the authenticated user based on the provided OAuth configuration.  This method will not process user synchronization or creation logic.
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to use for this request.</param>
		/// <returns>Logged in user or null if the accessing user is not logged in.</returns>
		public static User GetAuthenticatedUser(string hostName)
		{
			return GetAuthenticatedUser(hostName, null, null);
		}

		/// <summary>
		/// Retrieves the authenticated user but enables user synchronization and user creation (if enabled by configuration).  When user synchronization or user creation occurs, the user will be redirected to Evolution and back to the local site.  The state parameter should include details to restore the accessing user's position in the local site.
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to use for this request.</param>
		/// <param name="state">The state of the current request that will be passed through any redirects back to Evolution to enable user creation or synchronization</param>
		/// <param name="redirect">The action that will be called to redirect, if a redirect is neccessary.  The local site should use this action to redirect to the provided Uri.</param>
		/// <returns></returns>
		public static User GetAuthenticatedUser(string hostName, NameValueCollection state, Action<Uri> redirect)
		{
			var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
			if (config != null)
				return ServiceLocator.Get<IOAuthCredentialService>().GetUser(config, state, redirect);

			return null;
		}

		/// <summary>
		/// Retreives the default user based on the provided OAuth configuration
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to use for this request.</param>
		/// <returns>The default user.</returns>
		public static User GetDefaultUser(string hostName)
		{
			var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
			if (config != null)
				return ServiceLocator.Get<IDefaultOAuthUserService>().GetDefaultUser(config);

			return null;
		}

		/// <summary>
		/// Generates the URL to which the accessing user should be directed to login to this site via OAuth.  When the user is successfully logged in, IOAuthClientConfiguration.UserLoggedIn() is called.
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to use for this request.</param>
		/// <param name="state">Any additional data that should be passed through the login process.</param>
		/// <returns>Redirection URL</returns>
		public static Uri Login(string hostName, NameValueCollection state)
		{
			var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
			if (config != null)
				return ServiceLocator.Get<IOAuthCredentialService>().GetLoginUrl(config, state);

			return null;
		}

		/// <summary>
		/// Generates a URL to Evolution and includes accessing user details to attempt to log the user in before directing the user to the Evolution URL
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to use for this request.</param>
		/// <param name="evolutionUrl">The URL within Evolution that should be accessed</param>
		/// <returns></returns>
		public static Uri AuthenticatedRedirect(string hostName, string evolutionUrl)
		{
			var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
			if (config != null)
				return ServiceLocator.Get<IOAuthCredentialService>().GetAuthenticatedRedirectUrl(config, evolutionUrl);

			return null;
		}

		/// <summary>
		/// Generates the URL to which the accesing user should be directed to attempt to log out the user from Evolution.
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to use for this request.</param>
		/// <param name="state">Any additional data that should be passed through the logout process.</param>
		/// <returns></returns>
		public static Uri EvolutionLogOut(string hostName , NameValueCollection state)
		{
			var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
			if (config != null)
				return ServiceLocator.Get<IOAuthCredentialService>().GetEvolutionLogOutUrl(config, state);

			return null;
		}

		/// <summary>
		/// Logs the accessing user out.
		/// </summary>
		/// <param name="oAuthClientConfigurationId">The ID of the OAuth client configuration to use for this request.</param>
		public static void LogOut(string hostName)
		{
			var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
			if (config != null)
				ServiceLocator.Get<IOAuthCredentialService>().Logout(config);
		}

		/// <summary>
		/// Adds user authentication details to the HTTP request
		/// </summary>
		/// <param name="user"></param>
		/// <param name="request"></param>
		public static void ApplyAuthenticationToRequest(string hostName, User user, System.Net.HttpWebRequest request)
		{
			var config = ServiceLocator.Get<IConfigurationManagerService>().Get(hostName);
			if (config != null)
			{
				if (config.EvolutionCredentials != null)
				{
					request.Credentials = config.EvolutionCredentials;
					if (user != null)
					{
						request.Headers["OAuth-Authorization"] = user.OAuthToken;
					}
				} 
				else if (user != null) 
				{
					request.Headers["Authorization"] = "OAuth " + user.OAuthToken;
				}
			}
		}
	}
}
