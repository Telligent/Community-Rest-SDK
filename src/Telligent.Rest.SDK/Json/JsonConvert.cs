using System.Dynamic;
using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.RestSDK.Json
{
    public class JsonConvert
    {
        public static dynamic Deserialize(string json)
        {
            var deserializer = ServiceLocator.Get<IDeserializer>();

            dynamic result = new ExpandoObject();

            deserializer.Deserialize(result, new JsonReader(json));

            return result;
        }
    }
}
