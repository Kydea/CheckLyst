using CheckListManager.Common.Data;
using CheckListManager.WorkFlows.Logic.List;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Tests.Data
{
    public class PrototypeTypesRepositoryExtended : Repository, IPrototypeTypesRepositoryExtended
    {
        public PrototypeTypesRepositoryExtended(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public void Add(long CompanyId, Common.Data.PrototypeTypes prototypeTypes)
        {
            Entities.PrototypeTypes.Add(prototypeTypes);
        }
    }
}
