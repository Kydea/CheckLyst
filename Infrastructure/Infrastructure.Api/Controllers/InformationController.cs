using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Infrastructure.Logic.Companies;
using CheckListManager.Infrastructure.Logic.Information;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("Information")]
    public class InformationController : BaseApiController
    {
        private readonly IConfigurationService _configurationService;

        public InformationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [HttpPost]
        [Route("Get")]
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                ServerTime = DateTimeOffset.UtcNow,
                Version = new ReleaseInfoMessage()
            });
        }

        [HttpPost]
        [Route("Clock")]
        public IHttpActionResult Clock()
        {
            return Ok(new ClockResponse
            {
                ServerTime = DateTimeOffset.UtcNow,
            });
        }

        [HttpGet]
        [Route("SetupQRCode")]
        [AllowAnonymous]
        public IHttpActionResult SetupQRCode()
        {
            var Url = _configurationService.WebApplicationUrl;
            var ProtocolTolen = Url != null && Url.StartsWith("http://") ? "http" : "https";
            Url = $"{Url?.Replace($"{ProtocolTolen}://", "checklist://")}/setup/{ProtocolTolen}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(Url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10);

            var Stream = new MemoryStream();
            qrCodeImage.Save(Stream, System.Drawing.Imaging.ImageFormat.Png);

            return Content(HttpStatusCode.OK, Stream, new FileFormatter("image/jpeg"));
        }
    }
}
