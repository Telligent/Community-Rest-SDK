using System;
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

	    private IRestCache _cache;
	    private string HostCacheKey = "communityServer:Oauth::Host::";
	    private IHostConfigurationManager _configuration;
		internal ConfigurationManagerService(IRestCache cache,IHostConfigurationManager configManager)
		{
		    _cache = cache;
		    _configuration = configManager;
		}

		#region IConfigurationManagerService Members

		public void Add(IOAuthClientConfiguration configuration)
		{
			_cache.Put(GetCacheKey(configuration.Name),configuration,300);
		}
        public void Remove(string name)
        {
            _cache.Remove(GetCacheKey(name));
        }
	    public IOAuthClientConfiguration Get(string name)
	    {
	        var host = _cache.Get(GetCacheKey(name));
	        return host as IOAuthClientConfiguration;
	    }

	    private string GetCacheKey(string name)
	    {
	        return string.Concat(HostCacheKey, name);
	    }

	    #endregion
	}
}
