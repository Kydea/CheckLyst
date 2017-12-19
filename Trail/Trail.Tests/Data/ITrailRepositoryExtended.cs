using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Trail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Trail.Tests.Data
{
    public interface ITrailRepositoryExtended
    {
        Task<List<TrailMessage>> GetRecordsAsync();

        Task<TrailMessage> GetFirstAsync();

        Task<TrailMessage> GetFirstAsync(Func<TrailMessage, bool> filters);
    }
}
