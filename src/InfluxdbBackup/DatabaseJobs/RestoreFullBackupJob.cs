using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using NLog;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InfluxdbBackup.DatabaseJobs
{
    internal class RestoreFullBackupJob : IDatabaseJob
    {
        private IBackupMedium _backupMedium;
        private FileSystemHelper _fileSystemHelper = new FileSystemHelper();
        private readonly ILogger _logger;

        public RestoreFullBackupJob(IBackupMedium backupMedium, ILogger logger)
        {
            this._backupMedium = backupMedium;
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
                _fileSystemHelper.CreateDirectoryIfNotExists(ConfigurationHelper.RestoreDirectory);
                _fileSystemHelper.RemoveFiles(ConfigurationHelper.RestoreDirectory, "*");
                var latestBackupFilepath = await _backupMedium.DownloadLatestBackupAsync(ConfigurationHelper.RestoreDirectory);

                _fileSystemHelper.ExtractZipToDirectory(latestBackupFilepath, ConfigurationHelper.RestoreDirectory);

                try
                {
                    InfluxDbCommandHelper.RestoreInfluxBackup(
                        Environment.GetEnvironmentVariable("INFLUXDB_HOST"),
                        int.Parse(Environment.GetEnvironmentVariable("INFLUXDB_PORT")),
                        Environment.GetEnvironmentVariable("INFLUXDB_DATABASE"),
                        ConfigurationHelper.RestoreDirectory,
                        _logger);
                }
                catch (System.Exception e)
                {
                    throw e;
                }

                //cleanup here
                _fileSystemHelper.RemoveFiles(ConfigurationHelper.RestoreDirectory, "*");
                _logger.Info("Database job completed succesfully!");
            }
            catch (Exception e)
            {
                _logger.Fatal("Database job failed: {0}", e.Message.ToString());
            }
        }

        public void ValidateEnvironmentVariables()
        {
            //there are no specific environment variables required for a full influxdb restore so we'll just do a return here
            return;
        }
    }
}