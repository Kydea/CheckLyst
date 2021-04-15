using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Models.ExternalSources
{
    public class Parameter
    {
        public string Code { get; set; }
        public DataType ValueType { get; set; }
        public string ExpressioCode { get; set; }
        public string ElementCode { get; set; }
        public bool ReadOnly { get; set; }
    }
}
