using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BogusDataGenerator.Extensions
{
    public class CacheManager : ICacheManager
    {
        private readonly ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

        public static CacheManager Instance { get; } = new CacheManager();

        static CacheManager() { }

        private CacheManager() { }

        public bool Contains(string key) => cache.ContainsKey(key);

        public int Count() => cache.Count;

        public T Get<T>(string key) => cache.ContainsKey(key) ? (T)cache[key] : default(T);

        public T GetOrSet<T>(string key, object value)
        {
            if (cache.ContainsKey(key))
            {
                return Get<T>(key);
            }
            else
            {
                Set(key, value);
                return (T)value;
            }
        }

        public IEnumerable<string> Keys() => cache.Keys;

        public void Modify(string key, object value)
        {
            if (key == null || string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Value can not be null or empty.");
            if (value == null || value.Equals(""))
                throw new ArgumentNullException(nameof(value), "Value can not be null or empty.");

            if (Remove(key))
            {
                Set(key, value);
            }
            else
                throw new ArgumentNullException(nameof(key), "Unable to delete the object.");
        }

        public bool Remove(string key)
        {
            object output;
            return cache.TryRemove(key, out output);
        }

        public bool Remove(string key, out object value)
        {
            return cache.TryRemove(key, out value);
        }

        public void Set(string key, object value)
        {
            cache.AddOrUpdate(key, value, (newKey, newValue) => value);
        }

        public IEnumerable<object> Values() => cache.Values;

        public Dictionary<string, object> CacheData()
        {
            var dic = new Dictionary<string, object>();
            foreach (var item in cache)
            {
                dic.Add(item.Key, item.Value);
            }
            return dic;
        }

        public Dictionary<string, T> CacheData<T>()
        {
            var dic = new Dictionary<string, T>();
            foreach (var item in cache)
            {
                dic.Add(item.Key, (T)item.Value);
            }
            return dic;
        }
    }
}
