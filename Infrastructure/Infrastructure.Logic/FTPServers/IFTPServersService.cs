using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.FTPServers
{
    public interface IFTPServersService : IService
    {
        Task<PagingResponse<List<FTPServersRow>>> GetDataAsync(IDefaultIdentity Identity, PagingRequest<FTPServersRequest> form);

        Task<FTPServersResponse> AddOrEdit(IDefaultIdentity Identity, FTPServersRow record, bool IsNew);

        Task<FTPServersResponse> Delete(IDefaultIdentity Identity, FTPServersRow record);

        Task<string> Test(IDefaultIdentity Identity, long ID);
    }
}
