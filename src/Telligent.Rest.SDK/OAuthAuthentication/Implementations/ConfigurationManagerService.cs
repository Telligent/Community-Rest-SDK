using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;

using Telligent.Evolution.Extensions.OAuthAuthentication.Services;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Implementations
{
	internal class ConfigurationManagerService : IConfigurationManagerService
	{
		Dictionary<Guid, IOAuthClientConfiguration> _configurations;

		internal ConfigurationManagerService()
		{
			_configurations = new Dictionary<Guid, IOAuthClientConfiguration>();
		}

		#region IConfigurationManagerService Members

		public void Add(IOAuthClientConfiguration configuration)
		{
			if (configuration != null)
				_configurations[configuration.Id] = configuration;
		}

		public IOAuthClientConfiguration Get(Guid id)
		{
			IOAuthClientConfiguration configuration;
			if (_configurations.TryGetValue(id, out configuration))
				return configuration;

			return null;
		}

		public void Remove(Guid id)
		{
			_configurations.Remove(id);
		}

		#endregion
	}
}
