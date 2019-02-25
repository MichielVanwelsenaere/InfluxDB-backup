using System;
using System.Threading.Tasks;

namespace InfluxdbBackup.Interfaces
{
    public interface IBackupMedium
    {
        Task UploadBackupAsync(string fileName);
        Task<String> DownloadLatestBackupAsync(string destinationDirectory);
        Task RemoveOldBackupsAsync(int maximumAllowedBackups);
        void ValidateEnvironmentVariables();
    }
}
