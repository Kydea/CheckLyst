using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.PrototypeTypes
{
    public class PrototypeTypesRow
    {
        public long Department { get; set; }
        public long ID { get; set; }
        public string Description { get; set; }
        public long? ApprovationWorkFlow { get; set; }
        public string ApprovationWorkFlowDescription { get; set; }
        public long? ObsolescenceWorkFlow { get; set; }
        public string ObsolescenceWorkFlowDescription { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
