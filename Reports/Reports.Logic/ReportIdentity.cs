using CheckListManager.Common.Core.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic
{
    public class ReportIdentity : IDefaultIdentity
    {
        public long CompanyId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool HasPermission(string functionGroup, string function = "", long? department = null, long? prototypeType = null, string tag = null) => true;
    }
}
