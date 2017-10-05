using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data.Infrastructure
{
    public class InfrastructureRepository : Repository, IInfrastructureRepository
    {
        public InfrastructureRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        public Task<List<TResult>> GetDepartmentsAsync<TResult>(long Company, Expression<Func<Departments, TResult>> SelectExpression)
            => Entities.Departments.Where(d => d.Company == Company).Select(SelectExpression).ToListAsync(); //TODO Usare memory cache
    }
}
