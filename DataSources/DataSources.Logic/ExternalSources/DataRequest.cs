using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.ExternalSources
{
    public class DataRequest
    {
        public string code { get; set; }

        //read from external source
        public Dictionary<string, string> parameters { get; set; }

        //export to an external source
        public List<string> names { get; set; }
        public List<List<string>> records { get; set; }

        public int page { get; set; }
        public string className { get; set; }
        public string password { get; set; }
        public string data { get; set; }
        public DateTimeOffset version { get; set; }
        public string decimalSeparator { get; set; }
        public string timeZone { get; set; }
    }
}
