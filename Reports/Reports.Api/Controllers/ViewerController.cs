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

namespace CheckListManager.Reports.Api.Controllers
{
    [Authorize(Roles = "Reports#View")]
    [RoutePrefix("Viewer")]
    public class ViewerController : BaseApiController
    {
        private readonly IViewerEditorService _reportViewerService;
        private readonly IConfigurationService _configurationService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;

        public ViewerController(IViewerEditorService reportViewerService, IConfigurationService configurationService, IRedisCache redisCache, ICommonRepository commonRepository)
        {
            _reportViewerService = reportViewerService;
            _configurationService = configurationService;
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
            try
            {
                var cancellationToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
                return Ok(await _reportViewerService.GetReportData(Identity, request, cancellationToken));
            }
            catch (Exception Exception)
            {
                var Response = new ResponseMessage<Exception> { Data = Exception };
                Response.Messages.AddRange(BuildErrorMessage(Exception));
                return new ErrorResult(Request, Response);
            }
        }
        
        [HttpPost]
        [Route("GetReportQuery")]
        [Authorize(Roles = "Reports#Edit")]
        public async Task<IHttpActionResult> GetReportQuery(ReportRequest request)
        {
            try
            {
                var cancellationToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
                return Ok(await _reportViewerService.GetReportQuery(Identity, request, cancellationToken));
            }
            catch (Exception Exception)
            {
                var Response = new ResponseMessage<Exception> { Data = Exception };
                Response.Messages.AddRange(BuildErrorMessage(Exception));
                return new ErrorResult(Request, Response);
            }
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
            var report = await _reportViewerService.GetReportData(Identity, data, cancellationToken);
            var pdf = new ReportPDF(data.CompanyId.Value, data.CompanyName, data.TimeZone, _configurationService.PublicRepositoryUrl, _configurationService, report);
            return Content(HttpStatusCode.OK, pdf, new PDFFormatter("Report"));
        }


    }
}
