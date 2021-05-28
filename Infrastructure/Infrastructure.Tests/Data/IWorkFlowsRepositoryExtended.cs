using CheckListManager.Common.Core.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests.Data
{
    public interface IWorkFlowsRepositoryExtended
    {
        Task<Common.Data.WorkFlows> AddWorkFlowAsync(IDefaultIdentity identity, Common.Data.WorkFlows workFlow);
    }
}
