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

        private FileStream _fileStream;

        private StreamWriter _streamWriter;

        private string _basePath;

        private int _fileSize = 10 * 1024 * 1024; //日志分隔文件大小

        private long _currentFileSize = 0;

        private BlockingCollection<string> _logs = new BlockingCollection<string>();

        private bool _run = true;

        private DateTime _lastFlushTime = DateTime.MinValue;

        private int _currentArchiveIndex = 0;
        #endregion

        #region LogWriter
        public LogWriter(LogType logType)
        {
            _logType = logType;

            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _basePath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

            Init();
        }
        #endregion

        #region Init
        private void Init()
        {
            //创建目录
            string logFilePath = CreateLogPath(_logType);
            string path = Path.GetDirectoryName(logFilePath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //初始化_currentArchiveIndex
            InitCurrentArchiveIndex(path);

            //创建或读取文件
            _fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8);

            //文件大小
            FileInfo fileInfo = new FileInfo(logFilePath);
            _currentFileSize = fileInfo.Length;

            //写日志线程
            Thread thread = null;
            thread = new Thread(new ThreadStart(() =>
            {
                string log;
                while (_run)
                {
                    if (_logs.TryTake(out log, Timeout.Infinite))
                    {
                        WriteFile(log, logFilePath);
                    }
                }
            }));
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region 初始化_currentArchiveIndex
        /// <summary>
        /// 初始化_currentArchiveIndex
        /// </summary>
        private void InitCurrentArchiveIndex(string pathFolder)
        {
            string strNow = DateTime.Now.ToString("yyyyMMdd");
            Regex regex = new Regex(strNow + "_*(\\d*).txt");
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
                    string path = Path.GetDirectoryName(logFilePath);
                    string name = Path.GetFileNameWithoutExtension(logFilePath);
                    _streamWriter.Close();
                    _fileStream.Close();
                    File.Move(logFilePath, Path.Combine(path, name + "_" + (++_currentArchiveIndex) + ".txt"));

                    //创建或读取文件
                    _fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8);
                }

                _streamWriter.WriteLine(log);
                _streamWriter.Flush();
                _fileStream.Flush();

                if (DateTime.Now.Subtract(_lastFlushTime).TotalMilliseconds > 100)
                {

                    _lastFlushTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        #endregion

        #region 生成日志文件路径
        /// <summary>
        /// 生成日志文件路径
        /// </summary>
        private string CreateLogPath(LogType logType)
        {
            try
            {
                string strNow = DateTime.Now.ToString("yyyyMMdd");
                return Path.Combine(_basePath, "Log\\" + logType.ToString() + "\\", strNow + ".txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                return null;
            }
        }
        #endregion

        #region 写日志
        public void WriteLog(string log)
        {
            _logs.Add(log);
        }
        #endregion

    }
}
