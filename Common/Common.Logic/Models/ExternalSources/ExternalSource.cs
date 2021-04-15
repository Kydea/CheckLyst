using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.ExternalSources
{
    public class ExternalSource : DependencyNode
    {
        public override string DependencyClassName => "external source";
        public override string DependencyObjectCode => Code;
        public override bool Update(DependencyRelation.Properties Property) => throw new ArgumentException();
        public override void MarkNodeAsChanged(ICheckList CheckList, HashSet<Tuple<DependencyNode, DependencyRelation.Properties>> Roots, DependencyRelation.Properties Property) => throw new ArgumentException();
        public string Code { get; set; }
        public string DataSource { get; set; }
        public Dictionary<string, Parameter> Parameters { get; set; }
        [JsonIgnore]
        public Dictionary<string, string> Values { get; set; }
    }
}
