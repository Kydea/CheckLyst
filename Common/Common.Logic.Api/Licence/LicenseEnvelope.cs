using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.Licence
{
    public class LicenseEnvelope
    {
        public LicensePayload Payload { get; set; }
        public string Signature { get; set; }
    }
}
