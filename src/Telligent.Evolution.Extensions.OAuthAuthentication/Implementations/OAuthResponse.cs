using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Implementations
{
	public class OAuthResponse
	{
		public string error { get; set; }
		public string access_token { get; set; }
		public int expires_in { get; set; }
		public string refresh_token { get; set; }
	}
}
