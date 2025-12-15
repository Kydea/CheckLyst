using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Core.Exceptions;
using System.Data;
using CheckListManager.Reports.Data.Models;
using System.Threading;

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
            int? id,
            string code,
            string description,
            int currentPage,
            int itemsPerPage)
        {
            if (code == null) code = string.Empty;
            if (description == null) description = string.Empty;
            var Query = Entities.Reports
                .Where(r =>
                    r.Company == company
                    && (id == null || r.ID == id)
                    && (code == string.Empty || r.Code.ToUpper().Contains(code.ToUpper()))
                    && (description == string.Empty || r.Description != null && r.Description.ToUpper().Contains(description.ToUpper()))
                );

            if (!string.IsNullOrEmpty(user))
            {
                var userGroups = (await Entities.Users.SingleAsync(u => u.Company == company && u.Code == user)).UserGroups.Select(ug => ug.Group);
                Query = Query.Where(r => (!r.Restricted || r.Groups.Any(g => userGroups.Contains(g.Code))) && r.Online);
            }

            int Count = await Query.CountAsync();

            Query = Query
                .OrderBy(r => r.ID)
                .Skip((currentPage - 1) * itemsPerPage);

            if (itemsPerPage > 0)
                Query = Query.Take(itemsPerPage);

            var Data = await Query.ToListAsync();
            return new PagingResponse<List<Common.Data.Reports>>
            {
                Data = Data,
                Count = Count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }

        public async Task<Common.Data.Reports> GetReportAsync(long company, long id, string user)
        {
            IQueryable<Common.Data.Reports> Query = Entities.Reports;
            if (!string.IsNullOrEmpty(user))
            {
                var userGroups = (await Entities.Users.SingleAsync(u => u.Company == company && u.Code == user)).UserGroups.Select(ug => ug.Group);
                Query = Query.Where(r => (!r.Restricted || r.Groups.Any(g => userGroups.Contains(g.Code))) && r.Online);
            }

            var Report = await Query.SingleOrDefaultAsync(d => d.Company == company && d.ID == id) ?? throw new DatabaseException($"Report {id} not found");
            return Report;
        }

        public async Task AddReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups)
        {
            report.Company = identity.CompanyId;
            if (groups != null && groups.Any())
                report.Groups = Entities.Groups.Where(g => g.Company == identity.CompanyId && groups.Contains(g.Code)).ToList();
            report.CreatedOn = DateTimeOffset.UtcNow;
            report.CreatedBy = identity.Code;
            Entities.Reports.Add(report);
            await SaveChangesAsync();
        }

        public async Task<Common.Data.Reports> EditReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups)
        {
            var existingReport = await GetReportAsync(identity.CompanyId, report.ID, null);
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
            foreach (var group in existingReport.Groups.ToList())
                existingReport.Groups.Remove(group);
            existingReport.ReportColumns = report.ReportColumns;
            existingReport.ReportClauses = report.ReportClauses;
            if (groups != null && groups.Any())
                existingReport.Groups = Entities.Groups.Where(g => g.Company == identity.CompanyId && groups.Contains(g.Code)).ToList();
            await SaveChangesAsync();
            return existingReport;
        }

        public async Task DeleteReportAsync(IDefaultIdentity identity, long id)
        {
            Entities.Reports.Remove(await GetReportAsync(identity.CompanyId, id, null));
            await SaveChangesAsync();
        }

        public async Task EnqueueReportRunAsync(IDefaultIdentity identity, long report, string parameters)
        {
            Entities.ReportRuns.Add(new ReportRuns
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Report = report,
                Parameters = parameters,
                Expiration = DateTimeOffset.UtcNow.AddDays(30),
                CreatedOn = DateTimeOffset.UtcNow
            });
            await SaveChangesAsync();
        }

        public async Task<long> CountRunsAsync(IDefaultIdentity identity) =>
            await Entities.ReportRuns.Where(r => r.Company == identity.CompanyId && r.User == identity.Code && r.CompletedOn.HasValue).CountAsync();

        public async Task<PagingResponse<List<RunRecord>>> GetRunsAsync(long companyId, string user, long? report, DateTime? from, DateTime? to, int currentPage, int itemsPerPage)
        {
            var query = Entities.ReportRuns.Where(r =>
                r.Company == companyId
                && r.User == user
                && (report == null || r.Report == report)
                && (from == null || r.CreatedOn >= from)
                && (to == null || r.CreatedOn <= to)
            );

            var count = await query.CountAsync();

            var data = await query
                .OrderBy(r => r.ID)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .Select(r => new RunRecord
                {
                    ID = r.ID,
                    ReportID = r.Report,
                    ReportCode = r.Reports.Code,
                    Expiration = r.Expiration,
                    CreatedOn = r.CreatedOn,
                    CompletedOn = r.CompletedOn
                })
                .ToListAsync();

            return new PagingResponse<List<RunRecord>>
            {
                Data = data,
                Count = count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }

        public Task<List<ReportRuns>> GetPendingRunsAsync() =>
            Entities.ReportRuns.Where(r => !r.CompletedOn.HasValue).OrderBy(r => r.CreatedOn).ToListAsync();

        public Task<ReportRuns> GetRunAsync(long id) =>
            Entities.ReportRuns.SingleOrDefaultAsync(r => r.ID == id) ?? throw new DatabaseException($"Report run {id} not found");

        public async Task DeleteRunAsync(DefaultIdentity identity, long iD)
        {
            var run = await Entities.ReportRuns.SingleOrDefaultAsync(r => r.Company == identity.CompanyId && r.User == identity.Code && r.ID == iD) ?? throw new DatabaseException($"Report run {iD} not found for user {identity.Code} and company {identity.CompanyId}");
            Entities.ReportRuns.Remove(run);
            await SaveChangesAsync();
        }

        public Task CompleteReportRunAsync(long id, byte[] data, Exception exception, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var run = await GetRunAsync(id);
                run.Data = data;
                run.CompletedOn = DateTimeOffset.UtcNow;
                run.Error = null;

                while (exception != null)
                {
                    run.Error += exception.Message;
                    exception = exception.InnerException;
                    if (exception != null) run.Error += " | ";
                }

                await SaveChangesAsync();
            }, cancellationToken);
        }

        public async Task DeleteExpiredRunsAsync(CancellationToken cancellationToken)
        {
            var expiredRuns = await Entities.ReportRuns.Where(r => r.Expiration <= DateTimeOffset.UtcNow).ToListAsync(cancellationToken);
            Entities.ReportRuns.RemoveRange(expiredRuns);
            await SaveChangesAsync();
        }
    }
}
