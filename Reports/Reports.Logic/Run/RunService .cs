using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Reports.Data;
using CheckListManager.Reports.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Run
{
    public class RunService : IRunService
    {
        private readonly IReportsRepository _reportsRepository;

        public RunService(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
            _reportsRepository = reportsRepository;
        }

        public async Task<ListResponse> BindAsync(DefaultIdentity identity, ListRequest request)
        {
            var response = new ListResponse();

            if (request.Source == null)
            {
                //initial load
                var reports = await _reportsRepository.GetReportsAsync(
                    identity.CompanyId,
                    identity.HasPermission("Reports", "Edit") ? string.Empty : identity.Code,
                    null,
                    null,
                    null,
                    1,
                    0
                );
                response.Fields["Report"] = new Field
                {
                    Options = reports.Data.Select(d => new Option(d.ID.ToString(), d.Code)),
                    Value = request.Report?.ToString()
                };

                response.Fields["From"] = new Field { Value = request.From?.ToString(DateTimeOffsetHelper.DateFormat) };
                response.Fields["To"] = new Field { Value = request.To?.ToString(DateTimeOffsetHelper.DateFormat) };

            }

            return response;
        }

        public async Task<PagingResponse<List<RunRecord>>> GetDataAsync(IDefaultIdentity Identity, PagingRequest<ListRequest> Form)
        {
            var Result = await _reportsRepository.GetRunsAsync(
                Identity.CompanyId,
                Identity.Code,
                Form.Data.Report,
                Form.Data.From,
                Form.Data.To,
                Form.CurrentPage,
                Form.ItemsPerPage
            );

            var Response = new PagingResponse<List<RunRecord>>
            {
                Count = Result.Count,
                CurrentPage = Result.CurrentPage,
                ItemsPerPage = Result.ItemsPerPage,
                Data = Result.Data
            };

            Response.Messages.AddRange(Response.Messages);

            return Response;
        }

        public Task<long> CountAsync(DefaultIdentity identity) =>
            _reportsRepository.CountRunsAsync(identity);

        public Task DeleteAsync(DefaultIdentity identity, RunRequest request) =>
            _reportsRepository.DeleteRunAsync(identity, request.ID);
    }
}
