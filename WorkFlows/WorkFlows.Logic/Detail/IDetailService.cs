using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.Detail
{
    public interface IDetailService : IService
    {
        Task<DetailResponse> LoadAsync(IDefaultIdentity identity, DetailRequest record);
        Task<DetailResponse> AddAsync(IDefaultIdentity identity, DetailRequest record);
        Task<DetailResponse> EditAsync(IDefaultIdentity identity, DetailRequest record);
        Task<DetailResponse> DeleteAsync(IDefaultIdentity identity, DetailRequest record);
    }
}
