using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.DataSources.Data
{
    public interface IDataSourcesRepository : IRepository
    {
        Task<PagingResponse<List<TResult>>> GetDataSourcesAsync<TResult, TKey>(long companyId, Expression<Func<Common.Data.DataSources, TResult>> select, Expression<Func<Common.Data.DataSources, bool>> filters, Expression<Func<Common.Data.DataSources, TKey>> order, int currentPage, int itemsPerPage);
        Task<Common.Data.DataSources> AddDataSourceAsync(DefaultIdentity identity, Common.Data.DataSources dataSource);
        Task<Common.Data.DataSources> EditDataSourceAsync(DefaultIdentity identity, Common.Data.DataSources dataSource);
        Task DeleteDataSourceAsync(DefaultIdentity identity, string code);
    }
}