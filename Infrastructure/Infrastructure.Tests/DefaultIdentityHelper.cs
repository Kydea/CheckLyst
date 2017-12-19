using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests
{
    public class DefaultIdentityHelper : Repository, IDefaultIdentity
    {
        public DefaultIdentityHelper(RelationalDatabaseEntities entities) : base(entities)
        {
            Users user = Entities.Users.First();

            CompanyId = user.Company;
            Code = user.Code;
        }

        public long CompanyId { get; protected set; }

        public string Code { get; protected set; }
    }
}
