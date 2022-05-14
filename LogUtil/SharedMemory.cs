using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// 共享内存
    /// </summary>
    public class SharedMemory
    {
        private string _sharedMemoryFileName;
        private MemoryMappedFile _file;
        private MemoryMappedViewAccessor _accessor;
        private bool _hasValue = false;

        public SharedMemory(LogType logType)
        {
            _sharedMemoryFileName = "LogWriterUseMutex.SharedMemory." + logType.ToString() + ".7693FFAD38004F6B8FD31F6A8B4CE2BD";

            try
            {
                _file = MemoryMappedFile.CreateOrOpen(_sharedMemoryFileName, 10);
                _accessor = _file.CreateViewAccessor();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void Write(long currentFileSize)
        {
            _accessor.Write(0, currentFileSize);
            _accessor.Flush();
        }

        public long Read()
        {
            return _accessor.ReadInt64(0);
        }
    }
}
