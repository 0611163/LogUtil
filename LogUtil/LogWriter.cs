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
    internal class LogWriter
    {
        #region 字段属性

        private LogType _logType;

        private string _basePath;

        private FileStream _fileStream;

        private StreamWriter _streamWriter;

        private BlockingCollection<string> _logs = new BlockingCollection<string>();

        private int _fileSize = 1 * 1024 * 1024; //日志分隔文件大小

        private int _currentArchiveIndex = 0;

        private long _currentFileSize = 0;

        private string _currentDateStr;

        private bool _run = true;

        private string _dateFormat = "yyyyMMddHHmmss"; //日志文件名日期格式化

        private string _rootFolder = "Log"; //日志文件夹名称

        #endregion

        #region LogWriter
        public LogWriter(LogType logType)
        {
            _logType = logType;

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

            //创建目录
            _currentDateStr = DateTime.Now.ToString(_dateFormat);
            string logFilePath = CreateLogPath(_logType, _currentDateStr);
            string logDir = Path.GetDirectoryName(logFilePath);

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            //创建日志写入流
            CreateStream(logFilePath);

            //初始化 _currentArchiveIndex
            InitCurrentArchiveIndex(logDir, _currentDateStr);

            //初始化 _currentFileSize
            InitCurrentFileSize(logFilePath);

            //创建写日志线程
            CreateWriterThread(logFilePath);
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
        private void InitCurrentArchiveIndex(string pathFolder, string currentDateStr)
        {
            Regex regex = new Regex(currentDateStr + "_*(\\d*).txt");
            string[] fileArr = Directory.GetFiles(pathFolder);
            foreach (string file in fileArr)
            {
                Match match = regex.Match(file);
                if (match.Success)
                {
                    string str = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        int temp = Convert.ToInt32(str);
                        if (temp > _currentArchiveIndex)
                        {
                            _currentArchiveIndex = temp;
                        }
                    }
                    else
                    {
                        _currentArchiveIndex = 0;
                    }
                }
            }
        }
        #endregion

        #region 初始化 _currentFileSize
        /// <summary>
        /// 初始化 _currentFileSize
        /// </summary>
        private void InitCurrentFileSize(string logFilePath)
        {
            FileInfo fileInfo = new FileInfo(logFilePath);
            _currentFileSize = fileInfo.Length;
        }
        #endregion

        #region CreateWriterThread
        /// <summary>
        /// 创建写日志线程
        /// </summary>
        private void CreateWriterThread(string logFilePath)
        {
            Thread thread = null;
            thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    string log;

                    while (_run)
                    {
                        if (_logs.TryTake(out log, Timeout.Infinite))
                        {
                            WriteFile(log, logFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
            }));
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region CreateStream
        /// <summary>
        /// 创建日志写入流
        /// </summary>
        private void CreateStream(string logFilePath)
        {
            _fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8);
        }
        #endregion

        #region CloseStream
        /// <summary>
        /// 关闭日志写入流
        /// </summary>
        private void CloseStream()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Close();
            }

            if (_fileStream != null)
            {
                _fileStream.Close();
            }
        }
        #endregion

        #region 拼接日志内容
        /// <summary>
        /// 拼接日志内容
        /// </summary>
        private static string CreateLogString(LogType logType, string log)
        {
            return string.Format(@"{0} {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), ("[" + logType.ToString() + "]").PadRight(7, ' '), log);
        }
        #endregion

        #region 写文件
        /// <summary>
        /// 写文件
        /// </summary>
        private void WriteFile(string log, string logFilePath)
        {
            try
            {
                int byteCount = Encoding.UTF8.GetByteCount(log);
                _currentFileSize += byteCount;
                if (_currentFileSize >= _fileSize)
                {
                    _currentFileSize = 0;
                    CreateArchive(logFilePath);
                }

                string dateStr = DateTime.Now.ToString(_dateFormat);
                if (_currentDateStr != dateStr)
                {
                    _currentDateStr = dateStr;
                    UpdateLogFileName(_currentDateStr);
                }

                _streamWriter.WriteLine(log);
                _streamWriter.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        #endregion

        #region CreateArchive
        /// <summary>
        /// 创建日志存档
        /// </summary>
        private void CreateArchive(string logFilePath)
        {
            string logDir = Path.GetDirectoryName(logFilePath);
            string fileName = Path.GetFileNameWithoutExtension(logFilePath);

            CloseStream(); //关闭日志写入流
            File.Move(logFilePath, Path.Combine(logDir, fileName + "_" + (++_currentArchiveIndex) + ".txt")); //存档
            CreateStream(logFilePath); //创建日志写入流
        }
        #endregion

        #region 生成日志文件路径
        /// <summary>
        /// 生成日志文件路径
        /// </summary>
        private string CreateLogPath(LogType logType, string currentDateStr)
        {
            try
            {
                return Path.Combine(_basePath, _rootFolder + "\\" + logType.ToString() + "\\", currentDateStr + ".txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                return null;
            }
        }
        #endregion

        #region UpdateLogFileName
        /// <summary>
        /// 更新日志文件名
        /// </summary>
        private void UpdateLogFileName(string currentDateStr)
        {
            //关闭日志写入流
            CloseStream();

            //创建新的日志路径
            string logFilePath = CreateLogPath(_logType, currentDateStr);
            string logDir = Path.GetDirectoryName(logFilePath);

            //创建日志写入流
            CreateStream(logFilePath);

            //初始化 _currentArchiveIndex
            InitCurrentArchiveIndex(logDir, currentDateStr);

            //初始化 _currentFileSize
            InitCurrentFileSize(logFilePath);
        }
        #endregion

        #region 写日志
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log">日志内容</param>
        public void WriteLog(string log)
        {
            _logs.Add(log);
        }
        #endregion

    }
}
