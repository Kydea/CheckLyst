using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Data.Models
{
    public class TasksRecord
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string InstanceCode { get { return $"{PrototypeName}.{Version}.{Year}.{Number}"; } }
        public string Year { get; set; }
        public int Number { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string DepartmentName { get; set; }
        public string PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public long Prototype { get; set; }
        public string PrototypeName { get; set; }
        public int Version { get; set; }
        public string TaskDescription { get; set; }
        public string TaskCode { get; set; }
        public string TaskValue { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}
