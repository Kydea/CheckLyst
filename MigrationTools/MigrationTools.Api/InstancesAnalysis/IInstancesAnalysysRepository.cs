using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Instances.Data.InstancesAnalysis
{
    public interface IInstancesAnalysisRepository
    {
        IQueryable<Common.Data.Instances> GetInstances();
    }
}