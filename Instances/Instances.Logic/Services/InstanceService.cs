using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Services
{
	public class InstanceService : IInstanceService
	{
		private readonly IServiceClient _client;
		private readonly IHashService _hashService;

		public InstanceService(IServiceClient client, IHashService hashService)
		{
			_client = client;
			_hashService = hashService;
		}

		public async Task<CreateInstanceResponse> Create(int prototype, int version)
		{
			var instance = await CreateInstanceFromPrototype(prototype, version);
			var response = new CreateInstanceResponse
			{
				Code = _hashService.HashStringToString($"{prototype}-{version}-{Guid.NewGuid()}"),
				Instance = instance
			};

			return response;
		}

		private async Task<Instance> CreateInstanceFromPrototype(int prototype, int version)
		{
			//TODO: error handling
			var instance = await _client.Get<Instance>($"Prototypes/Prototype/{prototype}/{version}");
			return instance;
		}
	}
}