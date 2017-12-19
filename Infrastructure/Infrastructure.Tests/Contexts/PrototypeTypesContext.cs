using CheckListManager.Infrastructure.Logic.PrototypeTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests.Contexts
{
    public class PrototypeTypesContext
    {
        public PrototypeTypesRequest Request { get; set; }

        public PrototypeTypesResponse Response { get; set; }

        public List<PrototypeTypesRow> Records { get; set; }
    }
}
