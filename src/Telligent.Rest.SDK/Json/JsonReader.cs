using System;
using System.Collections.Generic;
using Telligent.Evolution.RestSDK.Exceptions;
using Telligent.Evolution.RestSDK.Extensions;

namespace Telligent.Evolution.RestSDK.Json
{
    public class JsonReader : JsonObject
    {
        private enum ReaderState { Begin, Property, Value, End }

        private readonly char[] _json;
        private int _position;
        private ReaderState _state;

        public JsonReader(string json)
        {
            _json = json.ToCharArray();
            _position = 0;
            _state = ReaderState.Begin;
        }

        public bool Read()
        {
            if (_state == ReaderState.Begin && _position >= _json.Length) return false;

            Reset();

            Validate();

            if (_state == ReaderState.Property) ReadProperty();
            if (_state == ReaderState.Value) ReadValue();

            return (_state == ReaderState.Begin);
        }

        private void Validate()
        {
            var hasOpenBrace = false;

            foreach (var token in _json)
            {
                if (char.IsWhiteSpace(token)) continue;

                if (token == JsonChar.OpenBracket)
                {
                    _state = ReaderState.Value;
                    return;
                }

                if (hasOpenBrace) return;

                if (token == JsonChar.OpenBrace)
                {
                    hasOpenBrace = true;
                    _state = ReaderState.Property;
                }
            }

            throw new JsonBadFormat("Json contains invalid format.");
        }

        private void ReadProperty()
        {
            var start = ReadTo(JsonChar.DoubleQuote);

            if (start == -1) return;

            var end = ReadTo(JsonChar.PropertySeparator);

            Property = JsonParser.ReadAsString(_json.SubArray(start, end));

            _state = ReaderState.Value;
            _position++;
        }

        private void ReadValue()
        {
            var start = ReadTo(JsonChar.Empty, char.IsWhiteSpace);
            var end = ReadTo(JsonChar.Empty, t => !IsCloser(t), true);
            var partialArray = _json.SubArray(start, end);

            Text = new string(partialArray);

            switch (Type)
            {
                case JsonType.Object:
                    Value = JsonParser.ReadAsObject(partialArray);
                    break;
                case JsonType.Array:
                    Value = JsonParser.ReadAsArray(partialArray);
                    break;
                case JsonType.String:
                    Value = JsonParser.ReadAsString(partialArray);
                    break;
                case JsonType.Number:
                    Value = JsonParser.ReadAsNumber(partialArray);
                    break;
                case JsonType.Boolean:
                    Value = JsonParser.ReadAsBoolean(partialArray);
                    break;
                default:
                    Value = null;
                    break;
            }

            _state = ReaderState.Begin;
            _position++;
        }

        private int ReadTo(char value, Func<char, bool> next = null, bool trackEncapsulation = false)
        {
            var cap = new Stack<char>();
            var complementryValue = GetComplement(value);
            var start = _position;

            for (; _position < _json.Length; _position++)
            {
                var token = _json[_position];
                var previous = _position > 0 ? _json[_position - 1] : JsonChar.Empty;

                if (trackEncapsulation && complementryValue == JsonChar.Empty && IsOpener(token))
                {
                    value = token;
                    complementryValue = GetComplement(value);

                    if (value == complementryValue) continue;
                }
                if (trackEncapsulation && token == value && previous != JsonChar.Escape) cap.Push(token);
                if (next == null && token != value) continue;
                if (next != null && next(token)) continue;

                if (trackEncapsulation)
                {
                    if ((complementryValue != JsonChar.Empty && token != complementryValue) || previous == JsonChar.Escape) continue;
                    if (cap.Count > 0) cap.Pop();
                    if (cap.Count == 0) return _position;
                }
                else
                {
                    return _position;
                }
            }

            if (_state == ReaderState.Property && value == JsonChar.DoubleQuote)
            {
                _state = ReaderState.End;
            }
            else
            {
                throw new JsonParseError(string.Format("ReadTo could not find value ({0}) started at ({1}).", value, start));
            }

            return -1;
        }

        private char GetComplement(char value)
        {
            switch (value)
            {
                case JsonChar.OpenBracket:  return JsonChar.CloseBracket;
                case JsonChar.CloseBracket: return JsonChar.OpenBracket;
                case JsonChar.OpenBrace:    return JsonChar.CloseBrace;
                case JsonChar.CloseBrace:   return JsonChar.OpenBrace;
                default: return value;
            }
        }

        private bool IsOpener(char value)
        {
            value = char.ToLowerInvariant(value);

            switch (value)
            {
                case JsonChar.OpenBracket:
                    Type = JsonType.Array;
                    return true;
                case JsonChar.OpenBrace:
                    Type = JsonType.Object;
                    return true;
                case JsonChar.DoubleQuote:
                    Type = JsonType.String;
                    return true;
            }

            if (char.IsNumber(value)) Type = JsonType.Number;
            if (value == JsonChar.True || value == JsonChar.False) Type = JsonType.Boolean;
            if (value == JsonChar.Null) Type = JsonType.Null;

            return false;
        }

        private bool IsCloser(char value)
        {
            switch (value)
            {
                case JsonChar.CloseBracket:
                case JsonChar.CloseBrace:
                case JsonChar.Delimitter:
                case JsonChar.DoubleQuote:
                    return true;
                default: return false;
            }
        }
    }
}
