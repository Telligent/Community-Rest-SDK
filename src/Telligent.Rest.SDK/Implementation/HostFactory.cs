using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.Rest.SDK.Implementation
{
    internal static class HostFactory
    {

        private static readonly ConcurrentDictionary<string, Host> _hosts =
            new ConcurrentDictionary<string, Host>();

        public static Host Get(string name)
        {
            Host host = null;
            _hosts.TryGetValue(name, out host);
            return host;
        }

        public static void Add(Host host, bool force= true)
        {
            if (force)
            {
                Remove(host);
            }
            _hosts.TryAdd(host.Name, host);
        }

        public static void Remove(Host host)
        {
            Host removed = null;
            _hosts.TryRemove(host.Name, out removed);
        }
    }
}
