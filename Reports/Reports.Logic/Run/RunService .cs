using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Models;
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
        private readonly IServiceClient _client;
        private readonly IRedisCache _redisCache;

        public RunService(
            IReportsRepository reportsRepository,
            IServiceClient client,
            IRedisCache redisCache
        )
        {
            _reportsRepository = reportsRepository;
            _client = client;
            _redisCache = redisCache;
        }

        public async Task<ListResponse> BindAsync(DefaultIdentity identity, ListRequest request)
        {
            var response = new ListResponse();

            if (request.Source == null)
            {
                //initial load
                var reports = await _reportsRepository.GetReportsAsync(
                    identity.CompanyId,
                    identity.Code,
                    !identity.HasPermission("Reports", "Edit"),
                    null,
                    null,
                    null,
                    false, 
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

        public async Task DeleteAsync(DefaultIdentity identity, RunRequest request)
        {
            await _reportsRepository.DeleteRunAsync(identity, request.ID);
            var token = identity.Code + ":" + Guid.NewGuid();
            var user = new User
            {
                Company = identity.CompanyId,
                Code = identity.Code
            };
            await _redisCache.SetAsync($"Token:{token}", user, "Service", new TimeSpan(0, 1, 0));
            await _client.GetAsync($"Infrastructure/Notifications/Invalidate?token={token}&mask={(int)(Common.Logic.Models.Notifications.NotificationInvalidateMask.EnqueuedReportCount | Common.Logic.Models.Notifications.NotificationInvalidateMask.CompletedReportCount)}");
        }
    }
}
