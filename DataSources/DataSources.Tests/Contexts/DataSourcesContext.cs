using CheckListManager.DataSources.Logic.Detail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Tests.Contexts
{
    public class DataSourcesContext
    {
        public DetailRequest Request { get; set; }

        public DetailResponse Response { get; set; }
    }
}
