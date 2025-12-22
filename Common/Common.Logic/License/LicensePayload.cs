using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.License
{
    public class LicensePayload
    {
        public long Company { get; set; }
        public DateTime? Expiration { get; set; }
        public long? PrototypeTypesCount { get; set; }
        public long? PrototypesCount{ get; set; }
        public long? UsersCount { get; set; }
    }
}
