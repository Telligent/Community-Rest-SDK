using System.Configuration;
using System.IO;
using System.Web;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class WebConfigurationFile : IConfigurationFile
    {
        private HttpContextBase _context;
        public WebConfigurationFile(HttpContextBase context)
        {
            _context = context;
        }

        public string GetConfigurationData()
        {
            var path = _context.Server.MapPath("~/");
            var configFile = Path.Combine(path, "communityServer_SDK.config");
            if(!File.Exists(configFile))
                throw new ConfigurationErrorsException("Cannot find valid config file in web root");

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