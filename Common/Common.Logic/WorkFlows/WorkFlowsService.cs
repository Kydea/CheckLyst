using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Network;
using Newtonsoft.Json;
using System.Net.Http;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Common.Logic.WorkFlows
{
	public class WorkFlowsService : IWorkFlowsService
    {
        private IServiceClient _client;

        public WorkFlowsService(IServiceClient client)
        {
            _client = client;
        }

        public async Task<CreateResponse> CreateWorkFlowApplicationAsync(IDefaultIdentity identity, CreateRequest request)
        {
            try
            {
                var response = await _client.PostAsync("WorkFlows/Applications/Create", request);
                var responseData = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception(responseData);
                return JsonConvert.DeserializeObject<CreateResponse>(responseData);
            }
            catch (Exception e)
            {
                throw new Exception("Workflow application creation failed", e);
            }
        }
    }
}
