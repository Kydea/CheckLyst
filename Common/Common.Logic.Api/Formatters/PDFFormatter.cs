using CheckListManager.Common.Logic.Api.PDF;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.Formatters
{
    public class PDFFormatter : MediaTypeFormatter
    {
        public PDFFormatter(string FileName)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/pdf"));
            MediaTypeMappings.Add(new UriPathExtensionMapping("pdf", "application/pdf"));
            this.FileName = string.IsNullOrEmpty(FileName) || FileName.EndsWith(".pdf") ? FileName : FileName + ".pdf";
        }

        private string FileName { get; }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(PDFBase).IsAssignableFrom(type);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = FileName };

            base.SetDefaultContentHeaders(type, headers, mediaType);
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
        {
            var MemoryStream = new MemoryStream();
            await ((PDFBase)value).GenerateAsync(MemoryStream);

            var Bytes = MemoryStream.ToArray();
            stream.Write(Bytes, 0, Bytes.Length);
        }
    }
}
