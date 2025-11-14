using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.License
{
    public class LicenseModel
    {
        public LicensePayload Payload{ get; set; }
        public string Error { get; set; }
        public bool IsValid => Error == null;
    }
}
