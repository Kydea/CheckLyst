using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.DataSources
{
    public class DataRequest
    {
        public string code { get; set; }
        public Dictionary<string, string> parameters { get; set; }
        public int page { get; set; }
    }
}
