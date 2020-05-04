using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Infrastructure.Logic.Companies;
using CheckListManager.Infrastructure.Logic.Information;
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
            var Encoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.M);
            var Url = _configurationService.WebApplicationUrl;
            var ProtocolTolen = Url != null && Url.StartsWith("http://") ? "http" : "https";
            Url = $"{Url?.Replace($"{ProtocolTolen}://", "checklist://")}/setup/{ProtocolTolen}";
            var Code = Encoder.Encode(Url);
            const int PIXEL_SIZE = 8;
            var Image = new Bitmap(Code.Matrix.Width * PIXEL_SIZE, Code.Matrix.Height * PIXEL_SIZE);
            for (int X = 0; X < Code.Matrix.Width; ++X)
                for (int Y = 0; Y < Code.Matrix.Height; ++Y)
                    for (int XOffset = 0; XOffset < PIXEL_SIZE; ++XOffset)
                        for (int YOffset = 0; YOffset < PIXEL_SIZE; ++YOffset)
                            Image.SetPixel(X * PIXEL_SIZE + XOffset, Y * PIXEL_SIZE + YOffset, Code.Matrix.InternalArray[X, Y] ? Color.Black : Color.White);

            using (var Stream = new MemoryStream())
            {
                Image.Save(Stream, System.Drawing.Imaging.ImageFormat.Png);
                return Content(HttpStatusCode.OK, Stream, new FileFormatter("image/jpeg"));
            }

        }
    }
}
