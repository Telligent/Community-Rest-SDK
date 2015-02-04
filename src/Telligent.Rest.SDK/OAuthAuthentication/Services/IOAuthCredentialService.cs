using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;


namespace Telligent.Evolution.Extensions.OAuthAuthentication.Services
{
	internal interface IOAuthCredentialService
	{
		User GetUser(IOAuthClientConfiguration configuration, NameValueCollection redirectState, Action<Uri> redirect);
		Uri GetLoginUrl(IOAuthClientConfiguration configuration, NameValueCollection state);
		void UserLoggedIn(IOAuthClientConfiguration configuration, string authorizationCode, NameValueCollection state); 
		void Logout(IOAuthClientConfiguration configuration);
		Uri GetAuthenticatedRedirectUrl(IOAuthClientConfiguration configuration, string evolutionUrl);
		Uri GetEvolutionLogOutUrl(IOAuthClientConfiguration configuration, NameValueCollection state);
	}
}
