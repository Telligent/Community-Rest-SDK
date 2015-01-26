using Telligent.Evolution.RestSDK.Json;

namespace Telligent.Evolution.RestSDK.Services
{
    public interface IDeserializer
    {
        void Deserialize(dynamic element, JsonReader reader);
    }
}
