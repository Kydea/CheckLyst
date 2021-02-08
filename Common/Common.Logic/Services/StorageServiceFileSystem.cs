using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Web;
using CheckListManager.Common.Logic.Models;
using NLog;

namespace CheckListManager.Common.Logic.Services
{
    public class StorageServiceFileSystem : IStorageService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public async Task<StorageFile> GetAsync(string bucketName, string key)
        {
            var path = Path.Combine(bucketName, key);
            if (!File.Exists(path))
            {
                throw new Exception($"File {path} not found");
            }

            var ms = new MemoryStream();
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                await fileStream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
            }
            return new StorageFile(ms, MimeMapping.GetMimeMapping(path));
        }

        public async Task<bool> PutAsync(string bucketName, string key, Stream inputStream)
        {
            try
            {
                var path = Path.Combine(bucketName, key);
                var directory = Path.GetDirectoryName(path);
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                using (var outputStream = File.OpenWrite(path))
                {
                    await inputStream.CopyToAsync(outputStream);
                    await outputStream.FlushAsync();
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error uploading file to {bucketName} {key}");
                return false;
            }
        }

        public Task<bool> DeleteAsync(string bucketName, string key)
        {
            try
            {
                var path = Path.Combine(bucketName, key);
                if (File.Exists(path))
                {
                    var attributes = File.GetAttributes(path);

                    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        Directory.Delete(path);
                    else
                        File.Delete(path);
                }
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error deleting file {bucketName} {key}");
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteRecursiveAsync(string bucketName, string keyPrefix)
            => DeleteAsync(bucketName, keyPrefix);

        public Task<bool> CloneAsync(string bucketName, string sourceKey, string destinationKey)
        {
            var sourcePath = Path.Combine(bucketName, sourceKey);
            var destinationPath = Path.Combine(bucketName, destinationKey);
            
            var directory = Path.GetDirectoryName(destinationPath);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            try
            {
                File.Copy(sourcePath, destinationPath);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error cloning {bucketName} {sourceKey} to {destinationKey}");
                return Task.FromResult(false);
            }
        }
    }
}