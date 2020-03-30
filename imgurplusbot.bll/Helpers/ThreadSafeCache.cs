using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot.Types.Enums;

namespace imgurplusbot.bll.Helpers
{
    /// <summary>
    /// This is a generic cache that is thread safe and uses a read/write lock access for performance.
    /// The cache itself is a string key based dictionary.
    /// </summary>
    /// <typeparam name="T">The type that we want to keep in the cache</typeparam>
    public class ThreadSafeCache<T>
    {
        protected ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(); // mutex 
        protected List<ValueTuple<ValueTuple<string, MessageType[]>, T>> innerCache = new List<ValueTuple<ValueTuple<string, MessageType[]>, T>>();  // the cache itself

        // This method will replace existing item or add if it does not already exist
        public void Add(string key1, MessageType[] key2, T val)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Add(((key1, key2), val));
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        // This is to get an item from the cache by its key, it will return null if not found.

        public IEnumerable<T> this[string key1]
        {
            get
            {
                cacheLock.EnterReadLock();
                try
                {
                    
                    return innerCache.Where((el) => el.Item1.Item1 == key1).Select((el) => el.Item2);
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }
            }
        }

        public IEnumerable<T> this[MessageType key2]
        {
            get
            {
                cacheLock.EnterReadLock();
                try
                {

                    return innerCache.Where((el) => el.Item1.Item2.Contains(key2)).Select((el) => el.Item2);
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }
            }
        }

        // This method is to remove an item from the cache by its key
        public void Delete(string key1, MessageType[] key2)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Remove(innerCache.FirstOrDefault((el) => el.Item1 == (key1, key2)));
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        // This method empty the whole cache.
        public void Purge()
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Clear();
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        // This method validates that a key exists in the cache.
        public bool Exist(string key1, MessageType[] key2)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache.Any((el) => el.Item1 == (key1, key2));
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        // This methods gets the number of items saved in the cache.
        public int Size()
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache.Count;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

    }
}
