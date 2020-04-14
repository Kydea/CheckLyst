using CheckListManager.Common.Logic.Dependency;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class ExternalSourceDependencyNode : DependencyNode
    {
        public override string DependencyClassName => "external source";

        public override string DependencyObjectCode => Code;

        public override bool Update(DependencyRelation.Properties Property)
        {
            if (Property != DependencyRelation.Properties.Value) throw new ArgumentException();
            return true;
        }

        public string Code { get; set; }
    }
}
