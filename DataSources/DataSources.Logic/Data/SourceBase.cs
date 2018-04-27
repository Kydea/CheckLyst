using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Data
{
    public abstract class SourceBase
    {
        public abstract void Parse(string Data);

        public List<string> Columns { get; protected set; }

        public List<Parameter> Parameters { get; protected set; }
    }
}
