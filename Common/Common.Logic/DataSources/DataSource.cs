using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.DataSources
{
    public class DataSource
    {
        public string Class { get; set; }
        public string Code { get; set; }
        public string Password { private get; set; }
        public string Data { get; set; }
        public List<Parameter> Parameters { get; set; }
        public List<string> Columns { get; set; }
    }
}
