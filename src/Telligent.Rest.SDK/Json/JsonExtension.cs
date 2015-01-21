using System;

namespace Telligent.Evolution.RestSDK.Extensions
{
    public static class JsonExtension
    {
        public static char[] SubArray(this char[] array, int start, int end)
        {
            var length = end - start;
            if (length < 1) return new char[0];

            length++;

            var result = new char[length];
            Array.Copy(array, start, result, 0, length);
            return result;
        }
    }
}
