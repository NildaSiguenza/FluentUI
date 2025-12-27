using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class HighPrecisionTimerTest
    {
        public static void RunTest()
        {
            Console.WriteLine("=== 高精度定时器功能测试 ===\n");

            //// 测试1: 基本的启动和停止
            //Test1_BasicStartStop();

            //// 测试2: 暂停和恢复
            //Test2_PauseResume();

            //// 测试3: 带统计信息的回调
            //Test3_StatisticsCallback();

            //// 测试4: 长时间运行和统计
            //Test4_LongRunning();

            //// 测试5: 异常处理
            //Test5_ExceptionHandling();

            //// 测试6: 多定时器并发
            //Test6_MultiplTimers();


            Console.WriteLine("=== 测试指定执行次数功能 ===\n");

            // 测试1: 基本的指定次数执行
            Test1_BasicLimitedExecution();

            // 测试2: 带完成回调
            Test2_WithCompletionCallback();

            // 测试3: 中途手动停止
            Test3_ManualStop();

            // 测试4: 对比无限循环和限定次数
            Test4_CompareInfiniteAndLimited();

            Console.WriteLine("=== 高精度定时器一般性测试 ===\n");

            // 测试1: 1毫秒间隔
            TestTimer(1.0, 1000, "1毫秒间隔");

            // 测试2: 0.5毫秒间隔
            TestTimer(0.5, 2000, "0.5毫秒间隔");

            // 测试3: 对比Yield和SpinWait
            CompareYieldVsSpinWait();
        }

        #region 功能测试

        static void Test1_BasicStartStop()
        {
            Console.WriteLine("【测试1】基本启动和停止");

            int count = 0;
            var timer = new HighPrecisionTimer(1.0, () =>
            {
                count++;
                Console.Write($"\r执行次数: {count}");
            });

            Console.WriteLine("启动定时器...");
            timer.Start();

            Thread.Sleep(3000);

            Console.WriteLine("\n停止定时器...");
            timer.Stop();

            var stats = timer.Statistics;
            Console.WriteLine($"总执行次数: {stats.TickCount}");
            Console.WriteLine($"平均间隔: {stats.AverageInterval:F3}ms");
            Console.WriteLine($"平均误差: {stats.AverageError:F3}ms\n");

            timer.Dispose();
        }

        static void Test2_PauseResume()
        {
            Console.WriteLine("【测试2】暂停和恢复");

            int count = 0;
            var timer = new HighPrecisionTimer(0.5, () =>
            {
                count++;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Tick {count}");
            });

            timer.Start();
            Console.WriteLine("运行2秒...");
            Thread.Sleep(2000);

            Console.WriteLine("\n暂停2秒...");
            timer.Pause();
            Thread.Sleep(2000);

            Console.WriteLine("\n恢复运行...");
            timer.Resume();
            Thread.Sleep(2000);

            timer.Stop();
            Console.WriteLine($"\n总执行次数: {count}");
            Console.WriteLine($"预期次数: 约8次 (2秒 + 2秒，每秒2次)\n");

            timer.Dispose();
        }

        static void Test3_StatisticsCallback()
        {
            Console.WriteLine("【测试3】带统计信息的回调");

            var timer = new HighPrecisionTimer(1.0, (stats) =>
            {
                Console.WriteLine($"[Tick {stats.TickCount}] " +
                                $"平均间隔: {stats.AverageInterval:F3}ms, " +
                                $"平均误差: {stats.AverageError:F3}ms");
            });

            timer.Start();
            Thread.Sleep(5000);
            timer.Stop();

            var finalStats = timer.Statistics;
            Console.WriteLine("\n最终统计:");
            Console.WriteLine($"  最小间隔: {finalStats.MinInterval:F3}ms");
            Console.WriteLine($"  最大间隔: {finalStats.MaxInterval:F3}ms");
            Console.WriteLine($"  最大误差: {finalStats.MaxError:F3}ms");
            Console.WriteLine($"  平均回调耗时: {finalStats.AverageCallbackTime:F3}ms\n");

            timer.Dispose();
        }

        static void Test4_LongRunning()
        {
            Console.WriteLine("【测试4】长时间运行和实时监控");

            var timer = new HighPrecisionTimer(0.5, () =>
            {
                // 模拟一些工作
                Thread.SpinWait(1000);
            });

            timer.Start();

            Console.WriteLine("定时器运行中，每2秒显示统计信息...");
            Console.WriteLine("按任意键停止\n");

            var sw = Stopwatch.StartNew();
            while (!Console.KeyAvailable && sw.Elapsed.TotalSeconds < 10)
            {
                Thread.Sleep(2000);

                var stats = timer.Statistics;
                Console.WriteLine($"[{sw.Elapsed.TotalSeconds:F1}s] {stats}");
            }

            if (Console.KeyAvailable)
                Console.ReadKey(true);

            timer.Stop();

            var finalStats = timer.Statistics;
            Console.WriteLine("\n=== 最终统计报告 ===");
            Console.WriteLine($"运行时长: {finalStats.ElapsedSeconds:F2}秒");
            Console.WriteLine($"总触发次数: {finalStats.TickCount}");
            Console.WriteLine($"平均间隔: {finalStats.AverageInterval:F4}ms");
            Console.WriteLine($"间隔范围: {finalStats.MinInterval:F4}ms ~ {finalStats.MaxInterval:F4}ms");
            Console.WriteLine($"平均误差: {finalStats.AverageError:F4}ms");
            Console.WriteLine($"最大误差: {finalStats.MaxError:F4}ms");
            Console.WriteLine($"平均延迟: {finalStats.AverageLatency:F4}ms");
            Console.WriteLine($"错误次数: {finalStats.ErrorCount}\n");

            timer.Dispose();
        }

        static void Test5_ExceptionHandling()
        {
            Console.WriteLine("【测试5】异常处理");

            int count = 0;
            var timer = new HighPrecisionTimer(1.0, () =>
            {
                count++;
                if (count == 3)
                {
                    Console.WriteLine("抛出异常...");
                    throw new InvalidOperationException("测试异常");
                }
                Console.WriteLine($"Tick {count}");
            });

            timer.Start();
            Thread.Sleep(5000);
            timer.Stop();

            var stats = timer.Statistics;
            Console.WriteLine($"\n总执行次数: {stats.TickCount}");
            Console.WriteLine($"错误次数: {stats.ErrorCount}");
            Console.WriteLine($"最后错误: {stats.LastError?.Message ?? "无"}");
            Console.WriteLine("定时器在异常后继续运行\n");

            timer.Dispose();
        }

        static void Test6_MultiplTimers()
        {
            Console.WriteLine("【测试6】多定时器并发");

            var timer1 = new HighPrecisionTimer(1.0, () =>
            {
                Console.WriteLine($"[Timer1-1ms] {DateTime.Now:HH:mm:ss.fff}");
            });

            var timer2 = new HighPrecisionTimer(2.5, () =>
            {
                Console.WriteLine($"  [Timer2-2.5ms] {DateTime.Now:HH:mm:ss.fff}");
            });

            var timer3 = new HighPrecisionTimer(5.0, () =>
            {
                Console.WriteLine($"    [Timer3-5ms] {DateTime.Now:HH:mm:ss.fff}");
            });

            Console.WriteLine("启动3个定时器...\n");
            timer1.Start();
            Thread.Sleep(500);
            timer2.Start();
            Thread.Sleep(500);
            timer3.Start();

            Thread.Sleep(10000);

            Console.WriteLine("\n停止所有定时器...");
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();

            Console.WriteLine($"\nTimer1: {timer1.Statistics}");
            Console.WriteLine($"Timer2: {timer2.Statistics}");
            Console.WriteLine($"Timer3: {timer3.Statistics}\n");

            timer1.Dispose();
            timer2.Dispose();
            timer3.Dispose();
        }

        #endregion

        #region 执行次数测试

        static void Test1_BasicLimitedExecution()
        {
            Console.WriteLine("【测试1】基本的指定次数执行");

            int count = 0;
            var timer = new HighPrecisionTimer(3.0, () =>
            {
                count++;
                Console.WriteLine($"执行第 {count} 次 [{DateTime.Now:HH:mm:ss.fff}]");
            });

            Console.WriteLine("启动定时器，执行10次...\n");
            timer.Start(10); // 执行10次后自动停止

            // 等待完成
            while (!timer.IsCompleted)
            {
                Thread.Sleep(50);
            }

            Thread.Sleep(500); // 确保定时器完全停止

            var stats = timer.Statistics;
            Console.WriteLine($"\n定时器已自动停止");
            Console.WriteLine($"实际执行次数: {count}");
            Console.WriteLine($"统计执行次数: {stats.TickCount}");
            Console.WriteLine($"目标执行次数: {timer.TargetTickCount}");
            Console.WriteLine($"是否完成: {timer.IsCompleted}");
            Console.WriteLine($"平均间隔: {stats.AverageInterval:F3}ms\n");

            timer.Dispose();
        }

        static void Test2_WithCompletionCallback()
        {
            Console.WriteLine("【测试2】带完成回调");

            var completed = false;
            var timer = new HighPrecisionTimer(5.0, () =>
            {
                Console.Write(".");
            });

            Console.WriteLine("启动定时器，执行20次...");
            timer.Start(20, () =>
            {
                Console.WriteLine("\r\n 完成回调被触发！");
                completed = true;
            });

            // 等待完成
            while (!completed)
            {
                Thread.Sleep(50);
            }

            var stats = timer.Statistics;
            Console.WriteLine($"总耗时: {stats.ElapsedSeconds:F2}秒");
            Console.WriteLine($"执行次数: {stats.TickCount}\n");

            timer.Dispose();
        }

        static void Test3_ManualStop()
        {
            Console.WriteLine("【测试3】中途手动停止");

            var timer = new HighPrecisionTimer(50.0, () =>
            {
                Console.Write(".");
            });

            Console.WriteLine("启动定时器，目标100次，但2秒后手动停止...");
            timer.Start(100, () =>
            {
                Console.WriteLine("\n完成回调(不应该执行)");
            });

            Thread.Sleep(2000);

            Console.WriteLine("\n手动停止定时器");
            timer.Stop();

            var stats = timer.Statistics;
            Console.WriteLine($"实际执行次数: {stats.TickCount}");
            Console.WriteLine($"目标执行次数: {timer.TargetTickCount ?? 0}");
            Console.WriteLine($"是否完成: {timer.IsCompleted}\n");

            timer.Dispose();
        }

        static void Test4_CompareInfiniteAndLimited()
        {
            Console.WriteLine("【测试4】对比无限循环和限定次数");

            // 无限循环模式
            Console.WriteLine("模式1: 无限循环(手动停止)");
            int count1 = 0;
            var timer1 = new HighPrecisionTimer(1.0, () => { count1++; });

            timer1.Start(); // 无参数 = 无限循环
            Thread.Sleep(1000);
            timer1.Stop();

            Console.WriteLine($"  执行次数: {count1}");
            Console.WriteLine($"  目标次数: {timer1.TargetTickCount?.ToString() ?? "无限"}");
            Console.WriteLine($"  是否完成: {timer1.IsCompleted}");

            // 限定次数模式
            Console.WriteLine("\n模式2: 限定次数(自动停止)");
            int count2 = 0;
            var timer2 = new HighPrecisionTimer(1.0, () => { count2++; });

            var sw = System.Diagnostics.Stopwatch.StartNew();
            timer2.Start(1000); // 执行1000次

            while (!timer2.IsCompleted)
            {
                Thread.Sleep(50);
            }
            sw.Stop();

            Console.WriteLine($"  执行次数: {count2}");
            Console.WriteLine($"  目标次数: {timer2.TargetTickCount}");
            Console.WriteLine($"  是否完成: {timer2.IsCompleted}");
            Console.WriteLine($"  总耗时: {sw.ElapsedMilliseconds}ms");

            var stats2 = timer2.Statistics;
            Console.WriteLine($"  平均间隔: {stats2.AverageInterval:F3}ms");
            Console.WriteLine($"  理论耗时: {1000 * 1.0}ms\n");

            timer1.Dispose();
            timer2.Dispose();
        }

        #endregion

        #region 一般性测试

        static void TestTimer(double intervalMs, int iterations, string testName)
        {
            Console.WriteLine($"【{testName}】");
            Console.WriteLine($"目标间隔: {intervalMs}ms");
            Console.WriteLine($"执行次数: {iterations}");

            var timestamps = new List<long>();
            var stopwatch = Stopwatch.StartNew();
            int count = 0;

            var timer = new HighPrecisionTimer(intervalMs, () =>
            {
                timestamps.Add(stopwatch.ElapsedTicks);
                count++;
            });

            timer.Start();

            // 等待完成
            while (count < iterations)
            {
                Thread.Sleep(10);
            }

            timer.Stop();

            // 分析结果
            AnalyzeResults(timestamps, intervalMs);
            Console.WriteLine();
        }

        static void AnalyzeResults(List<long> timestamps, double targetIntervalMs)
        {
            if (timestamps.Count < 2)
            {
                return;
            }

            var intervals = new List<double>();
            for (int i = 1; i < timestamps.Count; i++)
            {
                double intervalMs = (timestamps[i] - timestamps[i - 1]) * 1000.0 / Stopwatch.Frequency;
                intervals.Add(intervalMs);
            }

            double avgInterval = intervals.Average();
            double minInterval = intervals.Min();
            double maxInterval = intervals.Max();
            double stdDev = Math.Sqrt(intervals.Average(x => Math.Pow(x - avgInterval, 2)));

            // 计算误差分布
            var errors = intervals.Select(i => Math.Abs(i - targetIntervalMs)).ToList();
            int within100us = errors.Count(e => e < 0.1);
            int within500us = errors.Count(e => e < 0.5);

            Console.WriteLine($"平均间隔: {avgInterval:F4}ms (误差: {(avgInterval - targetIntervalMs):+0.0000;-0.0000}ms)");
            Console.WriteLine($"最小间隔: {minInterval:F4}ms");
            Console.WriteLine($"最大间隔: {maxInterval:F4}ms");
            Console.WriteLine($"标准差:   {stdDev:F4}ms");
            Console.WriteLine($"精度分析: ±100μs内: {within100us}/{intervals.Count} ({100.0 * within100us / intervals.Count:F1}%)");
            Console.WriteLine($"         ±500μs内: {within500us}/{intervals.Count} ({100.0 * within500us / intervals.Count:F1}%)");
        }

        static void CompareYieldVsSpinWait()
        {
            Console.WriteLine("=== Thread.Yield() vs Thread.SpinWait() 对比 ===\n");

            const int iterations = 10000;

            // 测试 Yield
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                Thread.Yield();
            }
            var yieldTime = sw.Elapsed.TotalMilliseconds;

            // 测试 SpinWait
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                Thread.SpinWait(100);
            }
            var spinWaitTime = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"Thread.Yield() {iterations}次耗时: {yieldTime:F3}ms (平均: {yieldTime / iterations * 1000:F2}μs/次)");
            Console.WriteLine($"Thread.SpinWait(100) {iterations}次耗时: {spinWaitTime:F3}ms (平均: {spinWaitTime / iterations * 1000:F2}μs/次)");
            Console.WriteLine($"性能差异: SpinWait比Yield快 {yieldTime / spinWaitTime:F1}倍");
        }

        #endregion

        #region 场景测试

        /// <summary>
        /// 数据采集系统
        /// </summary>
        public static void DataAcquisitionSystem()
        {
            Console.WriteLine("=== 数据采集系统示例 ===\n");

            var dataBuffer = new System.Collections.Concurrent.ConcurrentQueue<SensorData>();
            int sampleRate = 1000; // 1kHz采样率

            var acquisitionTimer = new HighPrecisionTimer(1.0, (stats) =>
            {
                // 模拟读取传感器数据
                var data = new SensorData
                {
                    Timestamp = DateTime.Now,
                    Value = Math.Sin(stats.TickCount * 0.1),
                    SequenceNumber = stats.TickCount
                };

                dataBuffer.Enqueue(data);

                // 每1000个样本显示一次统计
                if (stats.TickCount % 1000 == 0)
                {
                    Console.WriteLine($"采集 {stats.TickCount} 个样本, " +
                                    $"平均采样率: {1000.0 / stats.AverageInterval:F2}Hz, " +
                                    $"误差: ±{stats.AverageError:F3}ms");
                }
            });

            acquisitionTimer.Start();

            // 运行10秒
            Thread.Sleep(10000);

            acquisitionTimer.Stop();

            Console.WriteLine($"\n采集完成，共 {dataBuffer.Count} 个样本");
            Console.WriteLine($"最终统计: {acquisitionTimer.Statistics}");

            acquisitionTimer.Dispose();
        }

        /// <summary>
        /// 实时控制系统
        /// </summary>
        public static void RealTimeControlSystem()
        {
            Console.WriteLine("\n=== 实时控制系统示例 ===\n");

            double setpoint = 100.0;
            double processValue = 0.0;
            double output = 0.0;

            // PID控制参数
            double Kp = 0.5, Ki = 0.1, Kd = 0.05;
            double integral = 0.0, lastError = 0.0;

            var controlTimer = new HighPrecisionTimer(10.0, () => // 100Hz控制循环
            {
                // PID控制算法
                double error = setpoint - processValue;
                integral += error * 0.01; // dt = 10ms
                double derivative = (error - lastError) / 0.01;

                output = Kp * error + Ki * integral + Kd * derivative;

                // 模拟过程响应
                processValue += output * 0.01;

                lastError = error;
            });

            controlTimer.Start();

            // 监控线程
            var monitorTimer = new HighPrecisionTimer(1000.0, () =>
            {
                Console.WriteLine($"PV: {processValue:F2}, Output: {output:F2}, Error: {setpoint - processValue:F2}");
            });

            monitorTimer.Start();

            Thread.Sleep(5000);

            controlTimer.Stop();
            monitorTimer.Stop();

            Console.WriteLine($"\n控制循环统计: {controlTimer.Statistics}");

            controlTimer.Dispose();
            monitorTimer.Dispose();
        }

        /// <summary>
        /// 性能基准测试
        /// </summary>
        public static void PerformanceBenchmark()
        {
            Console.WriteLine("\n=== 性能基准测试 ===\n");

            double[] intervals = { 0.1, 0.5, 1.0, 5.0, 10.0 };

            foreach (var interval in intervals)
            {
                Console.WriteLine($"测试间隔: {interval}ms");

                var timer = new HighPrecisionTimer(interval, () => { });

                timer.Start();
                Thread.Sleep(5000);
                timer.Stop();

                var stats = timer.Statistics;

                Console.WriteLine($"  执行次数: {stats.TickCount}");
                Console.WriteLine($"  平均间隔: {stats.AverageInterval:F4}ms");
                Console.WriteLine($"  平均误差: {stats.AverageError:F4}ms ({stats.AverageError / interval * 100:F2}%)");
                Console.WriteLine($"  最大误差: {stats.MaxError:F4}ms");
                Console.WriteLine($"  标准差范围: {stats.MinInterval:F4}ms ~ {stats.MaxInterval:F4}ms\n");

                timer.Dispose();
            }
        }


        class SensorData
        {
            public DateTime Timestamp { get; set; }

            public double Value { get; set; }

            public long SequenceNumber { get; set; }
        }
        #endregion

    }
}
