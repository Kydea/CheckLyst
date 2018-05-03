using System;
using System.Collections.Generic;

namespace CheckListManager.DataSources.Logic.Data
{
    public class DataSource
    {
        public string Class { get; set; }
        public string Code { get; set; }
        public string Password { private get; set; }
        public string Data { get; set; }
    }
}
