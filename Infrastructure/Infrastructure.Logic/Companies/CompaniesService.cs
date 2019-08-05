using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.Infrastructure.Logic.Companies
{
    public class CompaniesService : ICompaniesService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly ISettingService _settingService;

        public CompaniesService(ICommonRepository commonRepository, ISettingService settingService)
        {
            _commonRepository = commonRepository;
            _settingService = settingService;
        }

        public async Task<List<Company>> GetDataAsync()
            => (await _commonRepository.GetCompaniesAsync());

        public async Task<CompanySettingsResponse> GetSettingsAsync(long companyId)
        {
            var response = new CompanySettingsResponse
            {
                RequireUserReason = await _settingService.RequireUserReasonAsync(companyId)
            };

            return response;
        }
    }
}
