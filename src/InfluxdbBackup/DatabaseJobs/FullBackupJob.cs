using System;
using System.Threading.Tasks;
using Quartz;
using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using System.IO;
using NLog;

namespace InfluxdbBackup.DatabaseJobs
{

    internal class FullBackupJob : IDatabaseJob
    {
        private IBackupMedium _backupMedium;
        private FileSystemHelper _fileSystemHelper = new FileSystemHelper();
        private readonly ILogger _logger;


        public FullBackupJob(IBackupMedium backupMedium, ILogger logger)
        {
            _backupMedium = backupMedium;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.Info("Cron triggered, executing database job...");
            try
            {
                _logger.Info("Validating database job specific environment variables");
                ValidateEnvironmentVariables();
                _logger.Info("Database job specific environment variables validated succesfully!");
            }
            catch (Exception e)
            {
                _logger.Fatal("Validating specific database Job environment variables failed: {0}", e.Message.ToString());
            }

            try
            {
                _fileSystemHelper.CreateDirectoryIfNotExists(ConfigurationHelper.BackupDirectory);

                try
                {
                    InfluxDbCommandHelper.CreateInfluxBackup(
                        Environment.GetEnvironmentVariable("INFLUXDB_HOST"),
                        int.Parse(Environment.GetEnvironmentVariable("INFLUXDB_PORT")),
                        Environment.GetEnvironmentVariable("INFLUXDB_DATABASE"),
                        ConfigurationHelper.BackupDirectory,
                        _logger);
                }
                catch (System.Exception e)
                {
                    throw e;
                }


                string filename = Environment.GetEnvironmentVariable("BACKUP_FILENAME") + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip";
                _fileSystemHelper.CreateZipFromDirectory(filename, ConfigurationHelper.BackupDirectory);

                await _backupMedium.UploadBackupAsync(filename);
                await _backupMedium.RemoveOldBackupsAsync(Int32.Parse(Environment.GetEnvironmentVariable("BACKUP_MAXBACKUPS")));

                //clean up
                _fileSystemHelper.RemoveDirectory(ConfigurationHelper.BackupDirectory);
                _fileSystemHelper.RemoveFiles(Directory.GetCurrentDirectory(), "*.zip");
                _logger.Info("Database job completed succesfully!");
            }
            catch (Exception e)
            {
                _logger.Fatal("Database job failed: {0}", e.Message.ToString());
            }
        }

        public void ValidateEnvironmentVariables()
        {
            //there are no specific environment variables required for a full influxdb backup so we'll just do a return here
            return;
        }
    }
}