using CheckListManager.WorkFlows.Logic.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Tests.Data
{
    public interface IPrototypeTypesRepositoryExtended
    {
        void Add(long CompanyId, Common.Data.PrototypeTypes prototypeTypes);
    }
}
