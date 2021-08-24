using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Prototypes.Data.Analysis
{
    public class AnalysisRepository: Repository, IAnalysisRepository
    {
        public AnalysisRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public IQueryable<PrototypeVersions> GetPrototypeVersions()
        {
            return Entities.PrototypeVersions;
        }
    }
}