using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensions.OAuthAuthentication.Implementations;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Services
{
	internal static class ServiceManager
	{
		private static object _lockObject = new object();
		private static Dictionary<Type, object> _instances = null;

		internal static T Get<T>()
		{
			EnsureInitialized();
			return (T)_instances[typeof(T)];
		}

		internal static void EnsureInitialized()
		{
			if (_instances == null)
				lock (_lockObject)
					if (_instances == null)
					{
						var localInstances = new Dictionary<Type, object>();

						#region Service Bindings

						var userSyncService = new UserSyncService();
						localInstances[typeof(IUserSyncService)] = userSyncService;
						localInstances[typeof(IOAuthCredentialService)] = new OAuthCredentialService(userSyncService);
						localInstances[typeof(IDefaultOAuthUserService)] = new DefaultOAuthUserService();
						localInstances[typeof(IConfigurationManagerService)] = new ConfigurationManagerService();
						
						#endregion

						_instances = localInstances;
					}
		}
	}
}

