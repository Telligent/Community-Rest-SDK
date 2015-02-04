
namespace Telligent.Evolution.RestSDK.Json
{
    public class JsonObject
    {
        public enum JsonType { Object, Array, String, Number, Boolean, Null, Date, Guid, UnKnown }

        private JsonType _type;

        public JsonObject()
        {
            _type = JsonType.UnKnown;
        }

        public string Property { get; set; }
        public object Value { get; set; }
        public string Text { get; set; }
        public JsonType Type
        {
            get { return _type; }
            set { if (_type == JsonType.UnKnown) _type = value; }
        }

        public void Reset()
        {
            _type = JsonType.UnKnown;

            Text = string.Empty;
            Property = null;
            Value = null;
        }

        public static JsonType GetTypeFromValue(string value)
        {
            if (value == null) return JsonType.UnKnown;
            if (value.Length == 0) return JsonType.UnKnown;

            var valueArray = value.ToCharArray();
            var firstChar = valueArray[0];

            switch (firstChar)
            {
                case '[':
                    return JsonObject.JsonType.Array;
                case '{':
                    return JsonObject.JsonType.Object;
                case '"':
                    return JsonObject.JsonType.String;
            }

            if (char.IsNumber(firstChar)) return JsonObject.JsonType.Number;
            if (firstChar == 't' || firstChar == 'f') return JsonObject.JsonType.Boolean;
            if (firstChar == 'n') return JsonObject.JsonType.Null;

            return JsonType.UnKnown;
        }
    }
}
