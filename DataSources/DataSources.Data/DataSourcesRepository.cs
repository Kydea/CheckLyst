using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data.Exceptions;

namespace CheckListManager.DataSources.Data
{
    public class DataSourcesRepository : Repository, IDataSourcesRepository
    {
        public DataSourcesRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public async Task<PagingResponse<List<TResult>>> GetDataSourcesAsync<TResult, TKey>(
            long companyId,
            Expression<Func<Common.Data.DataSources, TResult>> select,
            Expression<Func<Common.Data.DataSources, bool>> filters,
            Expression<Func<Common.Data.DataSources, TKey>> order, 
            int currentPage, 
            int itemsPerPage)
        {
            var Data = await Entities.DataSources
                .Where(filters)
                .OrderBy(order)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            int Count = await Entities.DataSources
                .Where(filters)
                .CountAsync();

            return new PagingResponse<List<TResult>>
            {
                Data = Data,
                Count = Count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }

        private async Task<Common.Data.DataSources> GetDataSourcesAsync(DefaultIdentity identity, string code)
        {
            var DataSource = await Entities.DataSources.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.Code == code);
            if (DataSource == null) throw new DatabaseException($"Data source {code} not found");
            return DataSource;
        }

        public async Task<Common.Data.DataSources> AddDataSourceAsync(DefaultIdentity identity, Common.Data.DataSources dataSources)
        {
            dataSources.Company = identity.CompanyId;
            dataSources.CreatedOn = DateTimeOffset.UtcNow;
            dataSources.CreatedBy = identity.Code;
            Entities.DataSources.Add(dataSources);
            await SaveChangesAsync();
            return dataSources;
        }

        public async Task<Common.Data.DataSources> EditDataSourceAsync(DefaultIdentity identity, Common.Data.DataSources dataSource)
        {
            var ExistingPrototypeType = await GetDataSourcesAsync(identity, dataSource.Code);
            ExistingPrototypeType.Code = dataSource.Code;
            ExistingPrototypeType.Description = dataSource.Description;
            ExistingPrototypeType.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingPrototypeType.ModifiedBy = identity.Code;
            await SaveChangesAsync();
            return ExistingPrototypeType;
        }

        public async Task DeleteDataSourceAsync(DefaultIdentity identity, string code)
        {
            Entities.DataSources.Remove(await GetDataSourcesAsync(identity, code));
            await SaveChangesAsync();
        }

    }
}
