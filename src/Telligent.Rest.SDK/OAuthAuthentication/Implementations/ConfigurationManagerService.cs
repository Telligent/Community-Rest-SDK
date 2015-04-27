using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.Extensions.OAuthAuthentication.Services;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Implementations
{
	internal class ConfigurationManagerService : IConfigurationManagerService
	{

	    private static readonly ConcurrentDictionary<string, IOAuthClientConfiguration> _hosts =
	        new ConcurrentDictionary<string, IOAuthClientConfiguration>();
		internal ConfigurationManagerService()
		{

		}

		#region IConfigurationManagerService Members

		public void Add(IOAuthClientConfiguration configuration)
		{
		    _hosts.TryAdd(configuration.Name, configuration);
		}
        public void Remove(string name)
        {
            IOAuthClientConfiguration client = null;
            _hosts.TryRemove(name, out client);
        }
	    public IOAuthClientConfiguration Get(string name)
	    {
	        IOAuthClientConfiguration client = null;
            _hosts.TryGetValue(name, out client);
	        return client;
	    }

	    #endregion
	}
}
