using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.AccessControl.Data.Models
{
    public class ExportRow
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string CorporateID { get; set; }
        public bool Enabled { get; set; }
        public DateTimeOffset? LastLoginOn { get; set; }
        public string Group { get; set; }
        public long? Department { get; set; }
        public string DepartmentName { get; set; }
        public long? PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public string Role { get; set; }
        public string FunctionGroup { get; set; }
        public string Function { get; set; }
        public string SignatureLevel { get; set; }
        public string PrototypeWorkFlowLevel { get; set; }
        public string InstanceWorkFlowLevel { get; set; }
    }
}
