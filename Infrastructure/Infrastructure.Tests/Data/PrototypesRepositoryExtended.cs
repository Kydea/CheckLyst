using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests.Data
{
    public class PrototypesRepositoryExtended : Repository, IPrototypesRepositoryExtended
    {
        public PrototypesRepositoryExtended(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public async Task<Prototypes> AddProtoptypeAsync(IDefaultIdentity identity, Prototypes prototype)
        {
            prototype.Company = identity.CompanyId;
            prototype.CreatedBy = identity.Code;
            prototype.CreatedOn = DateTime.Now;
            Entities.Prototypes.Add(prototype);
            await SaveChangesAsync();
            return prototype;
        }
    }
}
