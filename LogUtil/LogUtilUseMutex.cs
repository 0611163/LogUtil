﻿using System;
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
    /// 写日志类 支持多进程并发
    /// </summary>
    public class LogUtilUseMutex
    {
        #region 字段

        private static LogWriterUseMutex _infoWriter = new LogWriterUseMutex(LogType.Info);

        private static LogWriterUseMutex _debugWriter = new LogWriterUseMutex(LogType.Debug);

        private static LogWriterUseMutex _errorWriter = new LogWriterUseMutex(LogType.Error);

        #endregion

        #region 写操作日志
        /// <summary>
        /// 写操作日志
        /// </summary>
        public static void Log(string log)
        {
            _infoWriter.WriteLog(log);
        }
        #endregion

        #region 写调试日志
        /// <summary>
        /// 写调试日志
        /// </summary>
        public static void Debug(string log)
        {
            _debugWriter.WriteLog(log);
        }
        #endregion

        #region 写错误日志
        public static void Error(Exception ex, string log = null)
        {
            Error(string.IsNullOrEmpty(log) ? ex.Message + "\r\n" + ex.StackTrace : (log + "：") + ex.Message + "\r\n" + ex.StackTrace);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        public static void Error(string log)
        {
            _errorWriter.WriteLog(log);
        }
        #endregion

    }

}
