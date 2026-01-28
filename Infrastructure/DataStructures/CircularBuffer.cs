using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// 环形缓冲区
    /// 特点: 当缓冲区满时，新数据会覆盖最旧的数据
    /// (固定大小,FIFO,复杂度O(1))
    /// </summary>
    public class CircularBuffer<T>
    {
        private readonly T[] buffer;
        private int start;      // 缓冲区起始位置
        private int count;      // 当前元素数量
        private readonly int capacity;  // 缓冲区容量
        private readonly object lockObj = new object();

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("容量必须大于0", nameof(capacity));
            }

            buffer = new T[capacity];
            this.capacity = capacity;
            start = 0;
            count = 0;
        }

        public int Count
        {
            get
            {
                lock (lockObj)
                {
                    return count;
                }
            }
        }

        public int Capacity => capacity;

        public bool IsFull
        {
            get
            {
                lock (lockObj)
                {
                    return count == capacity;
                }
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T item)
        {
            lock (lockObj)
            {
                if (count < capacity)
                {
                    // 缓冲区未满，直接添加
                    buffer[(start + count) % capacity] = item;
                    count++;
                }
                else
                {
                    // 缓冲区已满，覆盖最旧的数据
                    buffer[start] = item;
                    start = (start + 1) % capacity;
                }
            }
        }

        /// <summary>
        /// 批量添加元素
        /// </summary>
        public void AddRange(T[] items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            lock (lockObj)
            {
                foreach (var item in items)
                {
                    if (count < capacity)
                    {
                        buffer[(start + count) % capacity] = item;
                        count++;
                    }
                    else
                    {
                        buffer[start] = item;
                        start = (start + 1) % capacity;
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定位置的元素
        /// </summary>
        public T this[int index]
        {
            get
            {
                lock (lockObj)
                {
                    if (index < 0 || index >= count)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return buffer[(start + index) % capacity];
                }
            }
        }

        /// <summary>
        /// 获取当前所有元素
        /// </summary>
        public T[] ToArray()
        {
            lock (lockObj)
            {
                T[] result = new T[count];
                if (count == 0)
                {
                    return result;
                }

                if (start + count <= capacity)
                {
                    Array.Copy(buffer, start, result, 0, count);
                }
                else
                {
                    int firstPart = capacity - start;
                    Array.Copy(buffer, start, result, 0, firstPart);
                    Array.Copy(buffer, 0, result, firstPart, count - firstPart);
                }
                return result;
            }
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            lock (lockObj)
            {
                start = 0;
                count = 0;
                Array.Clear(buffer, 0, capacity);
            }
        }
    }
}
