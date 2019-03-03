using NLog;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace InfluxdbBackup.Helpers
{
    static class InfluxDbCommandHelper
    {
        internal static void CreateInfluxBackup(string host, int port, string database, string destinationDirectory, ILogger logger)
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
            RunShellCommand(cmd, logger);
        }

        internal static void RestoreInfluxBackup(string host, int port, string database, string sourceDirectory, ILogger logger)
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
            RunShellCommand(cmd, logger);
        }

        private static void RunShellCommand(string cmd, ILogger logger)
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
                    StripInfluxTimestampAndLog(LogLevel.Fatal, proc.StandardError.ReadLine(), logger);
                }
                if (!proc.StandardOutput.EndOfStream)
                {
                    string log = proc.StandardOutput.ReadLine();
                    StripInfluxTimestampAndLog(LogLevel.Info, log, logger);
                    if (log.Contains("(5)"))
                    {
                        proc.Kill();
                        throw new OperationCanceledException(String.Format("Failed to create Influxdb backup: {0}", log.Remove(0, 20)));
                    }
                }
            }
            proc.WaitForExit();
        }

        private static void StripInfluxTimestampAndLog(NLog.LogLevel logLevel, string influxdblog, ILogger logger)
        {
            Regex rgx = new Regex(@"\d{4}\/\d{2}\/\d{2}");
            if (rgx.IsMatch(influxdblog))
            {
                logger.Log(logLevel, influxdblog.Remove(0, 20));
            }
            else
            {
                logger.Log(logLevel, influxdblog);
            }
        }
    }
}