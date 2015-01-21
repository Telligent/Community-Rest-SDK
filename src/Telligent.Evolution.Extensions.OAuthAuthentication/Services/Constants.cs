using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Services
{
	internal static class Constants
	{
		internal const int RefreshMarginMinutes = 3;
		internal const int RequestTimeoutMilliseconds = 30000;
		internal const string ConfigurationIdQueryStringKey = "__cid";
		internal const string StateQueryStringKey = "state";
		internal const string AuthorizationCodeQueryStringKey = "code";
		internal const string ErrorQueryStringKey = "error";
		internal const string LoggedOutQueryStringKey = "logged_out";
		internal const string SynchronizedUserNameQueryStringKey = "__sun";
	}
}
