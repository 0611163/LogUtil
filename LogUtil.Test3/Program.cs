using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogUtil.Test3
{
    class Program
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private static int n = 300000;

        static void Main(string[] args)
        {
            try
            {
                Log("==== 开始 ========");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                List<Task> taskList = new List<Task>();
                Task tsk = null;
                int taskCount = 0;

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log.Info("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log.Debug("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log.Error("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                Task.WaitAll(taskList.ToArray());
                Log("Task Count=" + taskCount);

                Log("==== 结束 " + "，耗时：" + stopwatch.Elapsed.TotalSeconds.ToString("0.000") + " 秒 ========");
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }

            Console.Read();
        }

        private static Task TaskRun(Action action)
        {
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
            });
        }

        #region Log
        private static void Log(string log)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " + log);
        }
        #endregion

    }
}
