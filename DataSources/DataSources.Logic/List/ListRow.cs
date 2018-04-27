using System;

namespace CheckListManager.DataSources.Logic.List
{
    public class ListRow
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Class { get; set; }
        public string Parameters { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
