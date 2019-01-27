using System;
using System.Collections.Generic;
using System.Text;

namespace InfluxdbBackup.Helpers
{
    static class SimpleConsoleLogger
    {
        internal enum LogLevel
        {
            Critical = 5,
            Error = 4,
            Warning = 3,
            Info = 2,
            Debug = 1,
            Trace = 0
        }

        internal static void Log(LogLevel severity ,string format, params object[] arg)
        {
            string prefix = string.Format("{0:yyyy-MM-dd HH:mm:ss.ffff} [{1}]", DateTime.Now, severity.ToString());
            string rendered = string.Format(format, arg);
            Console.WriteLine("{0} {1}", prefix, rendered);
        }

    }
}
