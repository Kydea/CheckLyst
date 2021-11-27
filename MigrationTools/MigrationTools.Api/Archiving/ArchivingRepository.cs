using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Prototypes.Data.Archiving
{
    public class ArchivingRepository : Repository, IArchivingRepository
    {
        public ArchivingRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public IQueryable<Common.Data.Instances> GetInstances(int Company)
        {
            return Entities.Instances.Where(x => x.Company == Company).OrderBy(x => x.ID);
        }

        public IQueryable<Records> GetTrailRecords(int Company)
        {
            return Entities.Records.Where(x => x.Company == Company).OrderBy(x => x.ID);
        }
    }
}