using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Implementations;
using Telligent.Rest.SDK.Implementation;
using Telligent.Rest.SDK.Model;

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

        public static void EnsureInitialized(Dictionary<Type, object> instances = null)
        {
            if (instances != null)
            {
                lock (_lockObject)
                {
                    _instances = instances;
                }
            }
            else
            {
                if (_instances == null)
                    lock (_lockObject)
                        if (_instances == null)
                        {
                            HttpContextBase context = null;
                            if(HttpContext.Current != null)
                                context = new HttpContextWrapper(HttpContext.Current);

                            
                            var cache = new SimpleCache();
                            IConfigurationFile file;

                            if (context == null || System.Configuration.ConfigurationManager.AppSettings["communityServer:SDK:configPath"] != null)
                                file = new FileSystemConfigurationFile();
                            else
                                file = new WebConfigurationFile(context);

                            var configManager = new HostConfigurationManager(cache, file);

                            var localInstances = new Dictionary<Type, object>();
                         
                            var proxy = new RestCommunicationProxy();

                            var rest = new Telligent.Evolution.RestSDK.Implementations.Rest(proxy);
                            var deserializerService = new Deserializer();
                            var hostRegistrationService = new RestHostRegistrationService();
                            localInstances[typeof(IRest)] = rest;
                            localInstances[typeof (IRestCache)] = cache;
                            localInstances[typeof(IDeserializer)] = deserializerService;
                            localInstances[typeof(IRestCommunicationProxy)] = proxy;
                            localInstances[typeof(IRestHostRegistrationService)] = hostRegistrationService;
                            localInstances[typeof(IConfigurationFile)] = file;
                            localInstances[typeof(IHostConfigurationManager)] = configManager;
                            _instances = localInstances;
                        }
            }
           
        }
    }
}
