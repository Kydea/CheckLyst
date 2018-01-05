using CheckListManager.Common.Core.Services;
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
        Task<List<Company>> GetDataAsync();
        DateTimeOffset Clock();
    }
}
