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

        private static LogWriterMutex _infoWriterMutex = new LogWriterMutex(LogType.Info);

        private static LogWriterMutex _debugWriterMutex = new LogWriterMutex(LogType.Debug);

        private static LogWriterMutex _errorWriterMutex = new LogWriterMutex(LogType.Error);

        private static bool _supportMultiProcess = false;

        /// <summary>
        /// 是否支持多进程
        /// </summary>
        public static bool SupportMultiProcess
        {
            get
            {
                return _supportMultiProcess;
            }
            set
            {
                _supportMultiProcess = value;

                if (_supportMultiProcess)
                {
                    _infoWriter?.Dispose();
                    _debugWriter?.Dispose();
                    _errorWriter?.Dispose();

                    _infoWriterMutex = new LogWriterMutex(LogType.Info);
                    _debugWriterMutex = new LogWriterMutex(LogType.Debug);
                    _errorWriterMutex = new LogWriterMutex(LogType.Error);
                }
            }
        }

        #endregion

        #region 静态构造函数
        static LogUtil()
        {
            _infoWriter = new LogWriter(LogType.Info);
            _debugWriter = new LogWriter(LogType.Debug);
            _errorWriter = new LogWriter(LogType.Error);
        }
        #endregion

        #region 写操作日志
        /// <summary>
        /// 写操作日志
        /// </summary>
        public static void Log(string log)
        {
            if (_supportMultiProcess)
            {
                _infoWriterMutex.WriteLog(log);
            }
            else
            {
                _infoWriter.WriteLog(log);
            }
        }

        /// <summary>
        /// 写操作日志
        /// </summary>
        public static void Info(string log)
        {
            if (_supportMultiProcess)
            {
                _infoWriterMutex.WriteLog(log);
            }
            else
            {
                _infoWriter.WriteLog(log);
            }
        }
        #endregion

        #region 写调试日志
        /// <summary>
        /// 写调试日志
        /// </summary>
        public static void Debug(string log)
        {
            if (_supportMultiProcess)
            {
                _debugWriterMutex.WriteLog(log);
            }
            else
            {
                _debugWriter.WriteLog(log);
            }
        }
        #endregion

        #region 写错误日志
        /// <summary>
        /// 写错误日志
        /// </summary>
        public static void Error(Exception ex, string log = null)
        {
            Error(string.IsNullOrEmpty(log) ? ex.ToString() : log + "：" + ex.ToString());
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        public static void Error(string log, Exception ex)
        {
            Error(string.IsNullOrEmpty(log) ? ex.ToString() : log + "：" + ex.ToString());
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        public static void Error(string log)
        {
            if (_supportMultiProcess)
            {
                _errorWriterMutex.WriteLog(log);
            }
            else
            {
                _errorWriter.WriteLog(log);
            }
        }
        #endregion

    }

}
