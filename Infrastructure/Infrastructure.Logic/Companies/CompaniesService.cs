using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Companies
{
    public class CompaniesService : ICompaniesService
    {
        private readonly ICommonRepository _commonRepository;

        public CompaniesService(ICommonRepository commonRepository)
        {
            _commonRepository = commonRepository;
        }

        public async Task<List<Company>> GetDataAsync()
            => (await _commonRepository.GetCompaniesAsync());

        public DateTimeOffset Clock()
            => DateTimeOffset.UtcNow;

    }
}
