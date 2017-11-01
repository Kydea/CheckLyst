using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data.ListItems
{
    public class PrototypeVersion
    {
        public int Version { get; set; }
        public string VersionState { get; set; }
        public string Description { get { return $"{Version} - {VersionState}"; } }

    }
}
