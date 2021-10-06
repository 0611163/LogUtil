using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// 写日志类
    /// </summary>
    public class LogUtil
    {
        #region 字段
        private static LogWriter _infoWriter = new LogWriter(LogType.Info);

        private static LogWriter _debugWriter = new LogWriter(LogType.Debug);

        private static LogWriter _errorWriter = new LogWriter(LogType.Error);

        private static TaskSchedulerEx _scheduler = new TaskSchedulerEx(2, 2);
        #endregion

        #region 写操作日志
        /// <summary>
        /// 写操作日志
        /// </summary>
        public static Task Log(string log)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    _infoWriter.WriteLog(log);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
            }, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }
        #endregion

        #region 写调试日志
        /// <summary>
        /// 写调试日志
        /// </summary>
        public static Task Debug(string log)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    _debugWriter.WriteLog(log);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
            }, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }
        #endregion

        #region 写错误日志
        public static Task Error(Exception ex, string log = null)
        {
            return Error(string.IsNullOrEmpty(log) ? ex.Message + "\r\n" + ex.StackTrace : (log + "：") + ex.Message + "\r\n" + ex.StackTrace);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        public static Task Error(string log)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    _errorWriter.WriteLog(log);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
            }, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }
        #endregion

    }

}
