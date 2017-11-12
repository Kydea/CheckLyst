using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Services
{
	public class InstanceService : IInstanceService
	{
		private readonly IServiceClient _client;
		private readonly IHashService _hashService;
		private readonly CheckListJsonConverter _converter;

		public InstanceService(IServiceClient client, IHashService hashService, CheckListJsonConverter converter)
		{
			_client = client;
			_hashService = hashService;
			_converter = converter;
		}

		public async Task<CreateInstanceResponse> Create(int prototype, int version)
		{
			var instance = await CreateInstanceFromPrototype(prototype, version);
			var response = new CreateInstanceResponse
			{
				Code = _hashService.HashStringToString($"{prototype}-{version}-{Guid.NewGuid()}"),
				CheckList = instance
			};

			return response;
		}

		private async Task<Instance> CreateInstanceFromPrototype(int prototype, int version)
		{
			//TODO: error handling
			var httpResponse = await _client.GetAsync($"Prototypes/Prototype/GetAsync/{prototype}/{version}");
			var responseData = await httpResponse.Content.ReadAsStringAsync();

			var response = JsonConvert.DeserializeObject<ResponseMessage<GetPrototypeResponse>>(responseData, _converter);
			
			return response.Data.CheckList;
		}
	}
}