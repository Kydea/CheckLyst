using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data.Infrastructure
{
    public interface IInfrastructureRepository: IRepository
    {
        Task<List<TResult>> GetDepartmentsAsync<TResult>(long Company, Expression<Func<Departments, TResult>> SelectExpression);
    }
}