using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// 支持游标读取的环形缓冲区
    /// </summary>
    public class CursoredCircularBuffer<T>
    {
        private readonly T[] buffer;
        private readonly int capacity;
        private readonly object lockObj = new object();

        private long writePosition;  // 累计写入位置
        private long oldestPosition; // 最旧数据的位置

        public CursoredCircularBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("容量必须大于0", nameof(capacity));
            }

            this.capacity = capacity;
            buffer = new T[capacity];
            writePosition = 0;
            oldestPosition = 0;
        }

        /// <summary>
        /// 缓冲区容量
        /// </summary>
        public int Capacity => capacity;

        /// <summary>
        /// 当前数据数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (lockObj)
                {
                    return (int)Math.Min(writePosition - oldestPosition, capacity);
                }
            }
        }

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull
        {
            get
            {
                lock (lockObj)
                {
                    return writePosition - oldestPosition >= capacity;
                }
            }
        }

        /// <summary>
        /// 当前写入位置(用于创建游标)
        /// </summary>
        public long CurrentPosition
        {
            get
            {
                lock (lockObj)
                {
                    return writePosition;
                }
            }
        }

        /// <summary>
        /// 最旧数据位置
        /// </summary>
        public long OldestPosition
        {
            get
            {
                lock (lockObj)
                {
                    return oldestPosition;
                }
            }
        }

        /// <summary>
        /// 添加单个元素
        /// </summary>
        public void Add(T item)
        {
            lock (lockObj)
            {
                int index = (int)(writePosition % capacity);
                buffer[index] = item;
                writePosition++;

                // 如果超出容量，更新最旧位置
                if (writePosition - oldestPosition > capacity)
                {
                    oldestPosition = writePosition - capacity;
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
                    int index = (int)(writePosition % capacity);
                    buffer[index] = item;
                    writePosition++;
                }

                // 更新最旧位置
                if (writePosition - oldestPosition > capacity)
                {
                    oldestPosition = writePosition - capacity;
                }
            }
        }

        /// <summary>
        /// 按索引获取元素(相对于当前数据)
        /// </summary>
        public T this[int index]
        {
            get
            {
                lock (lockObj)
                {
                    int count = Count;
                    if (index < 0 || index >= count)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    long actualPosition = oldestPosition + index;
                    int bufferIndex = (int)(actualPosition % capacity);
                    return buffer[bufferIndex];
                }
            }
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        public T[] ToArray()
        {
            lock (lockObj)
            {
                int count = Count;
                if (count == 0)
                {
                    return new T[0];
                }

                T[] result = new T[count];
                for (int i = 0; i < count; i++)
                {
                    long actualPosition = oldestPosition + i;
                    int bufferIndex = (int)(actualPosition % capacity);
                    result[i] = buffer[bufferIndex];
                }

                return result;
            }
        }

        /// <summary>
        /// 从指定游标位置读取新数据
        /// </summary>
        /// <param name="cursor">读取游标(会被更新)</param>
        /// <returns>新增的数据</returns>
        public T[] ReadFrom(ref long cursor)
        {
            lock (lockObj)
            {
                // 如果游标太旧，跳到最旧的有效位置
                if (cursor < oldestPosition)
                {
                    cursor = oldestPosition;
                }

                // 如果游标超前，没有新数据
                if (cursor >= writePosition)
                {
                    return new T[0];
                }

                int count = (int)(writePosition - cursor);
                T[] result = new T[count];

                for (int i = 0; i < count; i++)
                {
                    long actualPosition = cursor + i;
                    int bufferIndex = (int)(actualPosition % capacity);
                    result[i] = buffer[bufferIndex];
                }

                // 更新游标
                cursor = writePosition;
                return result;
            }
        }

        /// <summary>
        /// 尝试读取新数据到目标数组
        /// </summary>
        /// <param name="cursor">读取游标</param>
        /// <param name="destination">目标数组</param>
        /// <param name="count">实际读取数量</param>
        /// <returns>是否有新数据</returns>
        public bool TryReadTo(ref long cursor, T[] destination, out int count)
        {
            lock (lockObj)
            {
                if (cursor < oldestPosition)
                {
                    cursor = oldestPosition;
                }

                if (cursor >= writePosition)
                {
                    count = 0;
                    return false;
                }

                int available = (int)(writePosition - cursor);
                count = Math.Min(available, destination.Length);

                for (int i = 0; i < count; i++)
                {
                    long actualPosition = cursor + i;
                    int bufferIndex = (int)(actualPosition % capacity);
                    destination[i] = buffer[bufferIndex];
                }

                cursor += count;
                return true;
            }
        }

        /// <summary>
        /// 获取最近N个元素
        /// </summary>
        public T[] GetLast(int count)
        {
            lock (lockObj)
            {
                int available = Count;
                if (count <= 0 || available == 0)
                {
                    return new T[0];
                }

                int actualCount = Math.Min(count, available);
                T[] result = new T[actualCount];

                long startPosition = writePosition - actualCount;
                for (int i = 0; i < actualCount; i++)
                {
                    int bufferIndex = (int)((startPosition + i) % capacity);
                    result[i] = buffer[bufferIndex];
                }

                return result;
            }
        }

        /// <summary>
        /// 尝试获取最后一个元素
        /// </summary>
        public bool TryGetLast(out T item)
        {
            lock (lockObj)
            {
                if (writePosition == oldestPosition)
                {
                    item = default(T);
                    return false;
                }

                int index = (int)((writePosition - 1) % capacity);
                item = buffer[index];
                return true;
            }
        }

        /// <summary>
        /// 创建一个新的读取游标(指向当前位置)
        /// </summary>
        public long CreateCursor()
        {
            lock (lockObj)
            {
                return writePosition;
            }
        }

        /// <summary>
        /// 创建一个从头开始的读取游标
        /// </summary>
        public long CreateCursorFromStart()
        {
            lock (lockObj)
            {
                return oldestPosition;
            }
        }

        /// <summary>
        /// 检查游标是否有效
        /// </summary>
        public bool IsCursorValid(long cursor)
        {
            lock (lockObj)
            {
                return cursor >= oldestPosition && cursor <= writePosition;
            }
        }

        /// <summary>
        /// 获取游标落后的数据数量
        /// </summary>
        public int GetLag(long cursor)
        {
            lock (lockObj)
            {
                if (cursor >= writePosition)
                {
                    return 0;
                }
                if (cursor < oldestPosition)
                {
                    return Count;
                }
                return (int)(writePosition - cursor);
            }
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            lock (lockObj)
            {
                Array.Clear(buffer, 0, capacity);
                writePosition = 0;
                oldestPosition = 0;
            }
        }
    }
}
