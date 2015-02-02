using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using Telligent.Evolution.Extensions.OAuthAuthentication.Services;

namespace Telligent.Evolution.Extensibility.OAuthClient.Version1
{
    public enum ErrorResponse { UnknownError, AccessDenied, InvalidUsername, InvalidEmail, DuplicateEmail }

    public interface IOAuthClientConfiguration
    {
        /// <summary>
        /// The unique, web-node-consistent ID representing this configuration.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Provides a place where items can be stored for the duration of the instance of the implmenting object
        /// </summary>
        Hashtable Items { get; }

        /// <summary>
        /// The base URL of the Evolution instance to authenticate against
        /// </summary>
        Uri EvolutionBaseUrl { get; }

        /// <summary>
        /// Identifies the full URL to the local OAuth Client HTTP Handler
        /// </summary>
        Uri LocalOAuthClientHttpHandlerUrl { get; }

        /// <summary>
        /// The OAuth client ID representing the local application
        /// </summary>
        string OAuthClientId { get; }

        /// <summary>
        /// The OAuth client secret associated to the local application
        /// </summary>
        string OAuthClientSecret { get; }

        /// <summary>
        /// Network credentials required when accessing Evolution (if Windows Auth is enabled, for example)
        /// </summary>
        NetworkCredential EvolutionCredentials { get; }

        /// <summary>
        /// The default user name to use when the local user is not logged in
        /// </summary>
        string DefaultUserName { get; }

        /// <summary>
        /// The default user language when the local user is not logged in
        /// </summary>
        string DefaultUserLanguageKey { get; }

        /// <summary>
        /// Called when the user is first logged in.
        /// </summary>
        /// <param name="value"></param>
        void SetAuthorizationCookie(string value);

        /// <summary>
        /// Called to retrieve information about the logged in user from the authorization cookie.
        /// </summary>
        /// <returns></returns>
        string GetAuthorizationCookieValue();

        /// <summary>
        /// Called when a user is successfully logged in
        /// </summary>
        /// <param name="state"></param>
        void UserLoggedIn(NameValueCollection state);

        /// <summary>
        /// Called when authenticating with Evolution fails or access is not granted
        /// </summary>
        /// <param name="state"></param>
        void UserLoginFailed(NameValueCollection state);

		/// <summary>
		/// Called when a logout request issued to Evolution is completed successfullly
		/// </summary>
		/// <param name="state"></param>
		void UserLoggedOut(NameValueCollection state);

		/// <summary>
		/// Called when a logout request issued to Evolution fails
		/// </summary>
		/// <param name="state"></param>
		void UserLogOutFailed(NameValueCollection state);

       
    }

    public interface IUserSynchronizedOAuthClientConfiguration:IOAuthClientConfiguration
    {
        /// <summary>
        /// When enabled, the logged in user is synchronized with Evolution to simulate a single single-on experience
        /// </summary>
        bool EnableEvolutionUserSynchronization { get; }

        /// <summary>
        /// Called to retrieve information about the logged in user from the Evolution authentication synchronization cookie.
        /// </summary>
        /// <returns></returns>
        string GetEvolutionUserSynchronizationCookieValue();

        /// <summary>
        /// The name of the local user to sychronize with Evolution when EnableEvolutionUserSynchronization is enabled.  It is also the username used
        /// when EnableUserCreation is on
        /// </summary>
        string LocalUserName { get; }
    }


    public interface IUserCreatableOAuthClientConfiguration : IOAuthClientConfiguration
    {

        /// <summary>
        /// When enabled, the accessing user on the local host will be sychronized with the OAuth host, that is, the user will be created (if it doesn't already exist) and automatically logged on.  When enabled, the OAuth client should have the Client Credentials grant type enabled.
        /// </summary>
        bool EnableEvolutionUserCreation { get; }

        /// <summary>
        /// The name of an Evolution user used to create the Local user when EnableEvolutionUserCreation is enabled.
        /// </summary>
        string EvolutionUserCreationManagementUserName { get; }

        /// <summary>
        /// The name of the local user to sychronize with Evolution when EnableEvolutionUserSynchronization is enabled.  It is also the username used
        /// when EnableUserCreation is on
        /// </summary>
        string LocalUserName { get; }

        /// <summary>
        /// The email address of the local user to synchronize with Evolution when EnableEvolutionUserCreation is enabled.
        /// </summary>
        string LocalUserEmailAddress { get; }

        /// <summary>
        /// When EnableEvolutionUserCreation is enabled and a user is being created in Evolution, this property is accessed to retrieve additional user details (profile field data, etc)
        /// </summary>
        Dictionary<string, string> LocalUserDetails { get; }

        /// <summary>
        /// Called when creating an Evolution account fails when EnableEvolutionUserSynchronization is set to 'true'
        /// </summary>
        /// <param name="username">The username that was used during user creation</param>
        /// /// <param name="emailAddress">The email address that was used during user creation</param>
        void UserCreationFailed(string username, string emailAddress, IDictionary<string, string> userData, string message, ErrorResponse errorResponse);
    }
}
