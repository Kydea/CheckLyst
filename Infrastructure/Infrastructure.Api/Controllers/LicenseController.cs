using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Licence;
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
        private ILicenseService _licenseService;

        public LicenseController(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        [Route("GeneratePrivateKey")]
        public IHttpActionResult GenerateLicenceKey()
        {
            string privateKeyXml = _licenseService.GeneratePrivateKey();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(privateKeyXml, Encoding.UTF8, "application/xml")
            };

            return ResponseMessage(response);
        }

        [Route("ExtractPublicKey")]
        public IHttpActionResult ExtractPublicKey(string privateKey)
        {
            string publicKeyXml = _licenseService.ExtractPublicKey(privateKey);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(publicKeyXml, Encoding.UTF8, "application/xml")
            };

            return ResponseMessage(response);
        }
    }
}
