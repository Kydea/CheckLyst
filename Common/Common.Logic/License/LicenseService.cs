using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.License
{
    public static class LicenseService
    {
        private static readonly string publicKey = @"<RSAKeyValue><Modulus>wF1kdL9Yj6KK2ySTnLnCrA90lH3oqLnBAQRtW6qPfjNizOkUA3BO7XJfe675XDFJhnllmRuOZOJoP5GjsLTjSkGyrFmwil/XYHLH1s7njaF6GulOqwx/HfQfMh9opaeEp4Jp4EAGJhnzRlYqtk9uwHnolft6RBAeyOltBA98eM2BLTdhXCHNo5RfFWICIciQZCK1UjiIpqQ92GC3xxA7thNcKH+pjtFhFOgT1GXAlxRs1UH7ZjNzKngkB8VoAiDxHyCA436O435jv8J8SWe0vOMC2chM0ZqkcOI2MTiF5NUAmg+vD5nRLYsvr8GDfys+lonXEh1qFkbmF63qv+zQ8Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public static string GeneratePrivateKey()
        {
            const int keySize = 2048;
            using (var rsa = new RSACryptoServiceProvider(keySize))
            {
                rsa.PersistKeyInCsp = false;
                return rsa.ToXmlString(true);
            }
        }

        public static string ExtractPublicKey(string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(privateKey);
                return rsa.ToXmlString(false);
            }
        }

        public static string Sign(LicensePayload payload, string privateKey)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)), signatureBytes;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(privateKey);
                signatureBytes = rsa.SignData(
                    payloadBytes,
                    CryptoConfig.MapNameToOID("SHA256")
                );
            }

            string signatureBase64 = Convert.ToBase64String(signatureBytes);
            return signatureBase64;
        }

        public static void VerifyLicense(LicensePayload payload, string signature)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)), signatureBytes;

            if (payload.Expiration.HasValue && payload.Expiration.Value < DateTimeOffset.UtcNow)
                throw new LicenseException("License expired");

            if (string.IsNullOrWhiteSpace(signature))
                throw new LicenseException("Empty signature");

            try
            {
                signatureBytes = Convert.FromBase64String(signature);
            }
            catch
            {
                throw new LicenseException("The signature is not a valid Base64 string");
            }

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(publicKey);
                if (!rsa.VerifyData(
                    payloadBytes,
                    CryptoConfig.MapNameToOID("SHA256"),
                    signatureBytes
                )) throw new LicenseException($"Invalid signature");
            }
        }
    }
}
