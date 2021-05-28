using CheckListManager.Common.Core.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests.Data
{
    public interface IPrototypesRepositoryExtended
    {
        Task<Common.Data.Prototypes> AddProtoptypeAsync(IDefaultIdentity identity, Common.Data.Prototypes prototype);
    }
}
