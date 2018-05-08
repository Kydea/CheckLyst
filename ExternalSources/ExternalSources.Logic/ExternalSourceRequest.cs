using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//Warning: keep aligned with CheckListManager.Common.Logic.ExternalSources.DataRequest
namespace CheckListManager.ExternalSources.Logic
{
    public class ExternalSourceRequest
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
