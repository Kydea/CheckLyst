using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Logic.Viewer;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.DataEngine;
using System;
using CheckListManager.Common.Logic.Cache;
using Newtonsoft.Json;
using System.Collections.Generic;
using CheckListManager.Reports.Logic.Detail;
using System.Threading;
using System.Web;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using System.Net;
using CheckListManager.Common.Data;
using MigraDoc.DocumentObjectModel;
using CheckListManager.Common.Core.Web.Security;
using System.ServiceModel.Security;
using System.Configuration;

namespace CheckListManager.Reports.Api.Controllers
{
    [Authorize(Roles = "Reports#View")]
    [RoutePrefix("Viewer")]
    public class ViewerController : BaseApiController
    {
        private readonly IReportViewerService _reportViewerService;
        private readonly IConfigurationService _configurationService;
        private readonly ISettingService _settingService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;

        public ViewerController(IReportViewerService reportViewerService, IConfigurationService configurationService, ISettingService settingService, IRedisCache redisCache, ICommonRepository commonRepository)
        {
            _reportViewerService = reportViewerService;
            _configurationService = configurationService;
            _settingService = settingService;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
        }

        [HttpPost]
        [Route("LoadParameters")]
        public async Task<IHttpActionResult> LoadParameters(DetailRequest request)
        {
            return Ok(await _reportViewerService.LoadParameters(Identity, request.ID.Value));
        }

        [HttpPost]
        [Route("GetReportData")]
        public async Task<IHttpActionResult> GetReportData(ReportRequest request)
        {
            if (request.Enqueue)
            {
                await _reportViewerService.EnqueueReportRun(Identity, request);
                return Ok(new ReportResponse { Enqueued = true });
            }

            var requestToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
            var timeout = request.Run.HasValue ? double.MaxValue : _configurationService.ContextualReportTimeout;
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout)))
            using (var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestToken, timeoutCancellationTokenSource.Token))
                try
                {
                    return Ok(await _reportViewerService.GetReportData(Identity, request, linkedCancellationTokenSource.Token));
                }
                catch (OperationCanceledException) when (timeoutCancellationTokenSource.IsCancellationRequested && !requestToken.IsCancellationRequested)
                {
                    await _reportViewerService.EnqueueReportRun(Identity, request);
                    return Ok(new ReportResponse { Enqueued = true });
                }
        }

        [HttpPost]
        [Route("GetReportQuery")]
        [Authorize(Roles = "Reports#Edit")]
        public async Task<IHttpActionResult> GetReportQuery(ReportRequest request)
        {
            var cancellationToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
            return Ok(await _reportViewerService.GetReportQuery(Identity, request, cancellationToken));
        }

        [HttpPost]
        [Route("Download")]
        public async Task<IHttpActionResult> Download(ReportRequest request)
        {
            var Token = Identity.Code + ":" + Guid.NewGuid();
            var Company = await _commonRepository.GetCompanyAsync(Identity.CompanyId);
            request.CompanyId = Identity.CompanyId;
            request.CompanyName = Company.Name;
            request.TimeZone = Company.TimeZone;
            request.UserCode = Identity.Code;
            request.UserDescription = Identity.Description;
            await _redisCache.SetAsync($"Token:{Token}", request, "Download", new TimeSpan(0, 1, 0));
            return Ok(new { Token });
        }

        [HttpGet]
        [Route("Download")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Download([FromUri] string Token)
        {
            var data = await _redisCache.GetAsync<ReportRequest>($"Token:{Token}", "Download");
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var cancellationToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
            var identity = new ReportIdentity
            {
                CompanyId = data.CompanyId.Value,
                Code = data.UserCode,
                Description = data.UserDescription
            };
            var report = await _reportViewerService.GetReportData(identity, data, cancellationToken);
            var details = await _reportViewerService.LoadParameters(identity, report.ID);
            var decimalSeparator = await _settingService.DecimalSeparatorAsync(data.CompanyId.Value);
            var pdf = new ReportPDF(data.CompanyId.Value, data.CompanyName, data.TimeZone, decimalSeparator, _configurationService.PublicRepositoryUrl, _configurationService, data.FontSize, data.Format, data.Orientation, identity.Code, data, report, details, cancellationToken);
            return Content(HttpStatusCode.OK, pdf, new PDFFormatter("Report"));
        }


        public class ReportIdentity : IDefaultIdentity
        {
            public long CompanyId { get; set; }
            public string Code { get; set; }
            public string Description { get; set; }
            public bool HasPermission(string functionGroup, string function = "", long? department = null, long? prototypeType = null, string tag = null) => true;
        }
    }
}
