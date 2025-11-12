using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Licence;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Infrastructure.Logic.Companies;
using CheckListManager.Infrastructure.Logic.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("License")]
    public class LicenseController : BaseApiController
    {
        [HttpGet]
        [Route("GeneratePrivateKey")]
        [AllowAnonymous]
        public IHttpActionResult GeneratePrivateKey()
        {
            string privateKeyXml = LicenseService.GeneratePrivateKey();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(privateKeyXml, Encoding.UTF8, "application/xml")
            };

            return ResponseMessage(response);
        }

        [Route("GenerateSecrets")]
        public IHttpActionResult GenerateSecrets(Message<LicenseManagerRequest> request)
        {
            var response = new LicenseManagerResponse
            {
                PublicKey = LicenseService.ExtractPublicKey(request.Data.PrivateKey),
                Signature = LicenseService.Sign(request.Data, request.Data.PrivateKey)
            };

            return Json(response);
        }
    }
}
