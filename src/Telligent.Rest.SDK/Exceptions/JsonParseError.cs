using System;

namespace Telligent.Evolution.RestSDK.Exceptions
{
    public class JsonParseError : Exception
    {
        public JsonParseError() { }
        public JsonParseError(string message) : base(message) { }
        public JsonParseError(string message, Exception innterException) : base(message, innterException) { }
    }
}
