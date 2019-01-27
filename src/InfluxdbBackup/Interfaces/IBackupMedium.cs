using System;
using System.Collections.Generic;
using System.Text;
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
