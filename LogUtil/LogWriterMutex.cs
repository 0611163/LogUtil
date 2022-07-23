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
    /// 支持多进程并发写日志的LogWriter版本
    /// </summary>
    internal class LogWriterMutex
    {
        #region 字段属性

        private LogType _logType;

        private string _basePath;

        private int _fileSize = 10 * 1024 * 1024; //日志分隔文件大小

        private LogStream _currentStream = new LogStream();

        private string _dateFormat = "yyyyMMdd"; //日志文件名日期格式化

        private string _rootFolder = "Log"; //日志文件夹名称

        private Mutex _mutex;

        private SharedMemory _sharedMemory;

        private DateTime _lastCheckFileExistsTime = DateTime.Now;

        #endregion

        #region LogWriter
        public LogWriterMutex(LogType logType)
        {
            _logType = logType;
            _mutex = new Mutex(false, "Mutex.LogWriter." + logType.ToString() + ".7693FFAD38004F6B8FD31F6A8B4CE2BD");
            _sharedMemory = new SharedMemory(logType);

            Init();
        }
        #endregion

        #region Init
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            //初始化 _basePath
            InitBasePath();

            //更新日志写入流
            UpdateCurrentStream();
        }
        #endregion

        #region 初始化 _basePath
        /// <summary>
        /// 初始化 _basePath
        /// </summary>
        private void InitBasePath()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _basePath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }
        #endregion

        #region 初始化 _currentArchiveIndex
        /// <summary>
        /// 初始化 _currentArchiveIndex
        /// </summary>
        private void InitCurrentArchiveIndex()
        {
            Regex regex = new Regex(_currentStream.CurrentDateStr + "_*(\\d*).txt");
            string[] fileArr = Directory.GetFiles(_currentStream.CurrentLogFileDir, _currentStream.CurrentDateStr + "*");
            foreach (string file in fileArr)
            {
                Match match = regex.Match(file);
                if (match.Success)
                {
                    string str = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        int temp = Convert.ToInt32(str);
                        if (temp > _currentStream.CurrentArchiveIndex)
                        {
                            _currentStream.CurrentArchiveIndex = temp;
                        }
                    }
                    else
                    {
                        _currentStream.CurrentArchiveIndex = -1;
                    }
                }
            }
        }
        #endregion

        #region 初始化 _currentFileSize
        /// <summary>
        /// 初始化 _currentFileSize
        /// </summary>
        private void InitCurrentFileSize()
        {
            FileInfo fileInfo = new FileInfo(_currentStream.CurrentLogFilePath);
            _currentStream.CurrentFileSize = fileInfo.Length;
            _sharedMemory.Write(_currentStream.CurrentFileSize);
        }
        #endregion

        #region CreateLogDir()
        /// <summary>
        /// 创建日志目录
        /// </summary>
        private void CreateLogDir()
        {
            string logDir = Path.Combine(_basePath, _rootFolder + "\\" + _logType.ToString());
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }
        #endregion

        #region CreateStream
        /// <summary>
        /// 创建日志写入流
        /// </summary>
        private void CreateStream()
        {
            _currentStream.CurrentFileStream = new FileStream(
                _currentStream.CurrentLogFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite | FileShare.Delete);
        }
        #endregion

        #region CloseStream
        /// <summary>
        /// 关闭日志写入流
        /// </summary>
        private void CloseStream()
        {
            if (_currentStream.CurrentFileStream != null)
            {
                _currentStream.CurrentFileStream.Close();
            }
        }
        #endregion

        #region Dispose 释放资源
        public void Dispose()
        {
            CloseStream();

            _currentStream.CurrentFileStream = null;
            _currentStream = null;
        }
        #endregion

        #region 拼接日志内容
        /// <summary>
        /// 拼接日志内容
        /// </summary>
        private static string CreateLogString(LogType logType, string log)
        {
            return string.Format("{0} {1} {2}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), ("[" + logType.ToString() + "]").PadRight(7, ' '), log);
        }
        #endregion

        #region 写文件
        /// <summary>
        /// 写文件
        /// </summary>
        private void WriteFile(string log)
        {
            byte[] bArr = null;

            try
            {
                //判断是否更新Stream
                string dateStr = DateTime.Now.ToString(_dateFormat);
                if (_currentStream.CurrentDateStr != dateStr)
                {
                    _currentStream.CurrentDateStr = dateStr;
                    UpdateCurrentStream();
                }

                //判断文件是否存在
                if (DateTime.Now.Subtract(_lastCheckFileExistsTime).TotalMilliseconds > 500)
                {
                    _lastCheckFileExistsTime = DateTime.Now;
                    if (!File.Exists(_currentStream.CurrentLogFilePath))
                    {
                        UpdateCurrentStream();
                    }
                }

                try
                {
                    _mutex.WaitOne();
                }
                catch (AbandonedMutexException)
                {
                }

                //判断是否创建Archive
                bArr = Encoding.UTF8.GetBytes(log);
                int byteCount = bArr.Length;

                _currentStream.CurrentFileSize = _sharedMemory.Read();
                _currentStream.CurrentFileSize += byteCount;
                _sharedMemory.Write(_currentStream.CurrentFileSize);

                if (_currentStream.CurrentFileSize >= _fileSize)
                {
                    _currentStream.CurrentFileSize = byteCount;
                    _sharedMemory.Write(_currentStream.CurrentFileSize);
                    CreateArchive();
                }

                //日志内容写入文件
                _currentStream.CurrentFileStream.Seek(0, SeekOrigin.End);
                _currentStream.CurrentFileStream.Write(bArr, 0, bArr.Length);
                _currentStream.CurrentFileStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
        #endregion

        #region CreateArchive
        /// <summary>
        /// 创建日志存档
        /// </summary>
        private void CreateArchive()
        {
            try
            {
                CloseStream(); //关闭日志写入流

                string fileName = Path.GetFileNameWithoutExtension(_currentStream.CurrentLogFilePath);
                string newFilePath = Path.Combine(_currentStream.CurrentLogFileDir, fileName + "_" + (++_currentStream.CurrentArchiveIndex) + ".txt");

                if (!File.Exists(newFilePath))
                {
                    File.Copy(_currentStream.CurrentLogFilePath, newFilePath); //存档

                    //清空
                    _currentStream.CurrentFileStream = new FileStream(_currentStream.CurrentLogFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                    _currentStream.CurrentFileStream.SetLength(0);
                    _currentStream.CurrentFileStream.Close();
                }
                else
                {
                    //初始化 _currentFileSize
                    InitCurrentFileSize();
                }

                CreateStream(); //创建日志写入流
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        #endregion

        #region UpdateCurrentStream
        /// <summary>
        /// 更新日志写入流
        /// </summary>
        private void UpdateCurrentStream()
        {
            try
            {
                try
                {
                    _mutex.WaitOne();
                }
                catch (AbandonedMutexException)
                {
                }

                //创建目录
                CreateLogDir();

                //关闭日志写入流
                CloseStream();

                //创建新的日志路径
                _currentStream.CurrentDateStr = DateTime.Now.ToString(_dateFormat);
                _currentStream.CurrentLogFileDir = Path.Combine(_basePath, _rootFolder + "\\" + _logType.ToString());
                _currentStream.CurrentLogFilePath = Path.Combine(_currentStream.CurrentLogFileDir, _currentStream.CurrentDateStr + ".txt");

                //创建日志写入流
                CreateStream();

                //初始化 _currentArchiveIndex
                InitCurrentArchiveIndex();

                //初始化 _currentFileSize
                InitCurrentFileSize();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
        #endregion

        #region 写日志
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log">日志内容</param>
        public void WriteLog(string log)
        {
            try
            {
                log = CreateLogString(_logType, log);
                WriteFile(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        #endregion

    }
}
