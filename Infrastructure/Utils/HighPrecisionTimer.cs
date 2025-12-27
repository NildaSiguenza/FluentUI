using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// 高精度定时器
    /// </summary>
    public class HighPrecisionTimer : IDisposable
    {
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(uint period);

        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(uint period);

        private readonly Action callback;
        private readonly Action<TimerStatistics> callbackWithStats;
        private readonly double intervalMs;
        private readonly bool useHighResolution;

        private long? targetTickCount; // null表示无限循环
        private Action onCompleted; // 完成时的回调

        private Thread timerThread;
        private volatile bool isRunning;
        private volatile bool isPaused;
        private bool disposed;

        private TimerStatistics statistics;
        private readonly object lockObject = new object();

        #region 构造函数

        /// <summary>
        /// 创建高精度定时器
        /// </summary>
        /// <param name="intervalMs">间隔时间(毫秒)</param>
        /// <param name="callback">回调函数</param>
        /// <param name="useHighResolution">是否使用高分辨率(调用winmm.dll)</param>
        public HighPrecisionTimer(double intervalMs, Action callback, bool useHighResolution = true)
        {
            if (intervalMs <= 0)
            {
                throw new ArgumentException("间隔必须大于0", nameof(intervalMs));
            }

            this.intervalMs = intervalMs;
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.useHighResolution = useHighResolution;
            statistics = new TimerStatistics();
        }

        /// <summary>
        /// 创建高精度定时器(带统计信息的回调)
        /// </summary>
        public HighPrecisionTimer(double intervalMs, Action<TimerStatistics> callbackWithStats, bool useHighResolution = true)
        {
            if (intervalMs <= 0)
            {
                throw new ArgumentException("间隔必须大于0", nameof(intervalMs));
            }

            this.intervalMs = intervalMs;
            this.callbackWithStats = callbackWithStats ?? throw new ArgumentNullException(nameof(callbackWithStats));
            this.useHighResolution = useHighResolution;
            statistics = new TimerStatistics();
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 定时器是否正在运行
        /// </summary>
        public bool IsRunning => isRunning;

        /// <summary>
        /// 定时器是否已暂停
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// 目标执行次数(null表示无限循环)
        /// </summary>
        public long? TargetTickCount => targetTickCount;

        /// <summary>
        /// 是否已完成(达到目标次数)
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (targetTickCount == null)
                {
                    return false;
                }

                lock (lockObject)
                {
                    return statistics.TickCount >= targetTickCount.Value;
                }
            }
        }

        /// <summary>
        /// 获取定时器统计信息
        /// </summary>
        public TimerStatistics Statistics
        {
            get
            {
                lock (lockObject)
                {
                    return statistics.Clone();
                }
            }
        }

        /// <summary>
        /// 目标间隔(毫秒)
        /// </summary>
        public double IntervalMs => intervalMs;
        #endregion

        #region 公共方法

        /// <summary>
        /// 启动定时器并执行指定次数后自动停止
        /// </summary>
        /// <param name="tickCount">执行次数</param>
        /// <param name="onCompleted">完成时的回调(可选)</param>
        public void Start(long tickCount, Action onCompleted = null)
        {
            if (tickCount <= 0)
            {
                throw new ArgumentException("执行次数必须大于0", nameof(tickCount));
            }

            lock (lockObject)
            {
                targetTickCount = tickCount;
                this.onCompleted = onCompleted;
            }

            Start(); // 调用原有的 Start 方法
        }

        /// <summary>
        /// 启动定时器
        /// </summary>
        public void Start()
        {
            lock (lockObject)
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(nameof(HighPrecisionTimer));
                }

                if (isRunning)
                {
                    return;
                }

                isRunning = true;
                isPaused = false;
                statistics = new TimerStatistics();

                if (useHighResolution)
                {
                    timeBeginPeriod(1);
                }

                timerThread = new Thread(TimerLoop)
                {
                    Priority = ThreadPriority.Highest,
                    IsBackground = true,
                    Name = $"HighPrecisionTimer-{intervalMs}ms"
                };

                timerThread.Start();
            }
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        /// <param name="timeoutMs">等待线程结束的超时时间(毫秒)，-1表示无限等待</param>
        /// <returns>是否成功停止</returns>
        public bool Stop(int timeoutMs = 5000)
        {
            lock (lockObject)
            {
                if (!isRunning)
                {
                    return true;
                }

                isRunning = false;
                isPaused = false;
                targetTickCount = null; // 清理目标次数
                onCompleted = null; // 清理完成回调
            }

            bool joined = timerThread?.Join(timeoutMs) ?? true;

            if (useHighResolution)
            {
                timeEndPeriod(1);
            }

            return joined;
        }

        /// <summary>
        /// 暂停定时器
        /// </summary>
        public void Pause()
        {
            if (!isRunning)
            {
                throw new InvalidOperationException("定时器未运行");
            }

            isPaused = true;
        }

        /// <summary>
        /// 恢复定时器
        /// </summary>
        public void Resume()
        {
            if (!isRunning)
            {
                throw new InvalidOperationException("定时器未运行");
            }

            isPaused = false;
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void ResetStatistics()
        {
            lock (lockObject)
            {
                statistics.Reset();
            }
        }
        #endregion

        #region 核心循环

        private void TimerLoop()
        {
            long nextTrigger = Stopwatch.GetTimestamp();
            long intervalTicks = (long)(intervalMs * Stopwatch.Frequency / 1000.0);

            statistics.StartTime = DateTime.Now;

            while (isRunning)
            {
                // 检查是否达到目标次数
                if (targetTickCount.HasValue && statistics.TickCount >= targetTickCount.Value)
                {
                    // 达到目标次数，触发完成回调并停止
                    isRunning = false;

                    try
                    {
                        onCompleted?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            statistics.ErrorCount++;
                            statistics.LastError = ex;
                        }
                    }

                    break;
                }

                // 处理暂停
                if (isPaused)
                {
                    Thread.Sleep(10);
                    nextTrigger = Stopwatch.GetTimestamp(); // 重置下次触发时间
                    continue;
                }

                nextTrigger += intervalTicks;
                long beforeCallback = Stopwatch.GetTimestamp();

                try
                {
                    // 执行回调
                    if (callback != null)
                    {
                        callback.Invoke();
                    }
                    else if (callbackWithStats != null)
                    {
                        lock (lockObject)
                        {
                            callbackWithStats.Invoke(statistics.Clone());
                        }
                    }

                    // 更新统计
                    long afterCallback = Stopwatch.GetTimestamp();
                    UpdateStatistics(beforeCallback, afterCallback, nextTrigger);
                }
                catch (Exception ex)
                {
                    // 记录错误但继续运行
                    lock (lockObject)
                    {
                        statistics.ErrorCount++;
                        statistics.LastError = ex;
                    }
                }

                // 精确等待到下一个触发点
                PreciseWait(nextTrigger);
            }

            statistics.StopTime = DateTime.Now;

            // 清理资源
            if (useHighResolution)
            {
                timeEndPeriod(1);
            }
        }

        private void PreciseWait(long targetTimestamp)
        {
            const long threshold1 = 2; // 2ms的阈值(以毫秒为单位，后面转换)
            const long threshold2 = 1; // 1ms的阈值

            long remaining;
            double remainingMs;

            // 阶段1: >2ms 使用 Sleep(1)
            while ((remaining = targetTimestamp - Stopwatch.GetTimestamp()) > 0)
            {
                remainingMs = remaining * 1000.0 / Stopwatch.Frequency;

                if (remainingMs > threshold1)
                {
                    Thread.Sleep(1);
                }
                else if (remainingMs > threshold2)
                {
                    Thread.Yield();
                }
                else
                {
                    Thread.SpinWait(10);
                }
            }
        }

        private void UpdateStatistics(long beforeCallback, long afterCallback, long scheduledTime)
        {
            lock (lockObject)
            {
                statistics.TickCount++;

                // 计算实际间隔
                if (statistics.LastTriggerTime > 0)
                {
                    double actualIntervalMs = (beforeCallback - statistics.LastTriggerTime) * 1000.0 / Stopwatch.Frequency;
                    statistics.TotalActualInterval += actualIntervalMs;

                    if (actualIntervalMs < statistics.MinInterval)
                    {
                        statistics.MinInterval = actualIntervalMs;
                    }

                    if (actualIntervalMs > statistics.MaxInterval)
                    {
                        statistics.MaxInterval = actualIntervalMs;
                    }

                    // 计算误差
                    double error = actualIntervalMs - intervalMs;
                    statistics.TotalError += Math.Abs(error);

                    if (Math.Abs(error) > statistics.MaxError)
                    {
                        statistics.MaxError = Math.Abs(error);
                    }
                }

                // 计算回调执行时间
                double callbackDuration = (afterCallback - beforeCallback) * 1000.0 / Stopwatch.Frequency;
                statistics.TotalCallbackTime += callbackDuration;

                if (callbackDuration > statistics.MaxCallbackTime)
                {
                    statistics.MaxCallbackTime = callbackDuration;
                }

                // 计算延迟(相对于预定时间)
                double latency = (beforeCallback - scheduledTime) * 1000.0 / Stopwatch.Frequency;
                statistics.TotalLatency += Math.Abs(latency);

                statistics.LastTriggerTime = beforeCallback;
            }
        }
        #endregion

        #region 资源释放

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Stop();
            }

            disposed = true;
        }

        ~HighPrecisionTimer()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// 定时器统计信息
    /// </summary>
    public class TimerStatistics
    {
        public long TickCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }

        public double MinInterval { get; set; } = double.MaxValue;
        public double MaxInterval { get; set; } = double.MinValue;
        public double TotalActualInterval { get; set; }

        public double MaxError { get; set; }
        public double TotalError { get; set; }

        public double MaxCallbackTime { get; set; }
        public double TotalCallbackTime { get; set; }

        public double TotalLatency { get; set; }

        public int ErrorCount { get; set; }
        public Exception LastError { get; set; }

        internal long LastTriggerTime { get; set; }

        /// <summary>
        /// 平均间隔(毫秒)
        /// </summary>
        public double AverageInterval => TickCount > 1 ? TotalActualInterval / (TickCount - 1) : 0;

        /// <summary>
        /// 平均误差(毫秒)
        /// </summary>
        public double AverageError => TickCount > 1 ? TotalError / (TickCount - 1) : 0;

        /// <summary>
        /// 平均回调执行时间(毫秒)
        /// </summary>
        public double AverageCallbackTime => TickCount > 0 ? TotalCallbackTime / TickCount : 0;

        /// <summary>
        /// 平均延迟(毫秒)
        /// </summary>
        public double AverageLatency => TickCount > 0 ? TotalLatency / TickCount : 0;

        /// <summary>
        /// 运行时长(秒)
        /// </summary>
        public double ElapsedSeconds => (StopTime ?? DateTime.Now).Subtract(StartTime).TotalSeconds;

        public void Reset()
        {
            TickCount = 0;
            StartTime = DateTime.Now;
            StopTime = null;
            MinInterval = double.MaxValue;
            MaxInterval = double.MinValue;
            TotalActualInterval = 0;
            MaxError = 0;
            TotalError = 0;
            MaxCallbackTime = 0;
            TotalCallbackTime = 0;
            TotalLatency = 0;
            ErrorCount = 0;
            LastError = null;
            LastTriggerTime = 0;
        }

        public TimerStatistics Clone()
        {
            return (TimerStatistics)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return $"Ticks: {TickCount}, Avg: {AverageInterval:F3}ms, Error: ±{AverageError:F3}ms, Runtime: {ElapsedSeconds:F1}s";
        }
    }
}
