using CheckListManager.Common.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.Licence
{
    public interface ILicenseService: ISingletonService
    {
        string GeneratePrivateKey();
        string ExtractPublicKey(string privateKeyXml);
        LicenseEnvelope GenerateLicence(LicensePayload payload, string privateKeyXml);
    }
}
