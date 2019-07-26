using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using CheckListManager.Common.Logic.Models;

namespace CheckListManager.Common.Logic.Services
{
    public class StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3;

        public StorageService(IAmazonS3 s3)
        {
            _s3 = s3;
        }

        public async Task<StorageFile> GetAsync(string bucketName, string key)
        {
            var response = await _s3.GetObjectAsync(bucketName, key);
            if (response.HttpStatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception($"File {key} not found");
            }

            return new StorageFile(response.ResponseStream, response.Headers.ContentType);
        }

        public async Task<bool> PutAsync(string bucketName, string key, Stream inputStream)
        {
            var response = await _s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = inputStream
            });

            return response.HttpStatusCode == HttpStatusCode.OK || response.HttpStatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteAsync(string bucketName, string key)
        {
            var response = await _s3.DeleteObjectAsync(bucketName, key);
            return response.HttpStatusCode == HttpStatusCode.OK || response.HttpStatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteRecursiveAsync(string bucketName, string keyPrefix)
        {
            return await IterateAsync(bucketName, keyPrefix, async itemKey => await DeleteAsync(bucketName, itemKey));
        }

        public async Task<bool> CloneAsync(string bucketName, string keyPrefix, string newKeyPrefix)
        {
            return await IterateAsync(bucketName, keyPrefix, async itemKey =>
            {
                var key = $"{newKeyPrefix}{itemKey.Replace(keyPrefix, "")}";
                var response = await _s3.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    DestinationBucket = bucketName,
                    SourceKey = itemKey,
                    DestinationKey = key
                });

                return response.HttpStatusCode == HttpStatusCode.OK;
            });
        }

        private async Task<bool> IterateAsync(string bucketName, string keyPrefix, Func<string, Task<bool>> action)
        {
            string continuationToken = null;
            ListObjectsV2Response response;
            do
            {
                response = await _s3.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = keyPrefix,
                    ContinuationToken = continuationToken
                });

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                foreach (var item in response.S3Objects)
                {
                    var result = await action(item.Key);
                    if (!result)
                    {
                        return false;
                    }
                }

                continuationToken = response.ContinuationToken;
            } while (response.IsTruncated);

            return true;
        }
    }
}