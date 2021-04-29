using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq.Expressions;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Infrastructure.Data
{
	public interface IInfrastructureRepository : IRepository
	{

        Task<Companies> EditCompanyAsync(IDefaultIdentity identity, Companies company);
        Task<Companies> AddCompanyAsync(IDefaultIdentity identity, Companies company);
        Task DeleteCompanyAsync(IDefaultIdentity identity, long id);

        Task<Departments> EditDepartmentAsync(IDefaultIdentity identity, Departments department);
        Task<Departments> AddDepartmentAsync(IDefaultIdentity identity, Departments department);
        Task DeleteDepartmentAsync(IDefaultIdentity identity, long id);

        Task<PrototypeTypes> EditPrototypeTypeAsync(IDefaultIdentity identity, PrototypeTypes prototypeType);
        Task<PrototypeTypes> AddPrototypeTypeAsync(IDefaultIdentity identity, PrototypeTypes prototypeType);
        Task DeletePrototypeTypeAsync(IDefaultIdentity identity, long id);

        Task<FTPServers> EditFTPServerAsync(IDefaultIdentity identity, FTPServers ftpServer);
        Task<FTPServers> AddFTPServerAsync(IDefaultIdentity identity, FTPServers ftpServer);
        Task DeleteFTPServerAsync(IDefaultIdentity identity, long id);
        Task<FTPServers> GetFTPServerAsync(IDefaultIdentity identity, long id);

        Task<List<WorkFlows>> GetWorkFlows(long companyId);

        Task<PagingResponse<List<TResult>>> GetCompaniesAsync<TResult>(
            IDefaultIdentity identity,
            Expression<Func<Companies, TResult>> select, 
            string name, int currentPage, int itemsPerPage);

        Task<PagingResponse<List<TResult>>> GetDepartmentsAsync<TResult>(
            IDefaultIdentity identity,
            Expression<Func<Departments, TResult>> select,
            string name, int currentPage, int itemsPerPage);

        Task<PagingResponse<List<TResult>>> GetPrototypeTypesAsync<TResult>(
            IDefaultIdentity identity,
            Expression<Func<PrototypeTypes, TResult>> select,
            long? department, string name, string functionGroup, 
            int currentPage, int itemsPerPage,
            string Function = "View");

        Task<PagingResponse<List<TResult>>> GetFTPServersAsync<TResult>(
            IDefaultIdentity identity,
            Expression<Func<FTPServers, TResult>> select,
            string name, string host, int currentPage, int itemsPerPage);

        Task<List<string>> GetTimeZonesAsync();    }
}
