using System.Collections.Generic;

namespace BogusDataGenerator.Cache
{
    internal interface ICacheManager
    {
        /// <summary>
        /// Check whether the key is contained by the cache
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Returns true if the key is contained by the cache</returns>
        bool Contains(string key);

        /// <summary>
        /// Returns the number of items in the cache
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// Get the object that its key is given
        /// </summary>
        /// <typeparam name="T">The object</typeparam>
        /// <param name="key">The given key to check</param>
        /// <returns>returns the object or null if it doesn't exists</returns>
        T Get<T>(string key);

        /// <summary>
        /// Get the object that its key is given or set a new object if the key does not exist
        /// </summary>
        /// <typeparam name="T">The object</typeparam>
        /// <param name="key">The given key to check</param>
        /// <param name="value">value that set to cache manager</param>
        /// <returns>returns the object that is cached</returns>
        T GetOrSet<T>(string key, object value);

        IEnumerable<string> Keys();

        /// <summary>
        /// Removes the object that is referenced by the given key
        /// </summary>
        /// <param name="key">The given key</param>
        bool Remove(string key);

        bool Remove(string key, out object value);

        /// <summary>
        /// Add a new object into the cache
        /// </summary>
        /// <param name="key">The key of the object to add</param>
        /// <param name="value">The value of the object to add</param>
        void Set(string key, object value);

        IEnumerable<object> Values();
    }
}
