using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.Formatters
{
    public class FileFormatter : MediaTypeFormatter
    {
        private readonly string _contentType;
        private readonly bool _serializeAsBase64;

        public FileFormatter(string contentType, bool serializeAsBase64 = false)
        {
            _contentType = contentType;
            _serializeAsBase64 = serializeAsBase64;
        }
        
        private static readonly Type TypeOfStream = typeof(Stream);
        
        public override bool CanReadType(Type type) => false;

        public override bool CanWriteType(Type type) => TypeOfStream.IsAssignableFrom(type);

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            headers.Add("X-Content-Type", _contentType);
            headers.ContentType = new MediaTypeHeaderValue(_serializeAsBase64 ? "text/plain" : _contentType);
            
            base.SetDefaultContentHeaders(type, headers, mediaType);
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            if (_serializeAsBase64)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ((Stream)value).CopyToAsync(memoryStream);
                    var serialized = Convert.ToBase64String(memoryStream.GetBuffer());
                    var bytes = Encoding.UTF8.GetBytes(serialized);

                    await writeStream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            else
            {
                await ((Stream)value).CopyToAsync(writeStream);
            }
        }
    }
}