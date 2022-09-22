using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal.InstancesAnalysis
{
    public class InstancesAnalysisRepository : Repository, IInstancesAnalysisRepository
    {
        public InstancesAnalysisRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public async Task<IEnumerable<InstanceListItem>> GetInstancesAsync()
        {
            return await Read<InstanceListItem>(
                "select i.\"ID\", i.\"Code\", i.\"Company\", i.\"PrototypeName\", i.\"Version\", i.\"Year\", i.\"Number\", i.\"InstanceState\" " + Environment.NewLine +
                "from \"CheckLists\".\"Instances\" i " + Environment.NewLine +
                "where i.\"ToolState\" is null " + Environment.NewLine +
                "order by i.\"Company\", i.\"Prototype\", i.\"Version\", i.\"Year\", i.\"Number\", i.\"ID\" " + Environment.NewLine +
                $"limit {int.Parse(ConfigurationManager.AppSettings["InstancesAnalysisRecordLimit"])}", null);
        }

        public async Task<InstancesAnalysisState> GetStateAsync()
        {
            var result = (await Read<InstancesAnalysisState>("select * from \"Tools\".\"InstancesAnalysis\" limit 1", null)).SingleOrDefault();
            return result;
        }

        public async Task StopAsync()
        {
            await Call<int>("\"Tools\".\"InstancesAnalysisStop\"", null);
        }

        public async Task OkAsync(long instance)
        {
            await Call<int>("\"Tools\".\"InstancesAnalysisOk\"", new Parameter("_Instance", instance));
        }

        public async Task ErrorAsync(string key, string error, long instance)
        {
            await Call<int>("\"Tools\".\"InstancesAnalysisError\"", new Parameter ("_Key", key), new Parameter("_Message", error), new Parameter("_Instance", instance));
        }
    }
}