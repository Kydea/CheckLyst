using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.ExternalSources
{
    public class DataRequest
    {
        public string code { get; set; }
        public Dictionary<string, string> parameters { get; set; }
        public int page { get; set; }
        public string className { get; set; }
        public string password { get; set; }
        public string data { get; set; }
        public DateTimeOffset version { get; set; }
    }
}
