using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace LogUtilTest
{
    public partial class Form1 : Form
    {
        private Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private log4net.ILog _log2 = null;

        private int n = 100000;

        public Form1()
        {
            InitializeComponent();
            ThreadPool.SetMinThreads(20, 20);

            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            FileInfo configFile = new FileInfo(Path.Combine(path, "log4net.config"));
            log4net.Config.XmlConfigurator.Configure(configFile);

            _log2 = log4net.LogManager.GetLogger(typeof(Form1));
        }

        #region Log
        private void Log(string log)
        {
            if (!this.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        textBox1.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + " " + log + "\r\n\r\n");
                    }));
                }
                else
                {
                    textBox1.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + " " + log + "\r\n\r\n");
                }
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            LogUtil.Log("测试写 Info 日志");
            LogUtil.Debug("测试写 Debug 日志");
            LogUtil.Error("测试写 Error 日志");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Log("==== 开始 ========");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                ConcurrentQueue<Task> taskQueue = new ConcurrentQueue<Task>();
                List<Task> taskList = new List<Task>();
                Task tsk = null;

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        Task task = LogUtil.Log("测试日志 " + i.ToString("000000"));
                        taskQueue.Enqueue(task);
                    }
                });
                taskList.Add(tsk);

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        Task task = LogUtil.Debug("测试日志 " + i.ToString("000000"));
                        taskQueue.Enqueue(task);
                    }
                });
                taskList.Add(tsk);

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        Task task = LogUtil.Error("测试日志 " + i.ToString("000000"));
                        taskQueue.Enqueue(task);
                    }
                });
                taskList.Add(tsk);

                Task.WaitAll(taskList.ToArray());
                Task.WaitAll(taskQueue.ToArray());
                Log("taskQueue.Count=" + taskQueue.Count);

                Log("==== 结束 " + "，耗时：" + stopwatch.Elapsed.TotalSeconds.ToString("0.000") + " 秒 ========");
                stopwatch.Stop();
            });
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            Log("==== 开始 ========");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ConcurrentQueue<Task> taskQueue = new ConcurrentQueue<Task>();

            await Task.Run(() =>
            {
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Log("测试日志 " + i.ToString("000000"));
                    taskQueue.Enqueue(task);
                }
            });

            await Task.Run(() =>
            {
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Debug("测试日志 " + i.ToString("000000"));
                    taskQueue.Enqueue(task);
                }
            });

            await Task.Run(() =>
            {
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Error("测试日志 " + i.ToString("000000"));
                    taskQueue.Enqueue(task);
                }
            });

            Log("taskQueue.Count=" + taskQueue.Count);
            foreach (Task tsk in taskQueue.ToList())
            {
                await tsk;
            }

            Log("==== 结束 " + "，耗时：" + stopwatch.Elapsed.TotalSeconds.ToString("0.000") + " 秒 ========");
            stopwatch.Stop();
        }

        //对比NLog
        private void button4_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Log("==== 开始 ========");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                List<Task> taskList = new List<Task>();
                Task tsk = null;
                int taskCount = 0;

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log.Info("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log.Debug("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = Task.Run(() =>
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
            });
        }

        //对比log4net
        private void button5_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Log("==== 开始 ========");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                List<Task> taskList = new List<Task>();
                Task tsk = null;
                int taskCount = 0;

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log2.Info("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log2.Debug("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = Task.Run(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log2.Error("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                Task.WaitAll(taskList.ToArray());
                Log("Task Count=" + taskCount);

                Log("==== 结束 " + "，耗时：" + stopwatch.Elapsed.TotalSeconds.ToString("0.000") + " 秒 ========");
                stopwatch.Stop();
            });
        }
    }
}
