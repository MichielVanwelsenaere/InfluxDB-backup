using System;
using System.Text.RegularExpressions;

namespace InfluxdbBackup.Helpers
{
    static class ConfigurationHelper
    {
        internal static string BackupDirectory { get; } = "influxdb-backup";
        internal static string RestoreDirectory { get; } = "influxdb-restore";

        internal static void VerifyMinimalConfiguration()
        {
            VerifyEnvironmentVariable("INFLUXDB_ACTION", false);
            VerifyEnvironmentVariable("INFLUXDB_ACTION_CRON", false);
            VerifyEnvironmentVariable("INFLUXDB_BACKUPMEDIUM", false);
            VerifyEnvironmentVariable("INFLUXDB_DATABASE", true);
            VerifyEnvironmentVariable("INFLUXDB_HOST", false);
            VerifyEnvironmentVariable("INFLUXDB_PORT", false, new Regex(@"[0-9]"));


            VerifyEnvironmentVariable("BACKUP_MAXBACKUPS", true, new Regex(@"^\d{1,2}$"));
            VerifyEnvironmentVariable("BACKUP_FILENAME", true);
        }

        internal static void VerifyEnvironmentVariable(String environmentVariable, bool allowedNull)
        {
            string value = Environment.GetEnvironmentVariable(environmentVariable);

            if (value == "null" && allowedNull == false)
            {
                throw new ArgumentException(String.Format("Environment variable {0} is not specified", environmentVariable));
            }
        }

        internal static void VerifyEnvironmentVariable(string environmentVariable, bool allowedNull, Regex regexExpression)
        {
            VerifyEnvironmentVariable(environmentVariable, allowedNull);

            string value = Environment.GetEnvironmentVariable(environmentVariable);
            if (!regexExpression.IsMatch(value))
            {
                throw new ArgumentException(String.Format("Environment variable {0} did not match the expected regex {1}", environmentVariable, regexExpression.ToString()));
            }
        }
    }
}