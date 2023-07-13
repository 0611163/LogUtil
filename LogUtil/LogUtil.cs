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

        private static LogWriter _infoWriter;

        private static LogWriter _debugWriter;

        private static LogWriter _errorWriter;

        private static LogWriterMutex _infoWriterMutex;

        private static LogWriterMutex _debugWriterMutex;

        private static LogWriterMutex _errorWriterMutex;

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

                    _infoWriter = null;
                    _debugWriter = null;
                    _errorWriter = null;

                    _infoWriterMutex = new LogWriterMutex(FileType.Info);
                    _debugWriterMutex = new LogWriterMutex(FileType.Debug);
                    _errorWriterMutex = new LogWriterMutex(FileType.Error);
                }
            }
        }

        private static LogLevel _writeToDebug = LogLevel.Debug;

        /// <summary>
        /// 写入Debug文件的日志级别
        /// </summary>
        public static LogLevel WriteToDebug
        {
            get
            {
                return _writeToDebug;
            }
            set
            {
                _writeToDebug = value;
            }
        }

        private static LogLevel _writeToInfo = LogLevel.Info;

        /// <summary>
        /// 写入Info文件的日志级别
        /// </summary>
        public static LogLevel WriteToInfo
        {
            get
            {
                return _writeToInfo;
            }
            set
            {
                if (value.HasFlag(LogLevel.Debug)) throw new Exception("不允许将Debug日志写入Info文件");

                _writeToInfo = value;
            }
        }

        #endregion

        #region 静态构造函数
        static LogUtil()
        {
            _infoWriter = new LogWriter(FileType.Info);
            _debugWriter = new LogWriter(FileType.Debug);
            _errorWriter = new LogWriter(FileType.Error);
        }
        #endregion

        #region 写调试日志
        /// <summary>
        /// 写调试日志
        /// </summary>
        public static void Debug(string log)
        {
            WriteToDebugFile(log, LogType.Debug);
        }

        /// <summary>
        /// 写调试日志
        /// </summary>
        private static void WriteToDebugFile(string log, LogType logType)
        {
            if (_supportMultiProcess)
            {
                _debugWriterMutex.WriteLog(log, logType);
            }
            else
            {
                _debugWriter.WriteLog(log, logType);
            }
        }
        #endregion

        #region 写操作日志
        /// <summary>
        /// 写操作日志
        /// </summary>
        public static void Log(string log)
        {
            Info(log);
        }

        /// <summary>
        /// 写操作日志
        /// </summary>
        public static void Info(string log)
        {
            WriteToInfoFile(log, LogType.Info);

            if (_writeToDebug.HasFlag(LogLevel.Info))
            {
                WriteToDebugFile(log, LogType.Info);
            }
        }

        /// <summary>
        /// 写操作日志
        /// </summary>
        private static void WriteToInfoFile(string log, LogType logType)
        {
            if (_supportMultiProcess)
            {
                _infoWriterMutex.WriteLog(log, logType);
            }
            else
            {
                _infoWriter.WriteLog(log, logType);
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
            WriteToErrorFile(log, LogType.Error);

            if (_writeToInfo.HasFlag(LogLevel.Error))
            {
                WriteToInfoFile(log, LogType.Error);
            }

            if (_writeToDebug.HasFlag(LogLevel.Error))
            {
                WriteToDebugFile(log, LogType.Error);
            }
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        private static void WriteToErrorFile(string log, LogType logType)
        {
            if (_supportMultiProcess)
            {
                _errorWriterMutex.WriteLog(log, logType);
            }
            else
            {
                _errorWriter.WriteLog(log, logType);
            }
        }
        #endregion

        #region Dispose
        public static void Dispose()
        {
            _infoWriter?.Dispose();
            _debugWriter?.Dispose();
            _errorWriter?.Dispose();

            _infoWriterMutex?.Dispose();
            _debugWriterMutex?.Dispose();
            _errorWriterMutex?.Dispose();
        }
        #endregion

    }

}
