using System;
using System.Collections.Generic;
using Telligent.Evolution.RestSDK.Implementations;

namespace Telligent.Evolution.RestSDK.Services
{
    public static class ServiceLocator
    {
		private static object _lockObject = new object();
		private static Dictionary<Type, object> _instances = null;

        public static T Get<T>()
        {
            EnsureInitialized();
			return (T)_instances[typeof(T)];
        }

        public static void EnsureInitialized()
        {
            if (_instances == null)
                lock (_lockObject)
					if (_instances == null)
					{
						var localInstances = new Dictionary<Type, object>();

                        var rest = new Telligent.Evolution.RestSDK.Implementations.Rest();
                        var deserializerService = new Deserializer();
                        var hostRegistrationService = new RestHostRegistrationService();
						localInstances[typeof(IRest)] = rest;
                        localInstances[typeof(IDeserializer)] = deserializerService;
                        localInstances[typeof(IRestHostRegistrationService)] = hostRegistrationService;
						_instances = localInstances;
					}
        }
    }
}
