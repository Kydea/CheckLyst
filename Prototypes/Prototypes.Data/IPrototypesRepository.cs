using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.Prototypes.Data
{
    public interface IPrototypesRepository : IRepository
    {
        Task<List<TResult>> GetPrototypesAsync<TResult, TKey>(long companyId, Expression<Func<PrototypeVersions, TResult>> select, Expression<Func<PrototypeVersions, bool>> filters, Expression<Func<PrototypeVersions, TKey>> order, int skip, int take);
    }
}