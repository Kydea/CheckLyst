using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Prototypes.Data.Analysis
{
    public interface IAnalysisRepository
    {
        IQueryable<PrototypeVersions> GetPrototypeVersions();
    }
}