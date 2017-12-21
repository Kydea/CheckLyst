using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.WorkFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Applications
{
    public interface IApplicationService : IService
    {
        Task<GetStepsResponse> GetStepsAsync(DefaultIdentity identity, GetStepsRequest request);
        Task<CreateResponse> CreateAsync(DefaultIdentity identity, CreateRequest request);
        Task<SignResponse> SignAsync(DefaultIdentity identity, SignRequest request);
    }
}
