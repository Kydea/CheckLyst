using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Instances.Logic.Models.CheckLists;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Prototypes.Api
{
    public class DefaultIdentityHelper : /*Repository, */IDefaultIdentity
    {
        public DefaultIdentityHelper(/*RelationalDatabaseEntities entities, */long company) //: base(entities)
        {
            //Users user = Entities.Users.Where(u => u.Company == company).First();

            CompanyId = company;
            Code = "CheckListManager";
        }

        public long CompanyId { get; }

        public string Code { get; set; }

        public string Description => throw new NotImplementedException();

        public bool HasPermission(string functionGroup, string function = "", long? department = null, long? prototypeType = null, string tag = null)
        {
            throw new NotImplementedException();
        }
    }
}