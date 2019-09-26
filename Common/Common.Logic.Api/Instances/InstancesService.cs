using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Network;
using NLog;

namespace CheckListManager.Common.Logic.Api.Instances
{
    public class InstancesService : IInstancesService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceClient _serviceClient;

        public InstancesService(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }
        
        public async Task InvalidateCacheAsync(InvalidateCacheRequest request)
        {
            try
            {
                var response = await _serviceClient.PostAsync("Instances/Instance/InvalidateCache", request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    throw new Exception(responseData);
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to invalidate instance cache");
            }
        }
    }
}