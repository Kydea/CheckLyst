using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.Licence
{
    public class LicensePayload
    {
        public DateTimeOffset? Expiration { get; set; }
        public long? UsersCount { get; set; }
        public long? PrototypeTypesCount { get; set; }
        public long? ProtoypesCount{ get; set; }
    }
}
