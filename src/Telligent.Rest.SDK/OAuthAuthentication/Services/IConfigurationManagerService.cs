using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Services
{
	internal interface IConfigurationManagerService
	{
		void Add(IOAuthClientConfiguration configuration);
		IOAuthClientConfiguration Get(string name);
	    void Remove(string name);
	}
}
