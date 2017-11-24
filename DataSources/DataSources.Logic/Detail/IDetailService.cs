using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Detail
{
    public interface IDetailService: IService
    {
        Task<DetailResponse> Add(DefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> Edit(DefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> Delete(DefaultIdentity identity, string code);
    }
}
