using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.Prototypes.Data.Archiving
{
    public interface IArchivingRepository
    {
        IQueryable<Common.Data.Instances> GetInstances(int Company);
    }
}