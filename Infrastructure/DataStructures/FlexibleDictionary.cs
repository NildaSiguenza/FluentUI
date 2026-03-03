using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{

    public static class DictionaryFactory
    {
        public static FlexibleDictionary<T1, T2> CreateDictionary<T1, T2>()
        {
            return new FlexibleDictionary<T1, T2>();
        }

        public static FlexibleConcurrentDictionary<T1, T2> CreateConcurrentDictionary<T1, T2>()
        {
            return new FlexibleConcurrentDictionary<T1, T2>();
        }
    }

    public class FlexibleDictionary<T1, T2>
    {

        private readonly Dictionary<T1, T2> dict = new Dictionary<T1, T2>();


        public ICollection<T1> Keys => dict.Keys;

        public ICollection<T2> Values => dict.Values;



        public bool ContainsKey(T1 key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        public void Add(T1 key, T2 value)
        {
            dict.Add(key, value);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public bool TryGetValue(T1 key, out T2 value)
        {
            return dict.TryGetValue(key, out value);
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        public bool Remove(T1 key)
        {
            return dict.Remove(key);
        }

        /// <summary>
        /// 更新键值对
        /// </summary>
        public bool TryUpdate(T1 key, T2 newValue)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = newValue;
                return true;
            }
            return false;
        }


        public void Clear()
        {
            dict.Clear();
        }

        /// <summary>
        /// 获取所有键值对
        /// </summary>
        public IReadOnlyDictionary<T1, T2> GetAllItems()
        {
            return new Dictionary<T1, T2>(dict);
        }

    }


    public class FlexibleConcurrentDictionary<T1, T2>
    {

        private readonly ConcurrentDictionary<T1, T2> dict = new ConcurrentDictionary<T1, T2>();


        public ICollection<T1> Keys => dict.Keys;

        public ICollection<T2> Values => dict.Values;



        public bool ContainsKey(T1 key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        public bool TryAdd(T1 key, T2 value)
        {
            return dict.TryAdd(key, value);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public bool TryGetValue(T1 key, out T2 value)
        {
            return dict.TryGetValue(key, out value);
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        public bool TryRemove(T1 key, out T2 value)
        {
            return dict.TryRemove(key, out value);
        }

        /// <summary>
        /// 更新键值对
        /// </summary>
        public bool TryUpdate(T1 key, T2 newValue)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = newValue;
                return true;
            }
            return false;
        }

        public void Clear()
        {
            dict.Clear();
        }

        /// <summary>
        /// 获取所有键值对
        /// </summary>
        public IReadOnlyDictionary<T1, T2> GetAllItems()
        {
            return new Dictionary<T1, T2>(dict);
        }

    }

}
