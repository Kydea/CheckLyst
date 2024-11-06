using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.REST
{
    public class RESTParameters
    {
        public string JSON { get; set; }
        public List<Parameter> QueryParameters { get; set; }

        public DateTimeOffset Version { get; set; }
    }
}
