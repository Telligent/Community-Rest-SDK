using System;

namespace Telligent.Evolution.RestSDK.Exceptions
{
    public class JsonBadFormat : Exception
    {
        public JsonBadFormat() { }
        public JsonBadFormat(string message) : base(message) { }
    }
}
