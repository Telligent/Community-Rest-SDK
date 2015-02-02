using Telligent.Evolution.RestSDK.Implementations;

namespace Telligent.Rest.SDK.Model
{
    public interface IHostConfigurationManager
    {
        HostConfiguration GetOptions(string name);
    }
}