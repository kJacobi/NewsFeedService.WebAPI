using Microsoft.Extensions.Caching.Memory;
using NewsFeedService.WebAPI.Data;
using Newtonsoft.Json.Linq;

namespace NewsFeedService.WebAPI.Services
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HashSet<int> _keys = new HashSet<int>();

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public HashSet<int> GetAllKeys()
        {
            return _keys;
        }

        public T GetFromCache<T>(int key)
        {            
            if (_memoryCache.TryGetValue(key, out T value))
            {
                return value;
            }
            return default;
        }

        public void AddToCache<T>(int key, T value)
        {
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(60));
            
            if (!_keys.Contains(key))
            {
                _keys.Add(key);
            }
        }

        public void RemoveFromCache(int key)
        {
            _memoryCache.Remove(key);
            if (_keys.Contains(key))
            {
                _keys.Remove(key);
            }
        }
    }

    public interface IMemoryCacheService
    {
        HashSet<int> GetAllKeys();
        T GetFromCache<T>(int key);
        void AddToCache<T>(int key, T value);
        void RemoveFromCache(int key);
    }
}
