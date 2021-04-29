using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Companies
{
    public interface ICompaniesService : IService
    {
        Task<List<Company>> GetSyntheticDataAsync();
        Task<CompanySettingsResponse> GetSettingsAsync(long companyId);
        Task<CompaniesResponse> BindAsync(IDefaultIdentity identity, CompaniesRequest form);
        Task<PagingResponse<List<CompaniesRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<CompaniesRequest> form);
        Task<CompaniesResponse> Add(IDefaultIdentity identity, CompaniesRequest record);
        Task<CompaniesResponse> Edit(IDefaultIdentity identity, CompaniesRequest record);
        Task<CompaniesResponse> Delete(IDefaultIdentity identity, CompaniesRequest record);
    }
}
