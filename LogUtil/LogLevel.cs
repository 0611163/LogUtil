using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    /// <summary>
    /// 日志级别
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        Debug = 1,

        Info = 2,

        Error = 4
    }
}
