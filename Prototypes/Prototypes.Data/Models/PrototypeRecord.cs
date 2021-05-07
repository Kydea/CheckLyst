using System;

namespace CheckListManager.Prototypes.Data.Models
{
    public class PrototypeRecord
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Version { get; set; }
        public DateTimeOffset? ValidityFrom { get; set; }
        public DateTimeOffset? ValidityUntil { get; set; }
        public long Department { get; set; }
        public string DepartmentName { get; set; }
        public long PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string State { get; set; }
    }
}
