using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Prototypes.Data.PrototypesAnalysis
{
    public interface IPrototypesAnalysisRepository
    {
        IQueryable<PrototypeVersions> GetPrototypeVersions();
    }
}