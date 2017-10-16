using CheckListManager.Common.Core.Web.ClassExtensions;
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
        public string TimeZone { get; set; }

        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Version { get; set; }

        private DateTimeOffset? _ValidityFrom;
        public DateTimeOffset? ValidityFrom { get { return _ValidityFrom?.ToTimeZone(TimeZone); } set { _ValidityFrom = value; } }

        private DateTimeOffset? _ValidityUntil;
        public DateTimeOffset? ValidityUntil { get { return _ValidityUntil?.ToTimeZone(TimeZone); } set { _ValidityUntil = value; } }

        public string CreatedBy { get; set; }

        private DateTimeOffset _CreatedOn;
        public DateTimeOffset CreatedOn { get { return _CreatedOn.ToTimeZone(TimeZone); } set { _CreatedOn = value; } }

        public string ModifiedBy { get; set; }

        private DateTimeOffset? _ModifiedOn;
        public DateTimeOffset? ModifiedOn { get { return _ModifiedOn?.ToTimeZone(TimeZone); } set { _ModifiedOn = value; } }

        public string State { get; set; }
    }
}
