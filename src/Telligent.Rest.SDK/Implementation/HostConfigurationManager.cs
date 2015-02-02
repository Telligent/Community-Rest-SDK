using System.Configuration;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class HostConfigurationManager : IHostConfigurationManager
    {
        private IRestCache _cache;
        private IConfigurationFile _file;
        private static readonly string _fileDataKey = "communityServer::configData";
        private static readonly string _hostCacheKey = "communityServer::SDK::host::";
        public HostConfigurationManager(IRestCache cache,IConfigurationFile configFile)
        {
            _cache = cache;
            _file = configFile;
        }
        public HostConfiguration GetOptions(string name)
        {

            var config = _cache.Get(GetCacheKey(name));
            if (config != null)
                return (HostConfiguration)config;

            var fileConfig = Read(name);
            if (fileConfig == null)
                throw new ConfigurationErrorsException("No host entry defined in configuraton for '" + name + "'");

            _cache.Put(GetCacheKey(name), fileConfig, 300);

            return fileConfig;

        }



        private HostConfiguration Read(string name)
        {
            var configData = GetConfigData();
            if (string.IsNullOrEmpty(configData))
                throw new ConfigurationErrorsException("Invalid configuration data");

            HostConfiguration config = new HostConfiguration();
            return config;
        }

        private string GetConfigData()
        {
            var configData = _cache.Get(_fileDataKey);
            if (configData != null)
                return configData.ToString();

            configData = _file.GetConfigurationData();
            if (string.IsNullOrEmpty(configData.ToString()))
                throw new ConfigurationErrorsException("Invalid configuration data");
            
            _cache.Put(_fileDataKey,configData,300);
            return configData.ToString();
        }
        private string GetCacheKey(string name)
        {
            return string.Concat(_hostCacheKey, name);
        }
        
    }
}