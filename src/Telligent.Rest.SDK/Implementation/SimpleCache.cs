using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Telligent.Evolution.RestSDK.Implementations
{
    public class SimpleCache : Telligent.Evolution.Extensibility.Rest.Version1.IRestCache
    {
        MemoryCache _cache;

        public SimpleCache()
        {
            _cache = new MemoryCache("Telligent.Evolution.RestSDK");
        }

        public void Put(string key, object value, int cacheDurationSeconds)
        {
            _cache.Add(key, value, DateTime.Now.AddSeconds(cacheDurationSeconds));
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public object Get(string key)
        {
            return _cache.Get(key);
        }

        public void Clear()
        {
            var list = new List<KeyValuePair<string, object>>(_cache);
            foreach (var item in list)
            {
                _cache.Remove(item.Key);    
            }
        }
    }
}
