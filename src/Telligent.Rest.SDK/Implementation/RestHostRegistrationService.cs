using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using Telligent.Evolution.Extensibility.Rest.Version1;


using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class RestHostRegistrationService : IRestHostRegistrationService
    {
		Dictionary<Guid, RestHost> _hosts = new Dictionary<Guid, RestHost>();

	
		public void Register(RestHost host)
		{
			if (host != null)
				_hosts[host.Id] = host;
		}

        public RestHost Get(Guid id)
		{
            RestHost host;
			if (_hosts.TryGetValue(id, out host))
				return host;

			return null;
		}

		public void Remove(Guid id)
		{
			_hosts.Remove(id);
		}

		
	}
}
