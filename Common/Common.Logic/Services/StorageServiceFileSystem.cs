using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using CheckListManager.Common.Logic.Models;

namespace CheckListManager.Common.Logic.Services
{
    public class StorageServiceFileSystem : IStorageService
    {
        public Task<StorageFile> GetAsync(string bucketName, string key)
        {
            var path = Path.Combine(bucketName, key);
            if (!File.Exists(path))
            {
                throw new Exception($"File {path} not found");
            }

            return Task.FromResult(new StorageFile(File.OpenRead(path), MimeMapping.GetMimeMapping(path)));
        }

        public async Task<bool> PutAsync(string bucketName, string key, Stream inputStream)
        {
            try
            {
                var path = Path.Combine(bucketName, key);
                using (var outputStream = File.OpenWrite(path))
                {
                    await inputStream.CopyToAsync(outputStream);
                    await outputStream.FlushAsync();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<bool> DeleteAsync(string bucketName, string key)
        {
            try
            {
                var path = Path.Combine(bucketName, key);
                var attributes = File.GetAttributes(path);

                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    Directory.Delete(path);
                }
                else
                {
                    File.Delete(path);
                }

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteRecursiveAsync(string bucketName, string keyPrefix)
            => DeleteAsync(bucketName, keyPrefix);

        public Task<bool> CloneAsync(string bucketName, string sourceKey, string destinationKey)
        {
            var sourcePath = Path.Combine(bucketName, sourceKey);
            var destinationPath = Path.Combine(bucketName, destinationKey);

            try
            {
                File.Copy(sourcePath, destinationPath);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}