using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.SQL
{
    public class SQLParameters
    {
        public string Host { get; set; }
        public string DataBase { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Query { get; set; }

        public DateTimeOffset Version { get; set; }
    }
}
