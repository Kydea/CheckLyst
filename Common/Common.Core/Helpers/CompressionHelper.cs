using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Helpers
{
    public class CompressionHelper
    {
        public static byte[] Compress(byte[] data, bool useGZipCompression)
        {
            CompressionLevel compressionLevel = CompressionLevel.Fastest;
            using (MemoryStream compressedStream = new MemoryStream())
            using (var zipStream = useGZipCompression ? (Stream)new GZipStream(compressedStream, compressionLevel) : new DeflateStream(compressedStream, compressionLevel))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Uncompress(byte[] data, bool useGZipCompression)
        {
            CompressionMode compressionMode = CompressionMode.Decompress;
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = useGZipCompression ? (Stream)new GZipStream(compressedStream, compressionMode) : new DeflateStream(compressedStream, compressionMode))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}
