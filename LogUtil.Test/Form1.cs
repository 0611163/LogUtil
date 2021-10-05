using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace LogUtilTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ThreadPool.SetMinThreads(20, 20);
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
            Log("==== 开始 ========");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Task> taskList = new List<Task>();

            Task.Run(() =>
            {
                int n = 10000;
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Log("测试日志 " + i.ToString("000000"));
                    taskList.Add(task);
                }
            });

            Task.Run(() =>
            {
                int n = 10000;
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Debug("测试日志 " + i.ToString("000000"));
                    taskList.Add(task);
                }
            });

            Task.Run(() =>
            {
                int n = 10000;
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Error("测试日志 " + i.ToString("000000"));
                    taskList.Add(task);
                }
            });

            Task.WaitAll(taskList.ToArray());

            Log("==== 结束 " + "，耗时：" + stopwatch.Elapsed.TotalSeconds.ToString("0.000") + " 秒 ========");
            stopwatch.Stop();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            Log("==== 开始 ========");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Task> taskList = new List<Task>();

            await Task.Run(() =>
            {
                int n = 10000;
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Log("测试日志 " + i.ToString("000000"));
                    taskList.Add(task);
                }
            });

            await Task.Run(() =>
            {
                int n = 10000;
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Debug("测试日志 " + i.ToString("000000"));
                    taskList.Add(task);
                }
            });

            await Task.Run(() =>
            {
                int n = 10000;
                for (int i = 0; i < n; i++)
                {
                    Task task = LogUtil.Error("测试日志 " + i.ToString("000000"));
                    taskList.Add(task);
                }
            });

            foreach (Task tsk in taskList)
            {
                await tsk;
            }

            Log("==== 结束 " + "，耗时：" + stopwatch.Elapsed.TotalSeconds.ToString("0.000") + " 秒 ========");
            stopwatch.Stop();
        }
    }
}
