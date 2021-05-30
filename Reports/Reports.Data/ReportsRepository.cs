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
using Dapper;
using System.Data;

namespace CheckListManager.Reports.Data
{
    public class ReportsRepository : Repository, IReportsRepository
    {
        public ReportsRepository(RelationalDatabaseEntities entities) : base(entities)
		{
        }

        public async Task<List<ReportSources>> GetReportSourcesAsync() =>
            await Entities.ReportSources.OrderBy(rs => rs.Code).ToListAsync();

        public async Task<PagingResponse<List<Common.Data.Reports>>> GetReportsAsync(
            long company,
            string user,
            string code,
            string description,
            int currentPage, 
            int itemsPerPage)
        {
            IEnumerable<string> userGroups = null;
            if (string.IsNullOrEmpty(user))
                user = string.Empty;
            else
                userGroups = (await Entities.Users.SingleAsync(u => u.Company == company && u.Code == user)).UserGroups.Select(ug => ug.Group);
            if (code == null) code = string.Empty;
            if (description == null) description = string.Empty;
            var Query = Entities.Reports
                .Where(r =>
                    user == string.Empty || !r.Restricted || r.Groups.Any(g => userGroups.Contains(g.Code))
                    && code == string.Empty || r.Code.ToUpper().Contains(code)
                    && description == string.Empty || r.Description != null && r.Description.ToUpper().Contains(description)
                );

            int Count = await Query.CountAsync();

            var Data = await Query
                .OrderBy(r => r.Code)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();


            return new PagingResponse<List<Common.Data.Reports>>
            {
                Data = Data,
                Count = Count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }

        public async Task<Common.Data.Reports> GetReportAsync(long company, string code)
        {
            var Report = await Entities.Reports.SingleOrDefaultAsync(d => d.Company == company && d.Code == code);
            if (Report == null) throw new DatabaseException($"Report {code} not found");
            return Report;
        }

        public async Task AddReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups)
        {
            report.Company = identity.CompanyId;
            report.Groups = Entities.Groups.Where(g => g.Company == identity.CompanyId && groups.Contains(g.Code)).ToList();
            report.CreatedOn = DateTimeOffset.UtcNow;
            report.CreatedBy = identity.Code;
            Entities.Reports.Add(report);
            await SaveChangesAsync();
        }

        public async Task<Common.Data.Reports> EditReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups)
        {
            var existingReport = await GetReportAsync(identity.CompanyId, report.Code);
            existingReport.Code = report.Code;
            existingReport.Description = report.Description;
            existingReport.ReportSource = report.ReportSource;
            existingReport.Query = report.Query;
            existingReport.InstancesRangeRestricted = report.InstancesRangeRestricted;
            existingReport.InstancesIDAlias = report.InstancesIDAlias;
            existingReport.PrototypesRangeRestricted = report.PrototypesRangeRestricted;
            existingReport.PrototypesIDAlias = report.PrototypesIDAlias;
            existingReport.Online = report.Online;
            existingReport.Restricted = report.Restricted;
            existingReport.ModifiedOn = DateTimeOffset.UtcNow;
            existingReport.ModifiedBy = identity.Code;
            Entities.ReportColumns.RemoveRange(existingReport.ReportColumns);
            Entities.ReportClauses.RemoveRange(existingReport.ReportClauses);
            foreach(var group in existingReport.Groups.ToList())
                existingReport.Groups.Remove(group);
            existingReport.ReportColumns = report.ReportColumns;
            existingReport.ReportClauses = report.ReportClauses;
            existingReport.Groups = Entities.Groups.Where(g => g.Company == identity.CompanyId && groups.Contains(g.Code)).ToList();
            await SaveChangesAsync();
            return existingReport;
        }

        public async Task DeleteReportAsync(IDefaultIdentity identity, string code)
        {
            Entities.Reports.Remove(await GetReportAsync(identity.CompanyId, code));
            await SaveChangesAsync();
        }
    }
}
