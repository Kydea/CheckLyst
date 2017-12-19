using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Trail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;

namespace CheckListManager.Trail.Tests.Data
{
    public class TrailRepositoryExtended : Repository, ITrailRepositoryExtended
    {
        public TrailRepositoryExtended(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public Task<List<TrailMessage>> GetRecordsAsync()
            => Entities.Records
                .Select(p => new TrailMessage()
                {
                    Id = p.ID,
                    Date = p.Date,
                    User = p.User,
                    Action = p.Action,
                    CheckList = p.CheckList,
                    Information = p.Information
                }).ToListAsync();

        public async Task<TrailMessage> GetFirstAsync()
            => (await GetRecordsAsync()).First();

        public async Task<TrailMessage> GetFirstAsync(Func<TrailMessage, bool> filters)
            => (await GetRecordsAsync()).Where(filters).First();
            
    }
}
