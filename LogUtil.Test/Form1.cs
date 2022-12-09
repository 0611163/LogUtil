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

            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
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

        #region TaskRun
        private Task TaskRun(Action action)
        {
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Log(ex.Message + "\r\n" + ex.StackTrace);
                }
            });
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
                List<Task> taskList = new List<Task>();
                Task tsk = null;
                int taskCount = 0;

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        LogUtil.Debug("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        LogUtil.Debug("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        LogUtil.Debug("测试日志 " + i.ToString("000000"));
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

        //对比NLog
        private void button3_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
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
                        _log.Debug("测试日志 " + i.ToString("000000"));
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
                        _log.Debug("测试日志 " + i.ToString("000000"));
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

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log2.Debug("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log2.Debug("测试日志 " + i.ToString("000000"));
                        Interlocked.Increment(ref taskCount);
                    }
                });
                taskList.Add(tsk);

                tsk = TaskRun(() =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        _log2.Debug("测试日志 " + i.ToString("000000"));
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


        //计算LogUtil日志行数
        private void button5_Click(object sender, EventArgs e)
        {
            TaskRun(() =>
            {
                string basePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                string dirPath = basePath + "\\Log";

                int count = 0;
                if (Directory.Exists(dirPath))
                {
                    foreach (string file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                    {
                        if (File.Exists(file))
                        {
                            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    while (!string.IsNullOrWhiteSpace(sr.ReadLine()))
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }
                Log("LogUtil日志行数：" + count);
            });
        }

        //多进程
        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                Process process = new Process();
                process.StartInfo.FileName = "LogUtil.Test2.exe";
                process.Start();
            }
        }

        //多进程
        private void button7_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                Process process = new Process();
                process.StartInfo.FileName = "LogUtil.Test3.exe";
                process.Start();
            }
        }

        //计算NLog日志行数
        private void button8_Click(object sender, EventArgs e)
        {
            TaskRun(() =>
            {
                string basePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                string dirPath = basePath + "\\nlog";

                int count = 0;
                if (Directory.Exists(dirPath))
                {
                    foreach (string file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                    {
                        if (File.Exists(file))
                        {
                            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    while (!string.IsNullOrWhiteSpace(sr.ReadLine()))
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }
                Log("NLog日志行数：" + count);
            });
        }
    }
}
