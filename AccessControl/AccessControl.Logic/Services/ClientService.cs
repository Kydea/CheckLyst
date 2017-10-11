using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Models;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.AccessControl.Logic.Services
{
	public class ClientService : IClientService
	{
		private const string RegionName = "Client";

		private readonly IRedisCache _cache;
		private readonly IHashService _hashService;

		public ClientService(IRedisCache cache, IHashService hashService)
		{
			_cache = cache;
			_hashService = hashService;
		}

		public async Task<string> CreateAsync(long companyId, string userCode, string token)
		{
			var counter = await _cache.IncrementAsync($"Counter.{companyId}", RegionName);
			var clientId = _hashService.HashStringToString($"{token}.{counter}");

			var client = new Client
			{
				Id = clientId,
				CompanyId = companyId,
				UserCode = userCode
			};

			await _cache.SetAsync($"Client.{clientId}", client, RegionName); //TODO add expiration date based on token expiration
																			 //TODO extend expiration when token is refresh (use SQS)

			return clientId;
		}
	}
}
