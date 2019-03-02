using System;
using System.Threading.Tasks;
using Quartz;
using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using System.IO;

namespace InfluxdbBackup.DatabaseJobs
{

    internal class FullBackupJob : IDatabaseJob
    {
        private IBackupMedium _backupMedium;
        private FileSystemHelper _fileSystemHelper = new FileSystemHelper();

        public FullBackupJob(IBackupMedium backupMedium)
        {
            this._backupMedium = backupMedium;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _fileSystemHelper.CreateDirectoryIfNotExists(ConfigurationManager.BackupDirectory);

            try
            {
                InfluxDbCommandHelper.CreateInfluxBackup(
                    Environment.GetEnvironmentVariable("INFLUXDB_HOST"),
                    int.Parse(Environment.GetEnvironmentVariable("INFLUXDB_PORT")),
                    Environment.GetEnvironmentVariable("INFLUXDB_DATABASE"),
                    ConfigurationManager.BackupDirectory);
            }
            catch (System.Exception e)
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Critical, "Failed to create InfluxDb backup: {0}", e.Message.ToString());
                throw e;
            }


            string filename = Environment.GetEnvironmentVariable("BACKUP_FILENAME") + DateTime.Now.ToString("yyyyMMddHHmmss") + ".tar.gz";
            _fileSystemHelper.CreateTarGZ(filename, ConfigurationManager.BackupDirectory);

            await _backupMedium.UploadBackupAsync(filename);
            await _backupMedium.RemoveOldBackupsAsync(Int32.Parse(Environment.GetEnvironmentVariable("BACKUP_MAXBACKUPS")));

            //clean up
            _fileSystemHelper.RemoveDirectory(ConfigurationManager.BackupDirectory);
            _fileSystemHelper.RemoveFiles(Directory.GetCurrentDirectory(), "*.tar.gz");
        }


        public void ValidateEnvironmentVariables()
        {
            //there are no specific environment variables required for a full influxdb backup so we'll just do a return here
            return;
        }
    }
}