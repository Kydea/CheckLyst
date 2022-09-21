using CheckListManager.Common.Data;
using CheckListManager.Instances.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CheckListManager.Instances.Data.InstancesAnalysis
{
    public class InstancesAnalysisRepository : Repository, IInstancesAnalysisRepository
    {
        public InstancesAnalysisRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public async Task ContinueAsync()
        {
            await Call<int>("\"Tools\".\"InstancesAnalysisContinue\"", null);
        }

        public async Task RestartAsync()
        {
            await Call<int>("\"Tools\".\"InstancesAnalysisRestart\"", null);
        }

        public async Task StopAsync()
        {
            await Call<int>("\"Tools\".\"InstancesAnalysisStop\"", null);
        }

        public async Task<InstancesAnalysisState> UpdateAsync()
        {
            var result = (await Read<InstancesAnalysisState>("select * from \"Tools\".\"InstancesAnalysis\" limit 1", null)).SingleOrDefault();
            result.Errors = (await Read<InstancesAnalysisState.Error>("select \"Key\", \"Message\" from \"Tools\".\"InstancesAnalysisLog\"", null)).ToList();
            return result;
        }
    }
}