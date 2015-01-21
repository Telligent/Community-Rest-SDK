using System.Collections.Generic;
using System.Dynamic;
using Telligent.Evolution.RestSDK.Json;
using Telligent.Evolution.RestSDK.Services;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class Deserializer : IDeserializer
    {
        public void Deserialize(dynamic element, JsonReader reader)
        {
            while (reader.Read())
            {
                switch (reader.Type)
                {
                    case JsonObject.JsonType.Object:
                        AddObject(element, reader);
                        break;
                    case JsonObject.JsonType.Array:
                        AddArray(element, reader);
                        break;
                    default:
                        AddProperty(element, reader.Value, reader.Property);
                        break;
                }
            }
        }

        private void AddArray(dynamic element, JsonReader reader)
        {
            var dynamicList = new List<dynamic>();
            var list = reader.Value as IEnumerable<string>;

            if (list == null) return;

            foreach (var item in list)
            {
                dynamic arrayElement = new ExpandoObject();

                var itemChars = item.ToCharArray();
                var itemType = JsonObject.GetTypeFromValue(item);

                switch (itemType)
                {
                    case JsonObject.JsonType.String:
                        AddProperty(arrayElement, JsonParser.ReadAsString(itemChars), reader.Property);
                        break;
                    case JsonObject.JsonType.Number:
                        AddProperty(arrayElement, JsonParser.ReadAsNumber(itemChars), reader.Property);
                        break;
                    case JsonObject.JsonType.Boolean:
                        AddProperty(arrayElement, JsonParser.ReadAsBoolean(itemChars), reader.Property);
                        break;
                    case JsonObject.JsonType.Object:
                        Deserialize(arrayElement, new JsonReader(item));
                        break;
                    case JsonObject.JsonType.Array:
                        AddArray(arrayElement, new JsonReader(item)
                        {
                            Property = reader.Property,
                            Value = JsonParser.ReadAsArray(itemChars),
                            Text = item,
                            Type = JsonObject.JsonType.Array
                        });
                        break;
                }

                AddProperty(dynamicList, arrayElement);
            }

            AddProperty(element, dynamicList, reader.Property);
        }

        private void AddObject(dynamic element, JsonReader reader)
        {
            dynamic dynamicElement = new ExpandoObject();

            Deserialize(dynamicElement, new JsonReader(reader.Value.ToString()));

            AddProperty(element, dynamicElement, reader.Property);
        }

        private void AddProperty(dynamic element, object value, string name = null)
        {
            if (element is List<dynamic>)
            {
                (element as List<dynamic>).Add(value);
            }
            else
            {
                var dictionary = element as IDictionary<string, object>;
                if (dictionary != null) dictionary[name ?? "_"] = value;
            }
        }
    }
}
