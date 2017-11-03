using System;

namespace CheckListManager.Instances.Logic.List
{
    public class ListRow
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
