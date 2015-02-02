using Telligent.Evolution.RestSDK.Json;

namespace Telligent.Rest.SDK.Model
{
    public interface IDeserializer
    {
        void Deserialize(dynamic element, JsonReader reader);
    }
}
