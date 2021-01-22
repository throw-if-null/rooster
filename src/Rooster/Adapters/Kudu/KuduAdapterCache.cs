using System;
using System.Collections.Concurrent;

namespace Rooster.Adapters.Kudu
{
    public class KuduApiAdapterCache
    {
        private readonly ConcurrentDictionary<string, IKuduApiAdapter> _cache;

        public KuduApiAdapterCache(ConcurrentDictionary<string, IKuduApiAdapter> cache)
        {
            _cache = cache;
        }

        public IKuduApiAdapter Get(string name)
        {
            return
                _cache[name] ??
                throw new ArgumentOutOfRangeException($"No Kudu Api adapter have been registered for name: {name}");
        }
    }
}
