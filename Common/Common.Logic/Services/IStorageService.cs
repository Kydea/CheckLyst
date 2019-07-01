using System.IO;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public interface IStorageService : IService
	{
		Task<Stream> GetAsync(string bucketName, string key);
		Task<bool> PutAsync(string bucketName, string key, Stream inputStream);
		Task<bool> DeleteAsync(string bucketName, string key);
		Task<bool> CloneAsync(string bucketName, string keyPrefix, string newKeyPrefix);
	}
}
