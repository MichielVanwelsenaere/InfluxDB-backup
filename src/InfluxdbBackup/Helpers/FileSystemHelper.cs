using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace InfluxdbBackup.Helpers
{
    internal class FileSystemHelper
    {
        internal void CreateDirectoryIfNotExists(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        internal void RemoveDirectory(string dir)
        {
            DirectoryInfo di = new DirectoryInfo(dir);
            di.Delete(true);
        }

        internal void MoveFile(string sourceFileName, string destinationFileName)
        {
            File.Move(sourceFileName, destinationFileName);
        }

        internal void CopyFile(string sourceFileName, string destinationFileName)
        {
            File.Copy(sourceFileName, destinationFileName);
        }

        internal FileSystemInfo GetOldestFile(string directory)
        {
            FileSystemInfo fileInfo = new DirectoryInfo(directory).GetFileSystemInfos()
                .OrderBy(fi => fi.CreationTime).First();
            return fileInfo;
        }

        internal FileSystemInfo GetNewestFile(string directory)
        {
            FileSystemInfo fileInfo = new DirectoryInfo(directory).GetFileSystemInfos()
                .OrderBy(fi => fi.CreationTime).Last();
            return fileInfo;
        }

        internal int CountFilesInDirectory(string directory)
        {
            return Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Length;
        }

        internal void RemoveFile(string file)
        {
            File.Delete(file);
        }

        internal void RemoveFiles(string dir, string file)
        {
            string[] MatchingFiles = Directory.GetFiles(dir, file);

            foreach (string f in MatchingFiles)
            {
                File.Delete(f);
            }
        }

        internal void CreateZipFromDirectory(string zipFileName, string sourceDirectory)
        {
            ZipFile.CreateFromDirectory(sourceDirectory, String.Concat(zipFileName));
        }

        internal void ExtractZipToDirectory(string zipSourcePath, string destinationDirectory)
        {
            ZipFile.ExtractToDirectory(zipSourcePath, destinationDirectory);
        }
    }
}