using Newtonsoft.Json;
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
        private static readonly UTF8Encoding utf8Encoding = new UTF8Encoding(false);

        public static byte[] Compress(byte[] data, bool useGZipCompression)
        {
            CompressionLevel compressionLevel = CompressionLevel.Fastest;
            using (MemoryStream compressedStream = new MemoryStream())
            using (var zipStream = useGZipCompression ? (Stream)new GZipStream(compressedStream, compressionLevel, leaveOpen: true) : new DeflateStream(compressedStream, compressionLevel, leaveOpen: true))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Flush();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data, bool useGZipCompression)
        {
            CompressionMode compressionMode = CompressionMode.Decompress;
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = useGZipCompression ? (Stream)new GZipStream(compressedStream, compressionMode, leaveOpen: true) : new DeflateStream(compressedStream, compressionMode, leaveOpen: true))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static byte[] SerializeToCompressedBytes<T>(T value, bool useGZipCompression, params JsonConverter[] converters)
        {
            using (var output = new MemoryStream())
            {
                using (Stream zipStream = useGZipCompression ? (Stream)new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true) : new DeflateStream(output, CompressionLevel.Fastest, leaveOpen: true))
                using (var writer = new StreamWriter(zipStream, utf8Encoding, bufferSize: 64 * 1024, leaveOpen: true))
                using (var jsonWriter = new JsonTextWriter(writer) { CloseOutput = false })
                {
                    var serializer = CreateSerializer(converters);
                    serializer.Serialize(jsonWriter, value);
                    jsonWriter.Flush();
                    writer.Flush();
                }

                return output.ToArray();
            }
        }

        public static T DeserializeFromCompressedBytes<T>(byte[] compressed, bool useGZipCompression, params JsonConverter[] converters)
        {
            using (var input = new MemoryStream(compressed))
            using (Stream zipStream = useGZipCompression ? (Stream)new GZipStream(input, CompressionMode.Decompress, leaveOpen: true) : new DeflateStream(input, CompressionMode.Decompress, leaveOpen: true))
            using (var reader = new StreamReader(zipStream, utf8Encoding, detectEncodingFromByteOrderMarks: false, bufferSize: 64 * 1024, leaveOpen: true))
            using (var jsonReader = new JsonTextReader(reader) { CloseInput = false })
            {
                var serializer = CreateSerializer(converters);
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        private static JsonSerializer CreateSerializer(JsonConverter[] converters)
        {
            var serializer = JsonSerializer.CreateDefault();

            if (converters != null)
                foreach (var c in converters)
                    if (c != null)
                        serializer.Converters.Add(c);

            return serializer;
        }
    }
}