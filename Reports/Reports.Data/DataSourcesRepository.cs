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

namespace CheckListManager.Reports.Data
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

        private async Task<Common.Data.DataSources> GetDataSourceAsync(IDefaultIdentity identity, string code)
        {
            var DataSource = await Entities.DataSources.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.Code == code);
            if (DataSource == null) throw new DatabaseException($"Data source {code} not found");
            return DataSource;
        }

        public async Task<Common.Data.DataSources> AddDataSourceAsync(IDefaultIdentity identity, Common.Data.DataSources dataSource)
        {
            dataSource.Company = identity.CompanyId;
            dataSource.CreatedOn = DateTimeOffset.UtcNow;
            dataSource.CreatedBy = identity.Code;
            Entities.DataSources.Add(dataSource);
            await SaveChangesAsync();
            return dataSource;
        }

        public async Task<Common.Data.DataSources> EditDataSourceAsync(IDefaultIdentity identity, Common.Data.DataSources dataSource)
        {
            var existingDataSource = await GetDataSourceAsync(identity, dataSource.Code);
            existingDataSource.Code = dataSource.Code;
            existingDataSource.Description = dataSource.Description;
            existingDataSource.Class = dataSource.Class;
            if (!string.IsNullOrEmpty(dataSource.Password))
                existingDataSource.Password = dataSource.Password;
            existingDataSource.Data = dataSource.Data;
            existingDataSource.ModifiedOn = DateTimeOffset.UtcNow;
            existingDataSource.ModifiedBy = identity.Code;
            await SaveChangesAsync();
            return existingDataSource;
        }

        public async Task DeleteDataSourceAsync(IDefaultIdentity identity, string code)
        {
            Entities.DataSources.Remove(await GetDataSourceAsync(identity, code));
            await SaveChangesAsync();
        }

    }
}
