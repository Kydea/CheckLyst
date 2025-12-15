using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Cache
{
    public static class RedisCompressedFormat
    {
        private static readonly byte[] Magic = Encoding.ASCII.GetBytes("v1|"); //versione 1

        public static byte[] Wrap(byte[] gzipPayload)
        {
            var result = new byte[Magic.Length + gzipPayload.Length];
            Buffer.BlockCopy(Magic, 0, result, 0, Magic.Length);
            Buffer.BlockCopy(gzipPayload, 0, result, Magic.Length, gzipPayload.Length);
            return result;
        }

        public static bool TryExtractGZipPayload(byte[] data, out byte[] gzipPayload)
        {
            gzipPayload = null;

            if (data == null || data.Length < Magic.Length)
                return false;

            for (int i = 0; i < Magic.Length; i++)
                if (data[i] != Magic[i])
                    return false;

            var len = data.Length - Magic.Length;
            gzipPayload = new byte[len];
            Buffer.BlockCopy(data, Magic.Length, gzipPayload, 0, len);
            return true;
        }
    }
}