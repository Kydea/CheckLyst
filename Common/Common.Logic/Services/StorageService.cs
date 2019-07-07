using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace CheckListManager.Common.Logic.Services
{
	public class StorageService : IStorageService
	{
		private readonly IAmazonS3 _s3;

		public StorageService(IAmazonS3 s3)
		{
			_s3 = s3;
		}

		public async Task<Stream> GetAsync(string bucketName, string key)
		{
			var response = await _s3.GetObjectAsync(bucketName, key);
			if (response.HttpStatusCode == HttpStatusCode.NotFound)
			{
				throw new Exception($"File {key} not found");
			}

			return response.ResponseStream;
		}

		public async Task<bool> PutAsync(string bucketName, string key, Stream inputStream)
		{
			var response = await _s3.PutObjectAsync(new PutObjectRequest
			{
				BucketName = bucketName,
				Key = key,
				InputStream = inputStream
			});

			return response.HttpStatusCode == HttpStatusCode.OK;
		}

		public async Task<bool> DeleteAsync(string bucketName, string key)
		{
			var response = await _s3.DeleteObjectAsync(bucketName, key);
			return response.HttpStatusCode == HttpStatusCode.OK;
		}

		public async Task<bool> CloneAsync(string bucketName, string keyPrefix, string newKeyPrefix)
		{
			var response = await _s3.ListObjectsV2Async(new ListObjectsV2Request
			{
				BucketName = bucketName,
				Prefix = keyPrefix
			});

			if (response.HttpStatusCode != HttpStatusCode.OK)
			{
				return false;
			}

			do
			{
				foreach (var item in response.S3Objects)
				{
					var key = $"{newKeyPrefix}{item.Key.Replace(keyPrefix, "")}";
					var copyResponse = await _s3.CopyObjectAsync(new CopyObjectRequest
					{
						SourceBucket = bucketName,
						DestinationBucket = bucketName,
						SourceKey = item.Key,
						DestinationKey = key
					});

					if (copyResponse.HttpStatusCode != HttpStatusCode.OK)
					{
						return false;
					}
				}

				response = await _s3.ListObjectsV2Async(new ListObjectsV2Request
				{
					BucketName = bucketName,
					Prefix = keyPrefix,
					ContinuationToken = response.NextContinuationToken
				});
				if (response.HttpStatusCode != HttpStatusCode.OK)
				{
					return false;
				}
			} while (response.IsTruncated);

			return true;
		}
	}
}