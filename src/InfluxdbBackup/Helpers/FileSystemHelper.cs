using System;
using System.IO;
using System.IO.Compression;

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

        internal void RemoveFiles(string dir, string file)
        {
            string[] MatchingFiles = Directory.GetFiles(dir, file);

            foreach(string f in MatchingFiles)
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