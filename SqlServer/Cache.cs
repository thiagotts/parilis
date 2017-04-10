using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace SqlServer {
    public class Cache<T> {
        private MemoryCache cache;

        public Cache() {
            cache = MemoryCache.Default;
        }

        public virtual IEnumerable<T> GetKeysStartedWith(string filter) {
            var items = cache.Where(item => item.Key.StartsWith(filter))
                .ToList();

            if (items.Any()) {
                return items.Select(item => item.Value).Cast<T>();
            }

            return new List<T>();

        }

        public virtual void Add(string key, T item) {
            cache.Add(key, item, DateTimeOffset.MaxValue);
        }
    }
}