using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public class ListRow
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Version { get; set; }
        public DateTimeOffset? ValidityFrom { get; set; }
        public DateTimeOffset? ValidityUntil { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string State { get; set; }
    }
}
