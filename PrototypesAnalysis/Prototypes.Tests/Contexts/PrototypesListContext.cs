using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Prototypes.Data.Models;
using CheckListManager.Prototypes.Logic.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Tests.Contexts
{
    public class PrototypesListContext
    {
        public PrototypeFilters Request { get; set; }

        public PagingResponse<List<PrototypeRecord>> Response { get; set; }
    }
}
