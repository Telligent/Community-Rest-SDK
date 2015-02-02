using System.Configuration;
using System.IO;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class FileSystemConfigurationFile:IConfigurationFile
    {

        public string GetConfigurationData()
        {
            var configFile = System.Configuration.ConfigurationManager.AppSettings["communityServer:SDK:configPath"];
            if (string.IsNullOrWhiteSpace(configFile))
                throw new ConfigurationErrorsException("Cannot find a valid configuration path in the application configuration app settings.  Add the full disk path to key communityServer:SDK:configPath");

            if (!File.Exists(configFile))
                throw new ConfigurationErrorsException("Cannot find valid config file at'" + configFile + "'");

            using (FileStream stream = new FileStream(configFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}