using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.Prototypes.Logic.Archiving
{
    public class ArchivingRequest
    {
        public int Company { get; set; }
        public bool AuditTrail { get; set; }
        public bool Instances { get; set; }
    }
}
