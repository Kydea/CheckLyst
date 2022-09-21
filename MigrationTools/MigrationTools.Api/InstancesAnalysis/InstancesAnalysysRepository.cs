using CheckListManager.Common.Data;
using CheckListManager.Instances.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CheckListManager.Instances.Data.InstancesAnalysis
{
    public class InstancesAnalysisRepository : Repository, IInstancesAnalysisRepository
    {
        public InstancesAnalysisRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public async Task<IEnumerable<InstanceListItem>> GetInstances(string ToolState)
        {
            if (ToolState != null) return new List<InstanceListItem>(0);
            return await Read<InstanceListItem>(
                "select i.\"Code\", i.\"Company\", i.\"PrototypeName\", i.\"Version\", i.\"Year\", i.\"Number\", i.\"InstanceState\" " + Environment.NewLine +
                "from \"CheckLists\".\"Instances\" i " + Environment.NewLine +
                //$"where {(ToolState == null ? "i.\"ToolState\" is null" : $"not i.\"ToolState\" = '{ToolState}'")} " + Environment.NewLine +
                "order by i.\"Company\", i.\"Prototype\", i.\"Version\", i.\"Year\", i.\"Number\", i.\"ID\" ", null);
        }
    }
}