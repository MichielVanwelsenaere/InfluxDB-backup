using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace InfluxdbBackup.BackupMedium
{
    internal class AzureBlob : IBackupMedium
    {
        private readonly BlobRequestOptions _resillientRequestOptions = new BlobRequestOptions()
        {
            RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(500), 5)
        };
        private readonly ILogger _logger;

        public AzureBlob(ILogger logger)
        {
            _logger = logger;
        }

        public async Task UploadBackupAsync(string fileName)
        {
            var storageCredentials = new StorageCredentials(
                Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_NAME"),
                Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_KEY")
            );

            //upload the backup to Azure blob
            try
            {
                _logger.Info("Attempting to upload backup to Azure, blobname: {0}", fileName);
                CloudBlobClient cloudBlobClient = new CloudStorageAccount(storageCredentials, useHttps: true).CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_CONTAINER"));
                await cloudBlobContainer.CreateIfNotExistsAsync(_resillientRequestOptions, null);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                await cloudBlockBlob.UploadFromFileAsync(fileName, null, _resillientRequestOptions, null);
                _logger.Info("Succesfully uploaded backup to Azure, blobname: {0}", fileName);
            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to upload backup to Azure! {0}", e.Message.ToString());
                throw e;
            }
        }

        public async Task<String> DownloadLatestBackupAsync(string DestinationDirectory)
        {
            var storageCredentials = new StorageCredentials(
                Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_NAME"),
                Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_KEY")
            );
            try
            {
                CloudBlobClient cloudBlobClient = new CloudStorageAccount(storageCredentials, useHttps: true).CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_CONTAINER"));

                BlobContinuationToken blobContinuationToken = null;
                BlobResultSegment results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, true, BlobListingDetails.All, 100, blobContinuationToken, _resillientRequestOptions, null);

                CloudBlob latestbackup = null;

                foreach (CloudBlob blob in results.Results)
                {
                    await blob.FetchAttributesAsync(null, _resillientRequestOptions, null);
                    if (latestbackup == null) latestbackup = blob;
                    if (blob.Properties.Created > latestbackup.Properties.Created)
                        latestbackup = blob;
                }

                _logger.Info("Found {0} backup(s) on Azure blob storage", results.Results.Count<IListBlobItem>());
                _logger.Info("Attempting to download latest backup {0} from Azure blob..", latestbackup.Name);
                await latestbackup.DownloadToFileAsync(String.Concat(DestinationDirectory, @"/", latestbackup.Name), System.IO.FileMode.CreateNew, null, _resillientRequestOptions, null);
                _logger.Info("Downloaded latest backup {0} succesfully from Azure blob!", latestbackup.Name);

                return String.Concat(DestinationDirectory, @"/", latestbackup.Name);

            }
            catch (Exception e)
            {
                _logger.Fatal("Failed to download latest backup from Azure blob storage: {0}", e.Message.ToString());
                throw e;
            }
        }

        public async Task RemoveOldBackupsAsync(int maximumAllowedBackups)
        {
            var storageCredentials = new StorageCredentials(
                Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_NAME"),
                Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_KEY")
            );

            //checks the number of backups on Azure and remove oldest if above maximum
            try
            {
                CloudBlobClient cloudBlobClient = new CloudStorageAccount(storageCredentials, useHttps: true).CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(Environment.GetEnvironmentVariable("AZURE_STORAGEACCOUNT_CONTAINER"));

                BlobContinuationToken blobContinuationToken = null;
                List<Tuple<Uri, DateTimeOffset?>> Containerblobs = new List<Tuple<Uri, DateTimeOffset?>>();
                BlobResultSegment results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, true, BlobListingDetails.All, 100, blobContinuationToken, _resillientRequestOptions, null);

                foreach (IListBlobItem item in results.Results)
                {
                    CloudBlockBlob blockblob = null;
                    try
                    {
                        blockblob = cloudBlobContainer.GetBlockBlobReference(new CloudBlockBlob(item.Uri).Name);
                        await blockblob.FetchAttributesAsync(null, _resillientRequestOptions, null);
                        Containerblobs.Add(new Tuple<Uri, DateTimeOffset?>(item.Uri, blockblob.Properties.Created));
                    }
                    catch (Exception e)
                    {
                        _logger.Warn("failed to fetch attribues for blob {0} with uri {1}: {2}", blockblob.Name, blockblob.StorageUri, e.Message.ToString());
                    }
                }

                _logger.Info("Found {0} backups on Azure blob storage", Containerblobs.Count);

                if (Containerblobs.Count > maximumAllowedBackups)
                {
                    _logger.Info("{0} backups on Azure blob storage exceed the maximum allowed number of backups {1}", Containerblobs.Count, maximumAllowedBackups);
                    var numberOfBackupsToRemove = Containerblobs.Count - maximumAllowedBackups;
                    _logger.Info("{0} backups need to be removed from Azure blob storage", numberOfBackupsToRemove);
                    IEnumerable<Tuple<Uri, DateTimeOffset?>> blobsToRemove = Containerblobs.OrderBy(x => x.Item2).Take(numberOfBackupsToRemove);

                    foreach (Tuple<Uri, DateTimeOffset?> blobItem in blobsToRemove)
                    {
                        _logger.Info("Attempting to remove backup blob {0} ...", blobItem.Item1.ToString());
                        var blockblob = cloudBlobContainer.GetBlockBlobReference(new CloudBlockBlob(blobItem.Item1).Name);
                        await blockblob.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, null, _resillientRequestOptions, null);
                        _logger.Info("backup blob {0} removed succesfully!", blobItem.Item1.ToString());
                    }
                }
                else
                {
                    _logger.Info("{0} backups on Azure blob storage does not exceed the maximum allowed number of backups {1}, no backups were removed...", Containerblobs.Count, maximumAllowedBackups);
                }
            }
            catch (Exception e)
            {
                _logger.Warn("Failed to remove old backups from Azure blob storage: {0}", e.Message.ToString());
                throw e;
            }
        }

        public void ValidateEnvironmentVariables()
        {
            ConfigurationHelper.VerifyEnvironmentVariable("AZURE_STORAGEACCOUNT_NAME", false);
            ConfigurationHelper.VerifyEnvironmentVariable("AZURE_STORAGEACCOUNT_KEY", false);
        }
    }
}