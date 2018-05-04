using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic
{
    public class ExternalSourceRequest
    {
        public string DataSourceCode { get; set; }
        public string DataSourceClass { get; set; }
        public string Password { get; set; }
        public string DataSourceParameters { get; set; }
        public DateTimeOffset Version { get; set; }
    }
}
