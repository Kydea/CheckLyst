using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.Licence
{
    public class LicenseService : ILicenseService
    {
        public LicenseService() 
        {
            //caricamento file licenza da settings company 1 e company specifica
        }

        public string GeneratePrivateKey()
        {
            const int keySize = 2048;
            using (var rsa = new RSACryptoServiceProvider(keySize))
            {
                rsa.PersistKeyInCsp = false;
                return rsa.ToXmlString(true);
            }
        }

        public string ExtractPublicKey(string privateKeyXml)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(privateKeyXml);
                return rsa.ToXmlString(false);
            }
        }

        public LicenseEnvelope GenerateLicence(LicensePayload payload, string privateKeyXml)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)), signatureBytes;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(privateKeyXml);
                signatureBytes = rsa.SignData(
                    payloadBytes,
                    CryptoConfig.MapNameToOID("SHA256")
                );
            }

            string signatureBase64 = Convert.ToBase64String(signatureBytes);
            return new LicenseEnvelope
            {
                Payload = payload,
                Signature = signatureBase64
            };
        }
    }
}
