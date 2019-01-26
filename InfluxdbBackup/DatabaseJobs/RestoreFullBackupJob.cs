using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfluxdbBackup.DatabaseJobs
{
    internal class RestoreFullBackupJob : IDatabaseJob
    {
        private IBackupMedium _backupMedium;
        private FileSystemHelper _fileSystemHelper = new FileSystemHelper();

        public RestoreFullBackupJob(IBackupMedium backupMedium)
        {
            this._backupMedium = backupMedium;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _fileSystemHelper.CreateDirectoryIfNotExists(ConfigurationManager.RestoreDirectory);
            _fileSystemHelper.RemoveFiles(ConfigurationManager.RestoreDirectory, "*");
            var latestbackupname = await _backupMedium.DownloadLatestBackupAsync(ConfigurationManager.RestoreDirectory);

            _fileSystemHelper.ExtractTarGZ(String.Concat(ConfigurationManager.RestoreDirectory, @"/", latestbackupname), ConfigurationManager.RestoreDirectory);

            try
            {
                InfluxDbCommandHelper.RestoreInfluxBackup(
                    Environment.GetEnvironmentVariable("INFLUXDB_HOST"),
                    int.Parse(Environment.GetEnvironmentVariable("INFLUXDB_PORT")),
                    Environment.GetEnvironmentVariable("INFLUXDB_DATABASE"),
                    ConfigurationManager.RestoreDirectory);
            }
            catch (System.Exception e)
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Critical, "RestoreJob failed: {0}", e.Message.ToString());
                throw e;
            }

            //cleanup here
            _fileSystemHelper.RemoveFiles(ConfigurationManager.RestoreDirectory, "*");
        }


        

        public void ValidateEnvironmentVariables()
        {
            //there are no specific environment variables required for a full influxdb restore so we'll just do a return here
            return;
        }
    }
}
