using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using static InfluxdbBackup.Helpers.SimpleConsoleLogger;

namespace InfluxdbBackup.Helpers
{
    static class InfluxDbCommandHelper
    {
        internal static void CreateInfluxBackup(string host, int port, string database, string destinationDirectory)
        {
            string cmd;

            if (database == "null")
            {
                //backs up all databases
                cmd = String.Format("influxd backup -portable -host {0}:{1} ./{2}", host, port, destinationDirectory);
            }
            else
            {
                //backs up a specific database
                cmd = String.Format("influxd backup -portable -database {0} -host {1}:{2} ./{3}", database, host, port, destinationDirectory);
            }
            RunShellCommand(cmd);
        }

        internal static void RestoreInfluxBackup(string host, int port, string database, string sourceDirectory)
        {
            string cmd = null;

            if (database == "null")
            {
                //restores all databases in sourcedirectory
                cmd = String.Format("influxd restore -portable -host {0}:{1} ./{2}", host, port, sourceDirectory);
            }
            else
            {
                //restores specific databases in sourcedirectory
                cmd = String.Format("influxd backup -portable -db {0} -host {1}:{2} ./{3}", database, host, port, sourceDirectory);                
            }
            RunShellCommand(cmd);
        }

        private static void RunShellCommand(string cmd)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{cmd}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardError.EndOfStream || !proc.StandardOutput.EndOfStream)
            {
                if (!proc.StandardError.EndOfStream)
                {
                    StripInfluxTimestampAndLog(SimpleConsoleLogger.LogLevel.Critical, proc.StandardError.ReadLine());
                }
                if (!proc.StandardOutput.EndOfStream)
                {
                    string log = proc.StandardOutput.ReadLine();
                    StripInfluxTimestampAndLog(SimpleConsoleLogger.LogLevel.Info, log);
                    if (log.Contains("(5)"))
                    {
                        proc.Kill();
                        throw new OperationCanceledException(String.Format("Failed to create Influxdb backup: {0}", log.Remove(0, 20)));
                    }
                }
            }
            proc.WaitForExit();
        }

        private static void StripInfluxTimestampAndLog(LogLevel logLevel, string influxdblog)
        {
            Regex rgx = new Regex(@"\d{4}\/\d{2}\/\d{2}");
            if (rgx.IsMatch(influxdblog))
            {
                SimpleConsoleLogger.Log(logLevel, influxdblog.Remove(0, 20));
            }else{
                SimpleConsoleLogger.Log(logLevel, influxdblog);
            }
        }
    }
}