using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Telligent.Evolution.Extensions.OAuthAuthentication.Services;
using Telligent.Evolution.RestSDK.Services;

namespace Telligent.Evolution.Extensibility.OAuthClient.Version1
{
	public class OAuthClientHttpHandler : IHttpHandler
	{
		#region IHttpHandler Members

		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var state = HttpUtility.ParseQueryString(context.Request.QueryString[Constants.StateQueryStringKey] ?? string.Empty);
			IOAuthClientConfiguration configuration = null;
			Guid configurationId;
			if (!string.IsNullOrEmpty(state[Constants.ConfigurationIdQueryStringKey]))
			{
                configuration = ServiceLocator.Get<IConfigurationManagerService>().Get(state[Constants.ConfigurationIdQueryStringKey]);
				if (configuration == null) 
				{
					context.Response.StatusCode = 500;
					context.Response.StatusDescription = "The OAuth configuration specified does not exist.";
					return;
				}
			}
			else
			{
				context.Response.StatusCode = 500;
				context.Response.StatusDescription = "The OAuth configuration identification could not be retrieved.";
				return;
			}


			if (context.Request.QueryString[Constants.AuthorizationCodeQueryStringKey] != null)
			{
				ServiceLocator.Get<IOAuthCredentialService>().UserLoggedIn(configuration, context.Request.QueryString[Constants.AuthorizationCodeQueryStringKey], state);
			}
			else if (context.Request.QueryString[Constants.LoggedOutQueryStringKey] != null)
			{
				if (context.Request.QueryString[Constants.LoggedOutQueryStringKey] == "true")
					configuration.UserLoggedOut(state);
				else
					configuration.UserLogOutFailed(state);
			}
			else if (context.Request.QueryString[Constants.ErrorQueryStringKey] != null)
			{
				configuration.UserLoginFailed(state);
			}
			else
			{
				// Default behavior?
				configuration.UserLoginFailed(state);
			}
		}

		#endregion
	}
}
