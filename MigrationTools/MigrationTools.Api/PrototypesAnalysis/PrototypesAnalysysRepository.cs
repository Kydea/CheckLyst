using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Prototypes.Data.PrototypesAnalysis
{
    public class PrototypesAnalysisRepository : Repository, IPrototypesAnalysisRepository
    {
        public PrototypesAnalysisRepository (RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public IQueryable<PrototypeVersions> GetPrototypeVersions()
        {
            return Entities.PrototypeVersions;
        }
    }
}