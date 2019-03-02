using InfluxdbBackup.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InfluxdbBackup.Helpers;
using NLog;
using System.IO;

namespace InfluxdbBackup.BackupMedium
{
    class LocalDirectory : IBackupMedium
    {
        private FileSystemHelper _fileSystemHelper = new FileSystemHelper();
        private readonly string backupPersistanceDirectory = @"data/localbackups/";
        private readonly ILogger _logger;

        public LocalDirectory(ILogger logger)
        {
            _logger = logger;
        }

        public async Task UploadBackupAsync(string fileName)
        {
            try
            {
                _fileSystemHelper.CreateDirectoryIfNotExists(backupPersistanceDirectory);
                _logger.Info("Attempting to move backup to local data directory, filename: {0}", fileName);
                _fileSystemHelper.MoveFile(fileName, String.Concat(backupPersistanceDirectory, fileName));
                _logger.Info("Succesfully moved backup to local data directory, filename: {0}", fileName);
            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to move backup to local data directory: {0}", e.Message.ToString());
                throw e;
            }
        }

        public async Task<string> DownloadLatestBackupAsync(string destinationDirectory)
        {
            try
            {
                _logger.Info("Attempting to get latest backup from local data directory, filename");
                FileSystemInfo latestBackup = _fileSystemHelper.GetNewestFile(backupPersistanceDirectory);
                _logger.Info("Succesfully retrieved latest backup from local data directory, filename: {0}", latestBackup.Name);
                return latestBackup.FullName;
            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to get latest backup from local data directory: {0}", e.Message.ToString());
                throw e;
            }
        }

        public async Task RemoveOldBackupsAsync(int maximumAllowedBackups)
        {
            try
            {
                int localBackupCount = _fileSystemHelper.CountFilesInDirectory(backupPersistanceDirectory);
                _logger.Info("Found {0} backups in the local data directory", localBackupCount);
                if (localBackupCount > maximumAllowedBackups)
                {
                    int numberOfBackupsToRemove = localBackupCount - maximumAllowedBackups;
                    _logger.Info("{0} backups need to be removed from the local data directory", numberOfBackupsToRemove);

                    for (int i = 0; i < numberOfBackupsToRemove; i++)
                    {
                        FileSystemInfo oldFileToRemove = _fileSystemHelper.GetOldestFile(backupPersistanceDirectory);
                        _logger.Info("Attempting to remove backup file {0} ...", oldFileToRemove.Name);
                        _fileSystemHelper.RemoveFile(oldFileToRemove.FullName);
                        _logger.Info("backup file {0} removed succesfully!", oldFileToRemove.Name);
                    }                   
                }
                else
                {
                    _logger.Info("{0} backups on in the local data directory does not exceed the maximum allowed number of backups {1}, no backups were removed...", localBackupCount, maximumAllowedBackups);

                }
            }
            catch (Exception e)
            {
                _logger.Warn("Failed to remove old backups from the local data directory: {0}", e.Message.ToString());
                throw e;
            }            
        }        

        public void ValidateEnvironmentVariables()
        {
            //there are no specific environment variables required for keeping backups local so we'll just do a return here
            return;
        }
    }
}
