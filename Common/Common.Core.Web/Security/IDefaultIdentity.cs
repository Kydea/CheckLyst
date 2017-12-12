using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Web.Security
{
    interface IDefaultIdentity
    {
        long CompanyId { get; }

        string Code { get; }
    }
}
