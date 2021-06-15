using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Data.Models
{
    public class InstanceRelation
    {
        public enum RelationTypes { Parent, Child };

        public int Item { get; set; }
        public long? ID { get; set; }
        public string Prototype { get; set; }
        public int Version { get; set; }
        public int? Year { get; set; }
        public int? Number { get; set; }
        public string VersionState { get; set; }
        public string Description { get; set; }
        public RelationTypes RelationType { get; set; }
        public string Code { get; set; }
        public string InstanceCode { get; set; }

    }
}
