using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Instances.Data.InstancesAnalysis
{
    public class InstancesAnalysisRepository : Repository, IInstancesAnalysisRepository
    {
        public InstancesAnalysisRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public IQueryable<Common.Data.Instances> GetInstances()
        {
            return Entities.Instances;
        }
    }
}